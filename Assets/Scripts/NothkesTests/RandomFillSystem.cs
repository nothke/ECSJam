using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Jobs;
using Random = Unity.Mathematics.Random;

struct RandomFillData : IComponentData
{
    public float a;
}

public class RandomFillSystem : JobComponentSystem
{
    struct Job : IJobProcessComponentData<RandomFillData>
    {
        Random rand;

        public Job(Random rand)
        {
            this.rand = rand;
        }

        public void Execute(ref RandomFillData data)
        {
            data.a = rand.NextFloat();
        }
    }

    Random systemRand;

    protected override void OnStartRunning()
    {
        systemRand = new Random(10);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // this is just to advance the random state between frames
        systemRand.NextFloat();

        Job job = new Job(systemRand);
        return job.Schedule(this, inputDeps);
    }
}
