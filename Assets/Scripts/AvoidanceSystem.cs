using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class AvoidanceSystem : JobComponentSystem
{
    
    private struct PositionJob : IJobProcessComponentData<AvoidanceData, Position>
    {
        public void Execute(ref AvoidanceData data, ref Position position)
        {
            
        }
        
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new PositionJob();
        return job.Schedule(this, inputDeps);
    }
}

public struct AvoidanceData : IComponentData
{
    public float2 Force;
}
