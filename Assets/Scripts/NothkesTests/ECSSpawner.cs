using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;

public abstract class ECSSpawner : MonoBehaviour
{
    public float count = 10000;

    public abstract void OnStart();

    static EntityManager manager;

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
    }
}
