﻿using System.Collections;
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
using Random = UnityEngine.Random;

public class EnemyPositioningSystem : JobComponentSystem
{
    public static int playAreaSize = 256;
    private static int numberOfForcesPerCell = 10;
    private static float3 centerPos = new float3(0, 0, 0);
    private static float3 direction = new float3(1, 0, 0);
    private static float globalOffset = 0;
    private static NativeArray<int> gridIndexData;
    private NativeArray<Entity> entities;
    private NativeArray<float3> entityPositions;
    private static bool entitiesLoaded;


    private struct PositionJob : IJobProcessComponentData<PositioningData, Position> // Scale
    {
        private readonly NativeArray<int> gridIndexData;
        private readonly NativeArray<float3> entityPositions;

        public PositionJob(NativeArray<int> gridData, NativeArray<float3> ent)
        {
            gridIndexData = gridData;
            entityPositions = ent;
        }

        public void Execute(ref PositioningData data, ref Position position) // , ref Scale scale
        {
            data.PreviousPosition.x = position.Value.x;
            data.PreviousPosition.y = position.Value.z;

            /*
            int distance = 2 + (data.Index / 50) * 2;

            float circleOffset = globalOffset * (distance % 4 == 0 ? -1 : 1);
            var dir = Quaternion.Euler(0, data.Index * 7.2f + circleOffset, 0) * direction;
            var oldY = position.Value.y;
            position.Value = math.lerp(position.Value, centerPos + new float3(dir.x, dir.y, dir.z) * distance, 0.1f);
            position.Value.y = oldY;*/

            // apply avoidance force
            position.Value.x += data.Force.x;
            position.Value.z += data.Force.y;
            data.Force *= .8f;

            //scale.Value.x = 1;

            if (data.life == 0)
            {
                position.Value = new float3(100000, 100000, 100000);
            }

            // noise
            //var noiz = noise.cellular(new float2(position.Value.x * 0.01f, position.Value.z * 0.01f));
            //data.Force = (-2 + noiz);
            //data.Force = noiz;

            float xin = playAreaSize / 2 - position.Value.x;
            float zin = playAreaSize / 2 - position.Value.z;

            //data.Force = new float2(
                //xin * 0.01f,
                //zin * 0.01f);

            var noiz = noise.cellular(new float2(xin * 0.01f, zin * 0.01f));
            data.Force = -0.5f + noiz;

            // force
            int outerIndex = CoordsToOuterIndex((int)position.Value.x, (int)position.Value.z);
            if (outerIndex >= 0 && outerIndex < gridIndexData.Length)
            {
                for (int i = outerIndex; i < outerIndex + numberOfForcesPerCell; i++)
                {
                    int entityIndex = gridIndexData[i];
                    if (entityIndex != data.Index && entityIndex != 0)
                    {
                        var entPos = entityPositions[entityIndex];
                        if (math.distance(entPos, position.Value) < 5f)
                        {
                            //data.life--;
                            data.Force = new float2(position.Value.x - entPos.x, position.Value.z - entPos.z) / 2;
                            break;
                        }
                        //data.Force += new float2(position.Value.x - entPos.x, position.Value.z - entPos.z);
                    }
                }

                //if (applyForce) data.Force = new float2(.5f, .5f);

                //data.Force = math.normalize(data.Force);
            }
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
            entityPositions = new NativeArray<float3>(100000, Allocator.Persistent);
            entitiesLoaded = true;
        }
        entities = EnemySpawner.entityArray;
        centerPos = GameObject.FindObjectOfType<InputComponent>().transform.position;
        globalOffset += .2f;

        // update avoidance data and calculate force;
        for (int i = 0; i < EnemySpawner.total; i++)
        {
            PositioningData indexForcePrevPos = EntityManager.GetComponentData<PositioningData>(entities[i]);
            Position position = EntityManager.GetComponentData<Position>(entities[i]);
            entityPositions[i] = position.Value;

            // remove old position from grid
            int outerIndex = CoordsToOuterIndex((int)indexForcePrevPos.PreviousPosition.x,
                (int)indexForcePrevPos.PreviousPosition.y);
            if (outerIndex >= 0 && outerIndex < gridIndexData.Length)
            {
                for (int innerIndex = outerIndex; innerIndex < outerIndex + numberOfForcesPerCell; innerIndex++)
                {
                    if (gridIndexData[innerIndex] == indexForcePrevPos.Index)
                    {
                        gridIndexData[innerIndex] = 0;
                    }
                }
            }

            // add new position to grid
            outerIndex = CoordsToOuterIndex((int)position.Value.x, (int)position.Value.z);
            if (outerIndex >= 0 && outerIndex < gridIndexData.Length)
            {
                for (int innerIndex = outerIndex; innerIndex < outerIndex + numberOfForcesPerCell; innerIndex++)
                {
                    if (gridIndexData[innerIndex] == 0)
                    {
                        gridIndexData[innerIndex] = indexForcePrevPos.Index;
                    }
                }
            }
        }

        // start job
        var job = new PositionJob(gridIndexData, entityPositions);
        return job.Schedule(this, inputDeps);
    }

    protected override void OnStopRunning()
    {
        gridIndexData.Dispose();
        entities.Dispose();
        entityPositions.Dispose();
        EnemySpawner.entityArray.Dispose();
        base.OnStopRunning();
        Debug.Log("STAO JE JEBEM GA");
    }

    public static int Hash(int x, int z)
    {
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
    public float2 PreviousPosition;
    public int life;
}
