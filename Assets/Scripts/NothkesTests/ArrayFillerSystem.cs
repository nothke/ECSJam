using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Jobs;
using Random = Unity.Mathematics.Random;
using Unity.Collections;

struct ArrayFillData : IComponentData
{
    public float a;
}

public class ArrayFillSystem : JobComponentSystem
{
    struct Job : IJobProcessComponentData<ArrayFillData>
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<int> array;
        public Random rand;

        public void Execute(ref ArrayFillData data)
        {
            data.a = rand.NextFloat();

            array[0] += 1;
        }
    }

    Random systemRand;

    [NativeDisableParallelForRestriction]
    NativeArray<int> array;

    protected override void OnStartRunning()
    {
        systemRand = new Random(10);
        array = new NativeArray<int>(10, Allocator.Persistent);
    }

    protected override void OnStopRunning()
    {
        lastJob.Complete();
        array.Dispose();
    }

    JobHandle lastJob;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // this is just to advance the random state between frames
        systemRand.NextFloat();

        Job job = new Job()
        {
            rand = systemRand,
            array = array
        };

        Debug.Log(array[0]);

        lastJob = job.Schedule(this, inputDeps);
        return lastJob;
    }
}