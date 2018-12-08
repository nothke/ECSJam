using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class EnemyPositioningSystem : JobComponentSystem
{
    private static float3 centerPos = new float3(0, 0, 0);
    private static float3 direction = new float3(1, 0, 0);
    private static float globalOffset = 0;
    
    private struct PositionJob : IJobProcessComponentData<PositioningData, Position>
    {
        public float DeltaTime;

        public void Execute(ref PositioningData data, ref Position position)
        {
            int distance = 2 + (data.Index / 50) * 2;

            float circleOffset = globalOffset * (distance % 4 == 0 ? -1 : 1);
            var dir = Quaternion.Euler(0, data.Index * 7.2f + circleOffset, 0) * direction;
            var tempY = position.Value.y;
            position.Value = math.lerp(position.Value, centerPos + new float3(dir.x, dir.y, dir.z) * distance, 0.01f);
            position.Value.y = tempY;//asd
        }
        
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        centerPos = GameObject.FindObjectOfType<InputComponent>().transform.position;
        globalOffset += .2f;
        var job = new PositionJob
        {
            DeltaTime = Time.deltaTime
        };
        return job.Schedule(this, inputDeps);
    }
}

public struct PositioningData : IComponentData
{
    public int Index;
}
