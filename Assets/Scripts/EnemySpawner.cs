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
    public static EntityManager _entityManager;
    private static MeshInstanceRenderer _meshRenderer;
    private static MeshInstanceRenderer _meshRenderer2;
    public static EntityArchetype _cellArchetype;
    public static NativeArray<Entity> entityArray;

    public static int total;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
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


    void OnDisable()
    {
        //entityArray.Dispose();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeWithScene()
    {
        var mrs = FindObjectsOfType<MeshInstanceRendererComponent>();

        _meshRenderer = mrs[0].Value;
        _meshRenderer2 = mrs[1].Value;

        //_meshRenderer = GameObject.FindObjectOfType<MeshInstanceRendererComponent>().Value;
        //_meshRenderer = GameObject.FindObjectOfType<MeshInstanceRendererComponent>().Value;

        for (int i = 0; i < 10000; i++)
        {
            SpawnEnemy(i);
        }
    }

    //static float size = 50;
    const float scale = 1;

    public static void SpawnEnemy(int index)
    {
        int playAreaSize = 400;
        float size = playAreaSize;
        Vector3 v = new Vector2(size*0.5f, size * 0.5f) + UnityEngine.Random.insideUnitCircle * size * 0.5f;
        v.z = v.y;
        v.y = 0.5f + UnityEngine.Random.value * 2;

        //Vector3 v = new Vector3(size * 0.5f, size * 0.5f, size * 0.5f) + UnityEngine.Random.insideUnitSphere * size;

        Entity enemyEntity = _entityManager.CreateEntity(_cellArchetype);
        _entityManager.SetComponentData(enemyEntity, new Position { Value = v });
        _entityManager.SetComponentData(enemyEntity, new Rotation { Value = quaternion.identity });
        //_entityManager.SetComponentData(enemyEntity, new Heading{ Value = new float3(1,0,0)});
        _entityManager.SetComponentData(enemyEntity, new EnemyData() { Speed = 0, SwayAngle = 0, SwayDirection = 1 });
        _entityManager.SetComponentData(enemyEntity, new PositioningData() { Index = index, life = 1 });
        //_entityManager.SetComponentData(enemyEntity, new MitosisData(){ A = 0 });
        _entityManager.SetComponentData(enemyEntity, new Scale() { Value = new float3(scale, scale, scale) });
        //_entityManager.SetComponentData(enemyEntity, _meshRenderer);

        _entityManager.AddSharedComponentData(enemyEntity, _meshRenderer);

        entityArray[total] = enemyEntity;
        total++;


        //Object.Instantiate(Enemy, new Vector3(Random.value * 50 - 25, Enemy.transform.position.y, Random.value * 50 - 25), Enemy.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Object.Instantiate(Enemy, new Vector3(Random.value * 50 - 25, Enemy.transform.position.y, Random.value * 50 - 25), Enemy.transform.rotation);
        }
    }

    // Nothketov dirty za testing

    public static void SpawnEnemyAtPosition(Vector3 pos)
    {
        int index = total;

        Entity enemyEntity = _entityManager.CreateEntity(_cellArchetype);
        _entityManager.SetComponentData(enemyEntity, new Position { Value = new float3(pos.x, pos.y, pos.z) });
        _entityManager.SetComponentData(enemyEntity, new Rotation { Value = quaternion.identity });
        //_entityManager.SetComponentData(enemyEntity, new Heading{ Value = new float3(1,0,0)});
        _entityManager.SetComponentData(enemyEntity, new EnemyData() { Speed = 0, SwayAngle = 0, SwayDirection = 1 });
        _entityManager.SetComponentData(enemyEntity, new Scale() { Value = new float3(scale, scale, scale) });
        //_entityManager.SetComponentData(enemyEntity, new MitosisData(){ A = 0 });
        _entityManager.SetComponentData(enemyEntity, new PositioningData() { Index = index, life = 1 });
        //_entityManager.SetComponentData(enemyEntity, _meshRenderer);

        _entityManager.AddSharedComponentData(enemyEntity, _meshRenderer2);


        entityArray[total] = enemyEntity;
        total++;

        //Debug.Log("Spawned " + index);
    }

    public static void SpawnEnemy()
    {
        SpawnEnemy(UnityEngine.Random.Range(0, int.MaxValue));
    }

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
}
