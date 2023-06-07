using Unity.Entities;
using UnityEngine;

namespace RaycastCar
{
    public class FuelBarrelAuthoring : MonoBehaviour
    {
        public float _fuelAddAmount;

        class Baker : Baker<FuelBarrelAuthoring>
        {
            public override void Bake(FuelBarrelAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FuelBarrel()
                {
                    FuelAddAmount = authoring._fuelAddAmount
                });
            }
        }
    }

    public struct FuelBarrel : IComponentData
    {
        public float FuelAddAmount;
    }
}
