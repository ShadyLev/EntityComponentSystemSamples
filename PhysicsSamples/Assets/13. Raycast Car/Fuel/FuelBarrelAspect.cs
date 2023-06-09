using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

namespace RaycastCar
{
    public readonly partial struct FuelBarrelAspect : IAspect
    {
        private readonly Entity entity;

        private readonly RefRO<LocalTransform> localTransform;
        private readonly RefRW<RaycastCar.FuelBarrelAdder> fuelAdder;

        public void IsInPickUpRange(LocalToWorld carPosition, float range)
        {
            float distance = math.distance(carPosition.Position, localTransform.ValueRO.Position);

            if (distance < range)
            {
                Debug.Log("IN RANGE");
                fuelAdder.ValueRW.AddFuel = true;
                fuelAdder.ValueRW.addingEntity = entity;
            }
            else
            {

            }
        }

        public void TestJobTask(Entity activeVehicle, float currentFuel, float maxFuel, float fuelDecreaseRatio)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            if (fuelAdder.ValueRW.AddFuel == true && fuelAdder.ValueRW.addingEntity != Entity.Null)
            {
                    float newFuelAmount = currentFuel + fuelAdder.ValueRW.FuelAddAmount;

                    newFuelAmount = Mathf.Clamp(newFuelAmount, 0, maxFuel);

                ecb.SetComponent<VehicleFuel>(activeVehicle, new VehicleFuel
                {
                    MaxFuel = maxFuel,
                    CurrentFuel = newFuelAmount,
                    FuelDecreaseRatio = fuelDecreaseRatio
                });

                Debug.Log("Added fuel");
            }
            ecb.DestroyEntity(fuelAdder.ValueRW.addingEntity);

            ecb.Dispose();
        }
    }
}
