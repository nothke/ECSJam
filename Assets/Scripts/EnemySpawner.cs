using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	public static EntityManager _entityManager;
	private static MeshInstanceRenderer _meshRenderer;
	public static EntityArchetype _cellArchetype;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Initialize()
	{
		_entityManager = World.Active.GetOrCreateManager<EntityManager>();

		_cellArchetype = _entityManager.CreateArchetype(
			typeof(Position),
			typeof(Rotation),
			typeof(Scale),
			typeof(EnemyData),
			typeof(PositioningData),
			typeof(AvoidanceData),
			typeof(MitosisData),
			//typeof(MeshInstanceRenderer),
			typeof(LocalToWorld));
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void InitializeWithScene()
	{
		_meshRenderer = GameObject.FindObjectOfType<MeshInstanceRendererComponent>().Value;
		
		for (int i = 0; i < 100; i++)
		{
			SpawnEnemy(i);
		}
	}

	public static void SpawnEnemy(int index)
	{
		Entity enemyEntity = _entityManager.CreateEntity(_cellArchetype);
		_entityManager.SetComponentData(enemyEntity, new Position{ Value = new float3(UnityEngine.Random.value * 50, .5f + UnityEngine.Random.value*2, UnityEngine.Random.value * 50)});
		_entityManager.SetComponentData(enemyEntity, new Rotation{ Value = quaternion.identity });
		//_entityManager.SetComponentData(enemyEntity, new Heading{ Value = new float3(1,0,0)});
		_entityManager.SetComponentData(enemyEntity, new EnemyData(){ Speed = 0, SwayAngle = 0, SwayDirection = 1});
		_entityManager.SetComponentData(enemyEntity, new PositioningData(){ Index = index });
		//_entityManager.SetComponentData(enemyEntity, new MitosisData(){ A = 0 });
		_entityManager.SetComponentData(enemyEntity, new Scale() { Value = new float3(1,1,1)});
		_entityManager.SetComponentData(enemyEntity, new AvoidanceData() { Force =  new float2(0,0)});
		//_entityManager.SetComponentData(enemyEntity, _meshRenderer);
		
		_entityManager.AddSharedComponentData(enemyEntity, _meshRenderer);
				
		//Object.Instantiate(Enemy, new Vector3(Random.value * 50 - 25, Enemy.transform.position.y, Random.value * 50 - 25), Enemy.transform.rotation);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space))
        {
            //Object.Instantiate(Enemy, new Vector3(Random.value * 50 - 25, Enemy.transform.position.y, Random.value * 50 - 25), Enemy.transform.rotation);
        }
	}
}
