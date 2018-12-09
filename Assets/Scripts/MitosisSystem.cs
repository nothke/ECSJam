using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class MitosisSystem : JobComponentSystem
{
    private static EntityCommandBuffer commandBuffer;
    private static float totalCooldown = 3;
    private static float currentCooldown = 10;
    
    
    private struct PositionJob : IJobProcessComponentData<MitosisData, Scale, PositioningData>
    {
        public float CDPercentage;

        public void Execute(ref MitosisData data, ref Scale scale, ref PositioningData posData)
        {
            scale.Value.x = .5f + CDPercentage / 2;
            scale.Value.z = 1 + (1 - CDPercentage);
            
            //commandBuffer.CreateEntity(EnemySpawner._cellArchetype);
        }
        
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        currentCooldown -= Time.deltaTime;
        
        var job = new PositionJob
        {
            CDPercentage = currentCooldown/totalCooldown
        };
        if (currentCooldown <= 0)
        {
            currentCooldown = totalCooldown;
            //NativeArray<Entity> allEntities = EntityManager.GetAllEntities(Allocator.TempJob);
            int step = (int)EnemySpawner.total / 500;
            for (int i = 0; i < EnemySpawner.total; i += step)
            {
                //Entity e = EntityManager.CreateEntity(EnemySpawner._cellArchetype);

                Debug.LogError("there is " + EnemySpawner.total + " and index is " + i);
                EnemySpawner.SpawnEnemyAtPosition(EntityManager.GetComponentData<Position>(EnemySpawner.entityArray[i]).Value +
                                                  EnemySpawner.ReturnRandomPositionOffset(1f));
                //if (EntityManager.HasComponent<MitosisData>(e))
                //{
                //    EntityManager.SetComponentData(e, new Position() {Value = EntityManager.GetComponentData<Position>(allEntities[i]).Value});
                //}
            }

        }
        return job.Schedule(this, inputDeps);
    }

    protected override void OnStopRunning()
    {
        base.OnStopRunning();
    }
}

public struct MitosisData : IComponentData
{
    public int A;
}
