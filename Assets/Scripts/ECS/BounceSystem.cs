using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Transforms;

public class BounceSystem : JobComponentSystem
{

    private static float gravity = 1;
    
    private struct BounceJob : IJobProcessComponentData<SwayData, Position>
    {
        public float DeltaTime;

        public void Execute(ref SwayData data, ref Position position)
        {
            position.Value.y += data.Speed * DeltaTime;
            data.Speed -= gravity;

            if (position.Value.y <= .5f)
            {
                data.Speed = 5f;
            }
        }
        
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new BounceJob
        {
            DeltaTime = Time.deltaTime
        };
        return job.Schedule(this, inputDeps);
    }
}
