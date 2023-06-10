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
                AddComponent(entity, new FuelBarrelData()
                {
                    FuelAddAmount = authoring._fuelAddAmount
                });

                AddComponent<FuelBarrel>(entity);
            }
        }
    }

    public struct FuelBarrelData : IComponentData
    {
        public float FuelAddAmount;
    }

    public struct FuelBarrel : IComponentData
    {
    }


}
