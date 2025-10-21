Shader "Unlit/WaterShader"  
{  
    Properties  
    {  
        _Color ("Color", Color) = (0,0,1,1)  
        _MainTex ("Texture", 2D) = "white" {}  
        _WaveSpeed ("Wave Speed", Float) = 1.0  
        _WaveHeight ("Wave Height", Float) = 0.1
        _WaveX ("Wave X Offset", Float) = 0.2
        _WaveZ ("Wave Z Offset", Float) = 0.6
        _Delta ("Delta", Float) = 0.01
        _Cube ("Skybox", CUBE) = "" {}
        _ReflectionStrength ("Reflection Strength", Range(0,1)) = 0.5
        _SpecularAtten ("Attenuation", Float) = 1.0
        _Shininess ("Shininess", Float) = 10
    }  
    SubShader  
    {  
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }  
        LOD 100  

        Blend SrcAlpha OneMinusSrcAlpha  

        Pass  
        {  
            CGPROGRAM  
            #pragma vertex vert  
            #pragma fragment frag  
            #pragma multi_compile_fog  

            #include "UnityCG.cginc"  

            struct appdata  
            {  
                float4 vertex : POSITION;  
                float2 uv : TEXCOORD0;
            };  

            struct v2f  
            {  
                float2 uv : TEXCOORD0;  
                float3 normal : TEXCOORD1;
                float3 viewDir: TEXCOORD2;
                float4 vertex : SV_POSITION;
                UNITY_FOG_COORDS(3)
            };  

            sampler2D _MainTex;  
            float4 _MainTex_ST;  
            fixed4 _Color;  
            float _WaveSpeed;  
            float _WaveHeight;
            float _WaveX;
            float _WaveZ;
            float _Delta;
            samplerCUBE _Cube;
            float _ReflectionStrength;
            float _SpecularAtten;
            float _Shininess;

            float FractalBrownianMotionSine(float x, float z) {
                float total = 0.0;
                float amplitude = 1.0;
                float frequency = 1.0;

                for (int i = 0; i < 4; i++) { // 5 octaves
                    // Alternate between sine and cosine for variety
                    if (fmod(i, 2) != 0) {
                        total += cos(_Time.y * _WaveSpeed * frequency + x * (_WaveX + i * 0.1) * frequency + z * (_WaveZ - i * 0.1) * frequency) * amplitude;
                    } else {
                        total += sin(_Time.y * _WaveSpeed * frequency + x * (_WaveX - i * 0.1) * frequency + z * (_WaveZ + i * 0.1) * frequency) * amplitude;
                    }

                    // Add a second wave layer with a different direction
                    total += sin(_Time.y * (_WaveSpeed * 0.5) * frequency + x * (_WaveZ + i * 0.2) * frequency + z * (_WaveX - i * 0.2) * frequency) * (amplitude * 0.5);

                    amplitude *= 0.5; // Reduce amplitude for each octave
                    frequency *= 2.0; // Double frequency for each octave
                }

                return total;
            }

            float GetWaveHeight(float x, float z) {
                return FractalBrownianMotionSine(x, z) * _WaveHeight - 0.04;
            }

            v2f vert (appdata v)  
            {  
                v2f o;

                // Calculate wave displacement
                float p = GetWaveHeight(v.vertex.x, v.vertex.z);
                v.vertex.y += p;

                // Calculate displaced positions for normal calculation
                float deltaX = v.vertex.x + _Delta;
                float deltaZ = v.vertex.z + _Delta;

                float3 originalPos = float3(v.vertex.x, p, v.vertex.z);
                float3 displacedX = float3(deltaX, GetWaveHeight(deltaX, v.vertex.z), v.vertex.z);
                float3 displacedZ = float3(v.vertex.x, GetWaveHeight(v.vertex.x, deltaZ), deltaZ);

                // Calculate tangents
                float3 tangentX = displacedX - originalPos;
                float3 tangentZ = displacedZ - originalPos;

                // Calculate normal using cross product
                float3 normal = normalize(cross(tangentZ, tangentX));

                // Calculate view direction
                float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
                float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);

                // Output data
                o.normal = normal;
                o.viewDir = viewDir;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);  
                return o;  
            }
            
            
            fixed4 frag (v2f i) : SV_Target  
            {  
    
                // Calculate reflection vector
                float3 refl = reflect(-normalize(i.viewDir), normalize(i.normal));
                fixed4 reflectedColor = texCUBE(_Cube, refl);
    
                // Calculate refraction vector
                float3 refr = refract(-normalize(i.viewDir), normalize(i.normal), 1 / 1.33); // Assuming water's IOR is 1.33
                fixed4 refractedColor = texCUBE(_Cube, refr);

                // Calculate Fresnel effect
                float fresnel = 6.0 * pow(1.0 - saturate(dot(normalize(i.viewDir), normalize(i.normal))), 4.0);

                // LightDir based on the scenes directional light
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                // Calculete the diffuse
                float diffuse = max(dot(normalize(i.normal), lightDir), 0.0);

                // Calculate halfway vector for Blinn-Phong specular
                float3 halfVector = normalize(lightDir + normalize(i.viewDir));
                float specular = pow(max(dot(normalize(i.normal), halfVector), 0.0), _Shininess);
                float3 specularRefl = _SpecularAtten * specular;

                // Blend reflection and refraction based on Fresnel
                fixed4 reflectionRefractionBlend = lerp(refractedColor, reflectedColor, fresnel);

                // Blend base texture and reflection/refraction mix
                fixed4 col = lerp(_Color, reflectionRefractionBlend, _ReflectionStrength);
                col.rgb *= diffuse + specularRefl;

                // UNITY_APPLY_FOG(i.fogCoord, col);  
                return col;  
            }
            ENDCG  
        }  
    }  
}