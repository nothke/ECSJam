using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using System;
using UnityEngine;

public class MicrobeSpawner
{
    const int spawnCount = 20000;

    static MeshInstanceRenderer bacteriaRenderer;
    static MeshInstanceRenderer antibodyRenderer;

    public static EntityManager _entityManager;
    public static EntityArchetype _cellArchetype;
    public static NativeArray<Entity> entityArray;

    //static float size = 50;
    const float scale = 1;

    public const float worldSize = 400;

    public static int total;

    public enum EntityType { Bacteria, Antibody };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        Debug.Log("Initializing MicrobeSpawner");

        // Create the archetype
        _entityManager = World.Active.GetOrCreateManager<EntityManager>();
        entityArray = new NativeArray<Entity>(100000, Allocator.Persistent);
        _cellArchetype = _entityManager.CreateArchetype(
            typeof(Position),
            typeof(Rotation),
            typeof(Scale),
            typeof(SwayData),
            typeof(PositioningData),
            //typeof(MitosisData),
            //typeof(MeshInstanceRenderer),
            typeof(LocalToWorld));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeWithScene()
    {
        bacteriaRenderer = GameObject.Find("BacteriaRenderer").GetComponent<MeshInstanceRendererComponent>().Value;
        antibodyRenderer = GameObject.Find("AntibodyRenderer").GetComponent<MeshInstanceRendererComponent>().Value;

        // spawn on start
        int playAreaSize = 400;
        float size = playAreaSize;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = new Vector2(size * 0.5f, size * 0.5f) + UnityEngine.Random.insideUnitCircle * size * 0.5f;
            pos.z = pos.y;
            pos.y = 0.5f + UnityEngine.Random.value * 2;

            // in sphere
            //Vector3 v = new Vector3(size * 0.5f, size * 0.5f, size * 0.5f) + UnityEngine.Random.insideUnitSphere * size;

            SpawnMicrobe(i, pos, EntityType.Bacteria);
        }
    }

    public static void SpawnEntity(Vector3 position, EntityType type)
    {
        int index = total;

        SpawnMicrobe(index, position, type);
    }

    public static void SpawnMicrobe(int index, Vector3 position, EntityType type)
    {
        Entity microbeEntity = _entityManager.CreateEntity(_cellArchetype);
        _entityManager.SetComponentData(microbeEntity, new Position { Value = position });
        _entityManager.SetComponentData(microbeEntity, new Rotation { Value = quaternion.identity });
        _entityManager.SetComponentData(microbeEntity, new SwayData() { Speed = 0, SwayAngle = 0, SwayDirection = 1 });
        _entityManager.SetComponentData(microbeEntity, new PositioningData() { Index = index, life = 1 });
        _entityManager.SetComponentData(microbeEntity, new Scale() { Value = new float3(scale, scale, scale) });

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

        _entityManager.AddSharedComponentData(microbeEntity, renderer);

        //_entityManager.SetComponentData(microbeEntity, new Heading{ Value = new float3(1,0,0)});
        //_entityManager.SetComponentData(microbeEntity, new MitosisData(){ A = 0 });

        entityArray[total] = microbeEntity;
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
