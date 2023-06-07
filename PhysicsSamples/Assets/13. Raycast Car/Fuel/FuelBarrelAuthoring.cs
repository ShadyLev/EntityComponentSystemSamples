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
                    FuelAddAmount = authoring._fuelAddAmount
                });

                AddComponent(entity, new FuelBarrel
                {
                    Barrel = GetEntity(authoring.barrel , TransformUsageFlags.Dynamic)
                });
            }
        }
    }

    public struct FuelBarrelAdder : IComponentData
    {
        public float FuelAddAmount;
    }

    public struct FuelBarrel : IComponentData
    {
        public Entity Barrel;
    }


}
