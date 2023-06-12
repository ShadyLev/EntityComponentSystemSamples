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

            float deltaTime = SystemAPI.Time.DeltaTime;

            var reduceFuelJob = new ReduceFuel
            {
                deltaTime = deltaTime
            };

            state.Dependency = reduceFuelJob.Schedule(state.Dependency);
        }
    }

    [WithAll(typeof(ActiveVehicle))]
    partial struct ReduceFuel: IJobEntity
    {
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

                //Set new fuel
                fuel.CurrentFuel = newCurrentFuel;
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
                speed.DesiredSpeed = newDesiredSpeed;
            }
        }
    }
}
