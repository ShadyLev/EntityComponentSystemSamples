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
            var activeVehicleQuery = SystemAPI.QueryBuilder().WithAll<ActiveVehicle, Vehicle, VehicleSpeed, VehicleFuel>().Build();

            var activeVehicle = Entity.Null;
            if (activeVehicleQuery.CalculateEntityCount() == 1)
            {
                activeVehicle = activeVehicleQuery.GetSingletonEntity();
            }
            else
            {
                return;
            }

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            float deltaTime = SystemAPI.Time.DeltaTime;

            var reduceFuelJob = new ReduceFuel
            {
                ecb = ecb.AsParallelWriter(),
                deltaTime = deltaTime
            };

            state.Dependency = reduceFuelJob.Schedule(state.Dependency);

            /*
            // Check if active vehicle is moving
            bool driveEngaged = false;
            var vehicleSpeed = SystemAPI.GetComponent<VehicleSpeed>(activeVehicle);
            driveEngaged = vehicleSpeed.DriveEngaged != 0;


            float currentFuel;
            if (driveEngaged)
            {
                var vehicleFuel = SystemAPI.GetComponent<VehicleFuel>(activeVehicle);
                currentFuel = vehicleFuel.CurrentFuel;

                currentFuel -= vehicleFuel.FuelDecreaseRatio * SystemAPI.Time.DeltaTime;
                currentFuel = Mathf.Clamp(currentFuel, 0, vehicleFuel.MaxFuel);

                if (currentFuel <= 0)
                {
                    var speed = SystemAPI.GetComponent<VehicleSpeed>(activeVehicle);
                    float newDesiredSpeed = 0;

                    if (speed.DesiredSpeed > 0)
                    {
                        newDesiredSpeed = speed.DesiredSpeed - 0.2f;
                    }else if(speed.DesiredSpeed <= 0)
                    {
                        newDesiredSpeed = 0;
                    }

                    // Set new speed
                    SystemAPI.SetComponent<VehicleSpeed>(activeVehicle, new VehicleSpeed {
                        TopSpeed = speed.TopSpeed,
                        DesiredSpeed = newDesiredSpeed,
                        Damping = speed.Damping,
                        DriveEngaged = 0,
                    });
                }

                // Set new fuel
                SystemAPI.SetComponent<VehicleFuel>(activeVehicle, new VehicleFuel
                {
                    MaxFuel = vehicleFuel.MaxFuel,
                    CurrentFuel = currentFuel,
                    FuelDecreaseRatio = vehicleFuel.FuelDecreaseRatio
                });
            }
            */
        }
    }

    [WithAll(typeof(ActiveVehicle))]
    partial struct ReduceFuel: IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public float deltaTime;

        void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref VehicleSpeed speed, ref VehicleFuel fuel)
        {

            // Reduce fuel when driving
            bool driveEngaged = speed.DriveEngaged != 0;

            if (driveEngaged)
            {
                float newCurrentFuel = fuel.CurrentFuel;
                newCurrentFuel -= fuel.FuelUsageAmount * deltaTime;
                newCurrentFuel = Mathf.Clamp(newCurrentFuel, 0, fuel.MaxFuel);

                //fuel.CurrentFuel = newCurrentFuel;

                // Set new fuel
                ecb.SetComponent<VehicleFuel>(chunkIndex, entity, new VehicleFuel
                {
                    MaxFuel = fuel.MaxFuel,
                    CurrentFuel = newCurrentFuel,
                    FuelUsageAmount = fuel.FuelUsageAmount,
                    SpeedDecrease = fuel.SpeedDecrease,
                    FuelPickupRange = fuel.FuelPickupRange
                });
            }

            // Stop vehicle if no fuel
            if(fuel.CurrentFuel <= 0)
            {
                float newDesiredSpeed = 0;

                if (speed.DesiredSpeed > 0)
                {
                    newDesiredSpeed = speed.DesiredSpeed - fuel.SpeedDecrease;
                }
                else if (speed.DesiredSpeed <= 0)
                {
                    newDesiredSpeed = 0;
                }

                // Set new speed
                ecb.SetComponent<VehicleSpeed>(chunkIndex, entity, new VehicleSpeed
                {
                    TopSpeed = speed.TopSpeed,
                    DesiredSpeed = newDesiredSpeed,
                    Damping = speed.Damping,
                    DriveEngaged = speed.DriveEngaged,
                });
            }
        }
    }
}
