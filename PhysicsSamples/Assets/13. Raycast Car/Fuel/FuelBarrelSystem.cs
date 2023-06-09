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
        private ComponentLookup<FuelBarrelData> fuelBarrelDataLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            posLookup = state.GetComponentLookup<LocalToWorld>(true);
            fuelBarrelDataLookup = state.GetComponentLookup<FuelBarrelData>(false);
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
                Debug.Log("No active car in the world");
                return;
            }

            if (activeBarrelsQuery.CalculateEntityCount() == 0)
            {
                Debug.Log("No Barrels in the world");
                return;
            }

            LocalToWorld carPosition = state.EntityManager.GetComponentData<LocalToWorld>(activeVehicle);
            VehicleFuel carFuelComponent = state.EntityManager.GetComponentData<VehicleFuel>(activeVehicle);

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            posLookup.Update(ref state);
            fuelBarrelDataLookup.Update(ref state);

            var addFuelJob = new AddFuelJob
            {
                activeVehicle = activeVehicle,
                positionLookup = posLookup,
                fuelBarrelDataLookup = fuelBarrelDataLookup,
                vehicleFuel = carFuelComponent,
                ecb = ecb.AsParallelWriter()
            };

            state.Dependency = addFuelJob.Schedule(state.Dependency);

            /*
            var barrelEntities = activeBarrelsQuery.ToEntityArray(Allocator.TempJob);

            for(int i=0; i < barrelEntities.Length; i++)
            {
                var barrelEntity = barrelEntities[i];
                var barrelComponent = state.EntityManager.GetComponentData<FuelBarrelAdder>(barrelEntity);

                LocalToWorld barrelPos = state.EntityManager.GetComponentData<LocalToWorld>(barrelEntity);

                float distance = math.distance(carPosition.Position, barrelPos.Position);

                if(distance < 5f)
                {
                    float newFuelAmount = carFuelComponent.CurrentFuel + barrelComponent.FuelAddAmount;

                    newFuelAmount = Mathf.Clamp(newFuelAmount, 0, carFuelComponent.MaxFuel);

                    state.EntityManager.SetComponentData<VehicleFuel>(activeVehicle, new VehicleFuel
                    {
                        MaxFuel = carFuelComponent.MaxFuel,
                        CurrentFuel = newFuelAmount,
                        FuelDecreaseRatio = carFuelComponent.FuelDecreaseRatio
                    });

                    state.EntityManager.DestroyEntity(barrelEntity);
                }
            }

            barrelEntities.Dispose();
            */
        }

        [WithAll(typeof(FuelBarrel))]
        [BurstCompile]
        public partial struct AddFuelJob : IJobEntity
        {
            [ReadOnly] public Entity activeVehicle;
            [ReadOnly] public ComponentLookup<LocalToWorld> positionLookup;
            [ReadOnly] public ComponentLookup<FuelBarrelData> fuelBarrelDataLookup;
            public VehicleFuel vehicleFuel;
            public EntityCommandBuffer.ParallelWriter ecb;

            [BurstCompile]
            public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity)
            {
                float3 barrelPos = positionLookup[entity].Position;
                float3 vehiclePos = positionLookup[activeVehicle].Position;

                float distance = math.distance(barrelPos, vehiclePos);

                if(distance <= fuelBarrelDataLookup[entity].TriggerRange)
                {
                    float newFuelAmount = vehicleFuel.CurrentFuel + fuelBarrelDataLookup[entity].FuelAddAmount;

                    newFuelAmount = math.clamp(newFuelAmount, 0, vehicleFuel.MaxFuel);

                    float maxFuel = vehicleFuel.MaxFuel;
                    float fuelRatio = vehicleFuel.FuelDecreaseRatio;

                    ecb.SetComponent<VehicleFuel>(chunkIndex, activeVehicle, new VehicleFuel {
                        CurrentFuel = newFuelAmount,
                        MaxFuel = maxFuel,
                        FuelDecreaseRatio = fuelRatio
                    });

                    ecb.DestroyEntity(chunkIndex, entity);
                }

            }
        }
    }
}
