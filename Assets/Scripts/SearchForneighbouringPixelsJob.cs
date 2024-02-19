using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.Diagnostics;

using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UIElements;

public class SearchForneighbouringPixelsJob : MonoBehaviour
{
    public NativeArray<Vector2> redPixelsArray;
    public NativeArray<Vector2> addetPixle;

    NativeArray<SearchJob> jobs = new NativeArray<SearchJob>(3, Allocator.Temp);
    NativeArray<JobHandle> handles = new NativeArray<JobHandle>(3, Allocator.Temp);

    public SearchForneighbouringPixelsJob(List<Vector2> redPixlsList)
    {
        redPixelsArray = new NativeArray<Vector2>(redPixlsList.Count, Allocator.TempJob);
        redPixelsArray.CopyFrom(redPixlsList.ToArray());}

    public Vector2 SearchForRedPixels(Vector2 startPoint)
    {
        Stopwatch timer = Stopwatch.StartNew();
        for (int i = 0; i < 3; i++)
        {
            var job = new SearchJob
            {
                startPoint = startPoint,
                redPixels = redPixelsArray,
                addetPixle = addetPixle,
                direction = i
            };
            jobs[i] = job;
            handles[i] = job.Schedule();
        }
        JobHandle.CompleteAll(handles);
        for (int i = 0; i < addetPixle.Length; i++)
        {
            Vector2 temp = addetPixle[i];
            if (startPoint.x < addetPixle[i].x)
            {
                OnDestroy();
                return temp;
            }
        }
        for (int i = 0; i < addetPixle.Length; i++)
        {
            Vector2 temp = addetPixle[i];
            if (startPoint.y < addetPixle[i].y)
            {
                OnDestroy();
                return temp;
            }
        }
        for (int i = 0; i < addetPixle.Length; i++)
        {
            Vector2 temp = addetPixle[i];
            if (startPoint.y > addetPixle[i].y)
            {
                OnDestroy();
                return temp;
            }
        }
        OnDestroy();
        timer.Stop();
        Debug.Log("Time ende0: " + timer.ElapsedMilliseconds);
        return Vector2.zero;
    }

    private void OnDestroy()
    {
        redPixelsArray.Dispose();
        addetPixle.Dispose();
        jobs.Dispose();
        handles.Dispose();
    }

    struct SearchJob : IJob
    {
        public Vector2 startPoint;
        public NativeArray<Vector2> redPixels;
        public NativeArray<Vector2> addetPixle;
        public int direction;
        public void Execute()
        {
            int dx = 0, dy = 0;
            Vector2 foundPixel = Vector2.zero;
            switch (direction)
            {
                case 0: dx = 1; break; // right 
                case 1: dy = 1; break; // up
                case 2: dy = -1; break; // down
            }
            if (dx == 1)
            {
                for (int i = 0; i < 50; i++)
                {
                    foundPixel = startPoint + new Vector2(dx + i, dy);
                    if (redPixels.Contains(foundPixel))
                    {
                        addetPixle.Append(foundPixel);
                        break;
                    }
                }
            }
            else if (dy == 1)
            {
                for (int i = 0; i < 50; i++)
                {
                    foundPixel = startPoint + new Vector2(dx, dy + i);
                    if (redPixels.Contains(foundPixel))
                    {
                        addetPixle.Append(foundPixel);
                        break;
                    }
                }
            }
            else if (dy == -1)
            {
                for (int i = 0; i < 50; i++)
                {
                    foundPixel = startPoint + new Vector2(dx, dy - i);
                    if (redPixels.Contains(foundPixel))
                    {
                        addetPixle.Append(foundPixel);
                        break;
                    }
                }
            }
        }
    }
}
