using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace RaycastCar
{
    [BurstCompile]
    public partial struct FuelBarrelSystem : ISystem
    {
        private ComponentLookup<LocalToWorld> posLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            posLookup = state.GetComponentLookup<LocalToWorld>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var activeVehicleQuery = SystemAPI.QueryBuilder().WithAll<ActiveVehicle>().Build();
            var activeBarrelsQuery = SystemAPI.QueryBuilder().WithAll<FuelBarrel>().Build();

            var activeVehicle = Entity.Null;
            if (activeVehicleQuery.CalculateEntityCount() == 1)
            {
                activeVehicle = activeVehicleQuery.GetSingletonEntity();
            }
            else
            {
                return;
            }

            if (activeBarrelsQuery.CalculateEntityCount() == 0)
            {
                return;
            }

            // Get data for the job
            VehicleFuel carFuelComponent = state.EntityManager.GetComponentData<VehicleFuel>(activeVehicle);

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            posLookup.Update(ref state);

            var addFuelJob = new AddFuelJob
            {
                activeVehicle = activeVehicle,
                positionLookup = posLookup,
                vehicleFuel = carFuelComponent,
                ecb = ecb.AsParallelWriter()
            };

            state.Dependency = addFuelJob.Schedule(state.Dependency);
        }

        [WithAll(typeof(FuelBarrel))]
        [BurstCompile]
        public partial struct AddFuelJob : IJobEntity
        {
            [ReadOnly] public Entity activeVehicle;
            [ReadOnly] public ComponentLookup<LocalToWorld> positionLookup;
            public VehicleFuel vehicleFuel;
            public EntityCommandBuffer.ParallelWriter ecb;

            [BurstCompile]
            public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in FuelBarrelData fuelData)
            {
                float3 barrelPos = positionLookup[entity].Position;
                float3 vehiclePos = positionLookup[activeVehicle].Position;

                float distance = math.distance(barrelPos, vehiclePos);

                if(distance <= vehicleFuel.FuelPickupRange)
                {
                    float newFuelAmount = vehicleFuel.CurrentFuel + fuelData.FuelAddAmount;
                    newFuelAmount = math.clamp(newFuelAmount, 0, vehicleFuel.MaxFuel);

                    ecb.SetComponent<VehicleFuel>(chunkIndex, activeVehicle, new VehicleFuel {
                        CurrentFuel = newFuelAmount,
                        MaxFuel = vehicleFuel.MaxFuel,
                        FuelUsageAmount = vehicleFuel.FuelUsageAmount,
                        SpeedDecrease = vehicleFuel.SpeedDecrease,
                        FuelPickupRange = vehicleFuel.FuelPickupRange
                    });

                    ecb.DestroyEntity(chunkIndex, entity);
                }
            }
        }
    }
}
