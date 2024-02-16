using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.Diagnostics;

using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System;
using System.Linq;

public class SearchForneighbouringPixelsJob : MonoBehaviour
{
    public NativeArray<Vector2> redPixelsArray;
    public NativeArray<Vector2> addetPixle;

    NativeArray<SearchJob> jobs = new NativeArray<SearchJob>(3, Allocator.Temp);
    NativeArray<JobHandle> handles = new NativeArray<JobHandle>(3, Allocator.Temp);


    Vector2 SearchForRedPixels(Vector2 startPoint,List<Vector2> redPixlsList)
    {

        for (int i = 0; i < 3; i++)
        {
            var job = new SearchJob
            {
                startPoint = startPoint,
                redPixels = new NativeArray<Vector2>(redPixelsArray, Allocator.TempJob),
                addetPixle = addetPixle,
                direction = i
            };
            jobs[i] = job;
            handles[i] = job.Schedule();
        }
        JobHandle.CompleteAll(handles);
        for (int i = 0; i < addetPixle.Length; i++)
        {
            if (startPoint.x < addetPixle[i].x)
            {
                return addetPixle[i];
            } else if (startPoint.y < addetPixle[i].y)
            {
                return addetPixle[i];
            } else if (startPoint.y > addetPixle[i].y)
            {
                return addetPixle[i];
            }
        }
        redPixelsArray.Dispose();
        addetPixle.Dispose();
        jobs.Dispose();
        handles.Dispose();
        return Vector2.zero;
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
            switch (direction)
            {
                case 0: dx = 1; break; // right 
                case 1: dy = 1; break; // up
                case 2: dy = -1; break; // down
            }

            for (int i = 1; i < 50; i++)
            {
                Vector2 foundPixel = startPoint + new Vector2(dx * i, dy * i);

                if (redPixels.Contains(foundPixel))
                {
                    addetPixle.Append(foundPixel);
                    return; 
                }
            }
        }
    }
}
