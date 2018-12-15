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

public class MicrobeMovementSystem : JobComponentSystem
{
    const int COLLISION_TILES_SIZE = 200;
    const int COLLISION_FORCES_PER_TILE = 5;

    const int ARRAY_SIZE = COLLISION_TILES_SIZE * COLLISION_TILES_SIZE * COLLISION_FORCES_PER_TILE;

    private float3 centerPos = new float3(0, 0, 0);
    private float3 direction = new float3(1, 0, 0);
    private float globalOffset = 0;
    private bool entitiesLoaded;

    [NativeDisableParallelForRestriction]
    private NativeArray<int> gridIndexArray;

    private NativeArray<Entity> entities;
    private NativeArray<float3> entityPositionsArray;

    ComponentDataArray<Position> positions;

    /*
    struct ArrayClearJob : IJobParallelFor
    {
        public NativeArray<int> indexArray;

        public void Execute(int i)
        {
            indexArray[i] = -1;
        }
    }*/

    // -WithEntity gives you Entity too, so we can use it's Index
    struct FillDataArrayJob : IJobProcessComponentDataWithEntity<Position>
    {
        [NativeDisableParallelForRestriction] // this allows us to write to the same data in parallel
        public NativeArray<int> gridIndexData;

        public void Execute(Entity entity, int index, ref Position data)
        {
            int i = CoordsToOuterIndex((int)data.Value.x, (int)data.Value.z);

            bool isWithinGrid = i >= 0 && i < gridIndexData.Length;

            if (!isWithinGrid) return;

            for (int gridIndex = i; gridIndex < i + COLLISION_FORCES_PER_TILE; gridIndex++)
            {
                if (gridIndexData[gridIndex] == -1) continue;

                gridIndexData[gridIndex] = entity.Index;
                // TODO: Add position too
            }
        }
    }

    [BurstCompile(Accuracy = Accuracy.Low)]
    private struct PositionJob : IJobProcessComponentData<MovementData, Position> // Scale
    {
        [Unity.Collections.ReadOnly]
        private NativeArray<int> gridIndexData;
        [Unity.Collections.ReadOnly]
        private NativeArray<float3> entityPositions;

        public PositionJob(NativeArray<int> gridData, NativeArray<float3> ent)
        {
            gridIndexData = gridData;
            entityPositions = ent;
        }

        public void Execute(ref MovementData data, ref Position position) // , ref Scale scale
        {

            data.PreviousPosition.x = position.Value.x;
            data.PreviousPosition.y = position.Value.z;

            // Match circle position
            /*
            int distance = 2 + (data.Index / 50) * 2;

            float circleOffset = globalOffset * (distance % 4 == 0 ? -1 : 1);
            var dir = Quaternion.Euler(0, data.Index * 7.2f + circleOffset, 0) * direction;
            var oldY = position.Value.y;
            position.Value = math.lerp(position.Value, centerPos + new float3(dir.x, dir.y, dir.z) * distance, 0.1f);
            position.Value.y = oldY;*/

            // Hacky death
            /*
            if (data.life == 0)
            {
                position.Value = new float3(100000, 100000, 100000);
            }*/

            int playAreaSize = 400;
            float xin = playAreaSize * 0.5f - position.Value.x;
            float zin = playAreaSize * 0.5f - position.Value.z;

            // Pull towards center

            data.Velocity += new float2(
                xin * 0.0005f,
                zin * 0.0005f);

            // Noise
            /*
            var noiz = -0.5f + noise.cellular(new float2(xin * 0.01f, zin * 0.01f));
            var noiz2 = -0.5f + noise.snoise(new float2(xin * 0.1f, zin * 0.1f));
            data.Velocity += noiz * 0.2f + noiz2 * 0.2f;*/

            // Collision

            int thisGridIndex = CoordsToOuterIndex((int)position.Value.x, (int)position.Value.z);
            bool isWithinGrid = thisGridIndex >= 0 && thisGridIndex < gridIndexData.Length;

            if (isWithinGrid)
            {
                for (int i = thisGridIndex; i < thisGridIndex + COLLISION_FORCES_PER_TILE; i++)
                {
                    int otherEntityIndex = gridIndexData[i];

                    if (otherEntityIndex == data.Index) continue; // skip if same
                    if (otherEntityIndex == -1) continue; // skip if empty

                    data.Velocity = -data.Velocity * 100;
                    position.Value.x = 100000;

                    /*
                    var entPos = entityPositions[otherEntityIndex];

                    var diff = new float2(
                        position.Value.x - entPos.x,
                        position.Value.z - entPos.z);

                    var sqrDistance = math.lengthsq(diff);

                    if (sqrDistance < 9f)
                    {
                        //data.life--;
                        //data.Velocity += -diff;
                        data.Velocity = -data.Velocity;
                        break;
                    }*/
                    //data.Force += new float2(position.Value.x - entPos.x, position.Value.z - entPos.z);
                }

                //if (applyForce) data.Force = new float2(.5f, .5f);

                //data.Force = math.normalize(data.Force);
            }

            // Update positions
            position.Value.x += data.Velocity.x;
            position.Value.z += data.Velocity.y;
            data.Velocity *= .8f;
        }

    }

    private static int CoordsToOuterIndex(int x, int z)
    {
        //int numberOfForcesPerCell = 10;
        //int playAreaSize = 400;
        return (x * COLLISION_TILES_SIZE + z) * COLLISION_FORCES_PER_TILE;
    }

    protected override void OnStartRunning()
    {
        entityPositionsArray = new NativeArray<float3>(100000, Allocator.Persistent);

        gridIndexArray = new NativeArray<int>(ARRAY_SIZE, Allocator.Persistent);

        entitiesLoaded = true;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // reset values to -1
        var clearArrayJob = new MemsetNativeArray<int> // MemsetNativeArray job assigns the same value to the whole array
        {
            Source = gridIndexArray,
            Value = -1
        };

        var clearArrayJobHandle = clearArrayJob.Schedule(ARRAY_SIZE, 64, inputDeps); // not sure what the 64 innerloop batch count means

        // fill index array job
        var fillJob = new FillDataArrayJob()
        {
            gridIndexData = gridIndexArray
        };

        var fillJobHandle = fillJob.Schedule(this, clearArrayJobHandle); // makes sure the clearArrayJob is complete

        #region legacy slow
        /*
        entities = MicrobeSpawner.entityArray;
        centerPos = Object.FindObjectOfType<InputComponent>().transform.position;
        globalOffset += .2f; // WTF IS THIS

        // update avoidance data and calculate force

        // THIS MAKES THE SYSTEM SLOW!!!
        
        for (int i = 0; i < MicrobeSpawner.total; i++)
        {
            MovementData indexForcePrevPos = EntityManager.GetComponentData<MovementData>(entities[i]);
            Position position = EntityManager.GetComponentData<Position>(entities[i]);
            entityPositionsArray[i] = position.Value;

            // remove old position from grid
            int outerIndex = CoordsToOuterIndex(
                (int)indexForcePrevPos.PreviousPosition.x,
                (int)indexForcePrevPos.PreviousPosition.y);

            if (outerIndex >= 0 && outerIndex < gridIndexArray.Length)
            {
                for (int innerIndex = outerIndex; innerIndex < outerIndex + COLLISION_FORCES_PER_TILE; innerIndex++)
                {
                    if (gridIndexArray[innerIndex] == indexForcePrevPos.Index)
                    {
                        gridIndexArray[innerIndex] = 0;
                    }
                }
            }

            // add new position to grid
            outerIndex = CoordsToOuterIndex((int)position.Value.x, (int)position.Value.z);
            if (outerIndex >= 0 && outerIndex < gridIndexArray.Length)
            {
                for (int innerIndex = outerIndex; innerIndex < outerIndex + COLLISION_FORCES_PER_TILE; innerIndex++)
                {
                    if (gridIndexArray[innerIndex] == 0)
                    {
                        gridIndexArray[innerIndex] = indexForcePrevPos.Index;
                    }
                }
            }
        }*/
        #endregion

        // movement job
        var movementJob = new PositionJob(gridIndexArray, entityPositionsArray); 
        var movementJobHandle = movementJob.Schedule(this, fillJobHandle);  // makes sure the fillJob is complete

        return movementJobHandle;

        //return movementJob.Schedule(this, inputDeps);
    }

    protected override void OnStopRunning()
    {
        gridIndexArray.Dispose();
        entities.Dispose();
        entityPositionsArray.Dispose();
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
public struct MovementData : IComponentData
{
    public int Index;
    public float2 Velocity;
    public float2 PreviousPosition;
    public int life;
}
