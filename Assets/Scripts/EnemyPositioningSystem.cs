using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class EnemyPositioningSystem : JobComponentSystem
{
    private static int playAreaSize = 50;
    private static int numberOfForcesPerCell = 10;
    private static float3 centerPos = new float3(0, 0, 0);
    private static float3 direction = new float3(1, 0, 0);
    private static float globalOffset = 0;
    private static NativeArray<int> gridIndexData;
    private NativeArray<Entity> entities;
    private NativeArray<float3> entityPositions;
    private static bool entitiesLoaded;
    
    
    private struct PositionJob : IJobProcessComponentData<PositioningData, Position>
    {
        private readonly NativeArray<int> gridIndexData;
        private NativeArray<float3> entityPositions;

        public PositionJob(NativeArray<int> gridData, NativeArray<float3> ent)
        {
            gridIndexData = gridData;
            entityPositions = ent;
        }
        
        public void Execute(ref PositioningData data, ref Position position)
        {
            
            int distance = 2 + (data.Index / 50) * 2;

            float circleOffset = globalOffset * (distance % 4 == 0 ? -1 : 1);
            var dir = Quaternion.Euler(0, data.Index * 7.2f + circleOffset, 0) * direction;
            var oldPos = position.Value;
            position.Value = math.lerp(position.Value, centerPos + new float3(dir.x, dir.y, dir.z) * distance, 0.01f);
            position.Value.y = oldPos.y;
            
            // apply avoidance force
//            position.Value.x += data.Force.x / 50;
//            position.Value.z += data.Force.y / 50;
//            data.Force *= .9f;
//            
//            // force
//            int outerIndex = CoordsToOuterIndex((int)position.Value.x, (int)position.Value.z);
//            if (outerIndex >= 0 && outerIndex < gridIndexData.Length)
//            {
//                for (int i = outerIndex; i < outerIndex + numberOfForcesPerCell; i++)
//                {
//                    int entityIndex = gridIndexData[i];
//                    if (entityIndex != data.Index && entityIndex != 0)
//                    {
//                        var entPos = entityPositions[entityIndex];
//                        data.Force += new float2(position.Value.x - entPos.x, position.Value.z - entPos.z);
//                    }
//                }
//            }
        }
        
        
    }

    private static int CoordsToOuterIndex(int x, int z)
    {
        return x * playAreaSize * numberOfForcesPerCell + z * numberOfForcesPerCell;
    }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        gridIndexData = new NativeArray<int>(playAreaSize * playAreaSize * numberOfForcesPerCell, Allocator.Persistent);
        entitiesLoaded = false;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (!entitiesLoaded)
        {
            entities = EntityManager.GetAllEntities(Allocator.Persistent);
            entityPositions = new NativeArray<float3>(entities.Length, Allocator.Persistent);
            entitiesLoaded = true;
        }
        centerPos = GameObject.FindObjectOfType<InputComponent>().transform.position;
        globalOffset += .2f;
        
        // update avoidance data and calculate force;
        for (int i = 0; i < entities.Length; i++)
        {
            if (entities[i].Index < 2)
            {
                continue;
            }
            PositioningData indexAndForce = EntityManager.GetComponentData<PositioningData>(entities[i]);
            Position position = EntityManager.GetComponentData<Position>(entities[i]);
            entityPositions[i] = position.Value;
        
        
            int outerIndex = CoordsToOuterIndex((int)position.Value.x, (int)position.Value.z);
            if (outerIndex >= 0 && outerIndex < gridIndexData.Length)
            {
                for (int innerIndex = outerIndex; innerIndex < outerIndex + numberOfForcesPerCell; innerIndex++)
                {
                    if (gridIndexData[innerIndex] == 0)
                    {
                        gridIndexData[innerIndex] = indexAndForce.Index;
                    }
                }
            }
        }
        
        // start job
        var job = new PositionJob(gridIndexData, entityPositions);
        return job.Schedule(this, inputDeps);
    }
    
    public static int Hash(int x, int z) {
        int hash = x;
        hash = (hash * 397) ^ z;
        hash += hash << 3;
        hash ^= hash >> 11;
        hash += hash << 15;
        return hash;
    }
}

public struct PositioningData : IComponentData
{
    public int Index;
    public float2 Force;
}
