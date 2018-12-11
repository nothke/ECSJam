using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;

public class FillerSpawner : MonoBehaviour
{
    const int count = 10000;

    void Start()
    {
        EntityManager manager = World.Active.GetOrCreateManager<EntityManager>();

        var archetype = manager.CreateArchetype(
            typeof(RandomFillData)
            );

        Entity entity = manager.CreateEntity(archetype);

        for (int i = 0; i < count; i++)
        {
            manager.CreateEntity(archetype);
        }

        Debug.Log(entity.Index);
    }
}
