using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.Physics.Systems;

namespace RaycastCar
{
    [BurstCompile]
    public partial struct FuelSystem: ISystem
    {

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var activeVehicleQuery = SystemAPI.QueryBuilder().WithAll<ActiveVehicle, Vehicle>().Build();

            var activeVehicle = Entity.Null;
            if (activeVehicleQuery.CalculateEntityCount() == 1)
            {
                activeVehicle = activeVehicleQuery.GetSingletonEntity();
            }
            else
            {
                return;
            }

            // Check if active vehicle is moving
            bool driveEngaged = false;
            if (SystemAPI.HasComponent<VehicleSpeed>(activeVehicle))
            {
                var vehicleSpeed = SystemAPI.GetComponent<VehicleSpeed>(activeVehicle);
                driveEngaged = vehicleSpeed.DriveEngaged != 0;
            }


            float currentFuel;
            if (SystemAPI.HasComponent<VehicleFuel>(activeVehicle) && driveEngaged)
            {
                var vehicleFuel = SystemAPI.GetComponent<VehicleFuel>(activeVehicle);
                currentFuel = vehicleFuel.CurrentFuel;

                currentFuel -= vehicleFuel.FuelDecreaseRatio * Time.deltaTime;
                currentFuel = Mathf.Clamp(currentFuel, 0, vehicleFuel.MaxFuel);

                if (currentFuel <= 0)
                {
                    // somehow disable the car
                
                }


                SystemAPI.SetComponent<VehicleFuel>(activeVehicle, new VehicleFuel
                {
                    MaxFuel = vehicleFuel.MaxFuel,
                    CurrentFuel = currentFuel,
                    FuelDecreaseRatio = vehicleFuel.FuelDecreaseRatio
                });
            }
        }
    }
}
