using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Renamed the class to avoid conflict with another 'Noise' class in the global namespace
public static class NoiseGenerator
{
    // Renamed the method to avoid conflict with another 'PerlinNoise' method in the same type
    public static float GeneratePerlinNoise(float x, float z)
    {
        float scale = 0.8f; // Smaller scale = finer details
        float offset = 10f; // Offset to differentiate from other noise layers
        int octaves = 10;   // Number of layers of noisef
        float persistence = 1.0f; // Amplitude multiplier for each octave
        float lacunarity = 1.0f; // Frequency multiplier for each octave

        float amplitude = 2.0f;
        float frequency = 1.0f;
        float noiseValue = 0.0f;

        for (int i = 0; i < octaves; i++)
        {
            float xPos = (x + 0.1f) / 32 * scale * frequency + offset;
            float zPos = (z + 0.1f) / 32 * scale * frequency + offset;
            noiseValue += Mathf.PerlinNoise(xPos, zPos) * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return noiseValue;
    }



    public static float GenerateBiomeMask(float x, float z)
    {
        float scale = 0.45f; // Bigger scale = larger biomes
        float offset = 100f; // Different offset to get a different noise pattern

        float xPos = (x + 0.1f) / 32 * scale + offset;
        float zPos = (z + 0.1f) / 32 * scale + offset;
        return Mathf.PerlinNoise(xPos, zPos);
    }
}
