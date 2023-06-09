using Unity.Entities;
using UnityEngine;

namespace RaycastCar
{
    public class FuelBarrelAuthoring : MonoBehaviour
    {
        public float _fuelAddAmount;
        public GameObject barrel;

        class Baker : Baker<FuelBarrelAuthoring>
        {
            public override void Bake(FuelBarrelAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FuelBarrelAdder()
                {
                    FuelAddAmount = authoring._fuelAddAmount,
                    AddFuel = false,
                    addingEntity = Entity.Null
                });

                AddComponent<FuelBarrel>(entity);
            }
        }
    }

    public struct FuelBarrelAdder : IComponentData
    {
        public float FuelAddAmount;
        public bool AddFuel;
        public Entity addingEntity;
    }

    public struct FuelBarrel : IComponentData
    {
    }


}
