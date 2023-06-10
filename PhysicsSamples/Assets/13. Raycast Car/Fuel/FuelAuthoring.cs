using Unity.Entities;
using UnityEngine;


namespace RaycastCar
{
    public class FuelAuthoring : MonoBehaviour
    {
        public float MaxFuel = 100f;
        public float FuelUsageAmount = 10f;
        public float FuelPickupRange = 3f;
        public float SpeedDecrease = 0.2f;

        class FuelBaker : Baker<FuelAuthoring>
        {
            public override void Bake(FuelAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new VehicleFuel
                {
                    MaxFuel = authoring.MaxFuel,
                    CurrentFuel = authoring.MaxFuel,
                    FuelUsageAmount = authoring.FuelUsageAmount,
                    SpeedDecrease = authoring.SpeedDecrease,
                    FuelPickupRange = authoring.FuelPickupRange
                });
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(transform.position, FuelPickupRange);
        }
    }
    public struct VehicleFuel : IComponentData
    {
        public float MaxFuel;
        public float CurrentFuel;
        public float FuelUsageAmount;
        public float SpeedDecrease;
        public float FuelPickupRange;
    }
}
