using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class AvoidanceSystem : JobComponentSystem
{
    
    private struct AvoidanceJob : IJobProcessComponentData<PositioningData, AvoidanceData, Position>
    {
        //public NativeHashMap<int, NativeArray<int>> avoidanceMap;
        
        
        public void Execute(ref PositioningData indexData, ref AvoidanceData data, ref Position position)
        {
//            int hash = Hash((int) position.Value.x, (int) position.Value.z);
//            NativeArray<int> listOfCellIndices;
//            if (!avoidanceMap.TryGetValue(hash, out listOfCellIndices))
//            {
//                avoidanceMap.TryAdd(hash, new NativeArray<int>());
//            }
//
//            avoidanceMap.TryGetValue(hash, out listOfCellIndices);
//            int i = 0;
//            while (listOfCellIndices[i] != 0)
//            {
//                i++;
//                // infinite loops ahoy
//            }
//
//            listOfCellIndices[i] = indexData.Index;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new AvoidanceJob();
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

public struct AvoidanceData : IComponentData
{
    public float2 Force;
}

