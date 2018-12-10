using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    static MeshInstanceRenderer bacteriaRenderer;
    static MeshInstanceRenderer antibodyRenderer;

    public static EntityManager _entityManager;
    public static EntityArchetype _cellArchetype;
    public static NativeArray<Entity> entityArray;

    //static float size = 50;
    const float scale = 1;

    public static int total;
    
    public enum EntityType { Bacteria, Antibody };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        // Create the archetype
        _entityManager = World.Active.GetOrCreateManager<EntityManager>();
        entityArray = new NativeArray<Entity>(100000, Allocator.Persistent);
        _cellArchetype = _entityManager.CreateArchetype(
            typeof(Position),
            typeof(Rotation),
            typeof(Scale),
            typeof(EnemyData),
            typeof(PositioningData),
            //typeof(MitosisData),
            //typeof(MeshInstanceRenderer),
            typeof(LocalToWorld));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeWithScene()
    {
        bacteriaRenderer = GameObject.Find("BacteriaRenderer").GetComponent<MeshInstanceRendererComponent>().Value;
        antibodyRenderer = GameObject.Find("BacteriaRenderer").GetComponent<MeshInstanceRendererComponent>().Value;

        // spawn on start
        int playAreaSize = 400;
        float size = playAreaSize;

        for (int i = 0; i < 10000; i++)
        {
            Vector3 pos = new Vector2(size * 0.5f, size * 0.5f) + UnityEngine.Random.insideUnitCircle * size * 0.5f;
            pos.z = pos.y;
            pos.y = 0.5f + UnityEngine.Random.value * 2;

            // in sphere
            //Vector3 v = new Vector3(size * 0.5f, size * 0.5f, size * 0.5f) + UnityEngine.Random.insideUnitSphere * size;

            SpawnEntity(i, pos, EntityType.Bacteria);
        }
    }

    public static void SpawnEntity(Vector3 position, EntityType type)
    {
        int index = total;

        SpawnEntity(index, position, type);
    }

    public static void SpawnEntity(int index, Vector3 position, EntityType type)
    {
        Entity enemyEntity = _entityManager.CreateEntity(_cellArchetype);
        _entityManager.SetComponentData(enemyEntity, new Position { Value = position });
        _entityManager.SetComponentData(enemyEntity, new Rotation { Value = quaternion.identity });
        _entityManager.SetComponentData(enemyEntity, new EnemyData() { Speed = 0, SwayAngle = 0, SwayDirection = 1 });
        _entityManager.SetComponentData(enemyEntity, new PositioningData() { Index = index, life = 1 });
        _entityManager.SetComponentData(enemyEntity, new Scale() { Value = new float3(scale, scale, scale) });

        MeshInstanceRenderer renderer;
        switch (type)
        {
            case EntityType.Bacteria:
                renderer = bacteriaRenderer; break;
            case EntityType.Antibody:
                renderer = antibodyRenderer; break;
            default:
                renderer = bacteriaRenderer; break;
        }

        _entityManager.AddSharedComponentData(enemyEntity, renderer);

        //_entityManager.SetComponentData(enemyEntity, new Heading{ Value = new float3(1,0,0)});
        //_entityManager.SetComponentData(enemyEntity, new MitosisData(){ A = 0 });

        entityArray[total] = enemyEntity;
        total++;
    }

    #region Utils

    public static float3 ReturnRandomPositionOffset(float maxVal)
    {
        float x = ReturnRandomFloat(maxVal);
        float z = ReturnRandomFloat(maxVal);
        return new float3(x, 0f, z);

    }

    public static float ReturnRandomFloat(float maxVal)
    {

        float val = UnityEngine.Random.Range(-maxVal, maxVal);
        return val;
    }
    #endregion
}
