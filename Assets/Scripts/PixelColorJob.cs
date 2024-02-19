using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System;

public class PixelColorJob : MonoBehaviour
{
    private NativeArray<Color> pixels;
    private NativeArray<Vector2> redPixelPositions;

    public bool[,] redPixleList;
    private int batchSize; // Adjust the batch size as needed

    public void RedPixelSearch(Texture2D textureToEdit)
    {
        int maxParallelJobs = Environment.ProcessorCount;
        batchSize = (textureToEdit.width * textureToEdit.height) / maxParallelJobs;
        pixels = new NativeArray<Color>(textureToEdit.GetPixels(), Allocator.TempJob);
        redPixelPositions = new NativeArray<Vector2>(textureToEdit.width * textureToEdit.height, Allocator.TempJob);
        PixelJob job = new PixelJob
        {
            pixels = pixels,
            redPixelPositions = redPixelPositions,
            textureWidth = textureToEdit.width,
            batchSize = batchSize
        };
        JobHandle handle = job.Schedule(maxParallelJobs, 1);

        redPixleList = new bool[textureToEdit.width,textureToEdit.height];

        for (int i = 0; i < pixels.Length - 1; i++)
        {
            int x = i % textureToEdit.width;
            int y = i / textureToEdit.width;

            redPixleList[x,y] = (pixels[i] == Color.red);
        }
        redPixelPositions.Dispose();
        textureToEdit.SetPixels(pixels.ToArray());

        pixels.Dispose();
        textureToEdit.Apply();
    }

    struct PixelJob : IJobParallelFor
    {
        public NativeArray<Color> pixels;
        public NativeArray<Vector2> redPixelPositions;
        public int textureWidth;
        public int batchSize;

        public void Execute(int batchIndex)
        {
            int startPixelIndex = batchIndex * batchSize;
            int endPixelIndex = Mathf.Min((batchIndex + 1) * batchSize, pixels.Length);

            for (int pixelIndex = startPixelIndex; pixelIndex < endPixelIndex; pixelIndex++)
            {
                int x = pixelIndex % textureWidth;
                int y = pixelIndex / textureWidth;

                if (pixels[pixelIndex].r > 0.7f && pixels[pixelIndex].g < 0.5f && pixels[pixelIndex].b < 0.5f)
                {
                    pixels[pixelIndex] = Color.red;
                    redPixelPositions[pixelIndex] = new Vector2Int(x, y);
                }
                else
                {
                    pixels[pixelIndex] = Color.black;
                }
            }
        }
    }
}