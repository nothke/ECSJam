using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class EnemySwaySystem : JobComponentSystem
{

    private static float swaySpeed = 2f;
    private static float swayLimit = .6f; //rads (rad ~ 57 deg)
    
    private struct SwayJob : IJobProcessComponentData<EnemyData, Rotation>
    {
        public float DeltaTime;

        public void Execute(ref EnemyData data, ref Rotation rotation)
        {
            if ((data.SwayAngle >= swayLimit && data.SwayDirection > 0) || (data.SwayAngle <= -swayLimit && data.SwayDirection < 0) )
            {
                data.SwayDirection *= -1;
            }
            data.SwayAngle += swaySpeed * data.SwayDirection * DeltaTime;
            
            rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.forward(rotation.Value), swaySpeed * data.SwayDirection * DeltaTime));
        }
        
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new SwayJob
        {
            DeltaTime = Time.deltaTime
        };
        return job.Schedule(this, inputDeps);
    }
}
