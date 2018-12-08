using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PlayerMovementSystem : ComponentSystem {

    private struct Filter
    {
        public Transform Transform;
        public InputComponent InputComponent;
    }

	protected override void OnUpdate()
    {
        var deltaTime = Time.deltaTime;
        foreach (var entity in GetEntities<Filter>())
        {
            if (entity.InputComponent.W)
            {
                entity.Transform.position += new Vector3(4f * deltaTime, 0, 0);
            }
            if (entity.InputComponent.A)
            {
                entity.Transform.position += new Vector3(0, 0, 4f * deltaTime);
            }
            if (entity.InputComponent.S)
            {
                entity.Transform.position += new Vector3(-4f * deltaTime, 0, 0);
            }
            if (entity.InputComponent.D)
            {
                entity.Transform.position += new Vector3(0, 0, -4f * deltaTime);
            }
        }
    }
}
