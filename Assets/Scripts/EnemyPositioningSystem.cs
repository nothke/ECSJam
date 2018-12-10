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
using Random = UnityEngine.Random;
using Unity.Burst;

public class EnemyPositioningSystem : JobComponentSystem
{
    //public int playAreaSize = 256;
    private int numberOfForcesPerCell = 5;
    private float3 centerPos = new float3(0, 0, 0);
    private float3 direction = new float3(1, 0, 0);
    private float globalOffset = 0;
    private NativeArray<int> gridIndexData;
    private bool entitiesLoaded;

    private NativeArray<Entity> entities;
    private NativeArray<float3> entityPositions;

    [BurstCompile(Accuracy = Accuracy.Low)]
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
            position.Value.x += data.Velocity.x;
            position.Value.z += data.Velocity.y;
            data.Velocity *= .8f;

            //scale.Value.x = 1;

            /*
            if (data.life == 0)
            {
                position.Value = new float3(100000, 100000, 100000);
            }*/

            // noise
            //var noiz = noise.cellular(new float2(position.Value.x * 0.01f, position.Value.z * 0.01f));
            //data.Force = (-2 + noiz);
            //data.Force = noiz;

            int playAreaSize = 400;
            float xin = playAreaSize / 2 - position.Value.x;
            float zin = playAreaSize / 2 - position.Value.z;

            // SABIJAC
            data.Velocity += new float2(
                xin * 0.0005f,
                zin * 0.0005f);

            var noiz = -0.5f + noise.cellular(new float2(xin * 0.01f, zin * 0.01f));
            var noiz2 = -0.5f + noise.snoise(new float2(xin * 0.1f, zin * 0.1f));
            data.Velocity += noiz * 0.2f + noiz2*0.2f;

            int numberOfForcesPerCell = 10;

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
                        if (math.distance(entPos, position.Value) < 3f)
                        {
                            //data.life--;
                            data.Velocity = new float2(position.Value.x - entPos.x, position.Value.z - entPos.z) / 2;
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
        int numberOfForcesPerCell = 10;
        int playAreaSize = 400;
        return x * playAreaSize * numberOfForcesPerCell + z * numberOfForcesPerCell;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (!entitiesLoaded)
        {
            entityPositions = new NativeArray<float3>(100000, Allocator.Persistent);
            entitiesLoaded = true;
            int playAreaSize = 400;
            gridIndexData = new NativeArray<int>(playAreaSize * playAreaSize * numberOfForcesPerCell, Allocator.Persistent);
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
        //EnemySpawner.entityArray.Dispose();
        base.OnStopRunning();
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

[BurstCompile]
public struct PositioningData : IComponentData
{
    public int Index;
    public float2 Velocity;
    public float2 PreviousPosition;
    public int life;
}
