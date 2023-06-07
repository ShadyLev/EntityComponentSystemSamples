using Unity.Entities;
using UnityEngine;


namespace RaycastCar
{
    public class FuelAuthoring : MonoBehaviour
    {
        public float MaxFuel = 100f;
        public float FuelDecreaseRatio = 10f;

        class FuelBaker : Baker<FuelAuthoring>
        {
            public override void Bake(FuelAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new VehicleFuel
                {
                    MaxFuel = authoring.MaxFuel,
                    CurrentFuel = authoring.MaxFuel,
                    FuelDecreaseRatio = authoring.FuelDecreaseRatio
                });
            }
        }
    }
    struct VehicleFuel : IComponentData
    {
        public float MaxFuel;
        public float CurrentFuel;
        public float FuelDecreaseRatio;
    }

    struct DisableVehicle : IComponentData
    {

    }
}
