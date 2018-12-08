using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	private static EntityManager _entityManager;
	private static MeshInstanceRenderer _meshRenderer;
	private static EntityArchetype _cubeArchetype;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Initialize()
	{
		_entityManager = World.Active.GetOrCreateManager<EntityManager>();

		_cubeArchetype = _entityManager.CreateArchetype(
			typeof(Position),
			typeof(Rotation),
			typeof(EnemyData),
			typeof(PositioningData),
			//typeof(MeshInstanceRenderer),
			typeof(TransformMatrix));
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void InitializeWithScene()
	{
		_meshRenderer = GameObject.FindObjectOfType<MeshInstanceRendererComponent>().Value;
		
		for (int i = 0; i < 10000; i++)
		{
			SpawnEnemy(i);
		}
	}

	private static void SpawnEnemy(int index)
	{
		Entity enemyEntity = _entityManager.CreateEntity(_cubeArchetype);
		_entityManager.SetComponentData(enemyEntity, new Position{ Value = new float3(Random.value * 50 - 25, .5f + Random.value*2, Random.value * 50 - 25)});
		_entityManager.SetComponentData(enemyEntity, new Rotation{ Value = quaternion.identity });
		//_entityManager.SetComponentData(enemyEntity, new Heading{ Value = new float3(1,0,0)});
		_entityManager.SetComponentData(enemyEntity, new EnemyData(){ Speed = 0, SwayAngle = 0, SwayDirection = 1});
		_entityManager.SetComponentData(enemyEntity, new PositioningData(){ Index = index });
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
