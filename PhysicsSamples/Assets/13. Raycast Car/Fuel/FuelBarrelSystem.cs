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
        }

        public partial struct GetDistancesJob : IJobEntity
        {
            public LocalToWorld position;
            public float pickUpRange;

            public void Execute(FuelBarrelAspect fuelBarrelAspect)
            {
                fuelBarrelAspect.IsInPickUpRange(position, pickUpRange);
            }
        }

        public partial struct AddFuelJob : IJobEntity
        {
            public Entity activeVehicle;
            public float currentFuel;
            public float maxFuel;
            public float fuelDecreaseRatio;

            public void Execute(FuelBarrelAspect fuelBarrelAspect)
            {
                fuelBarrelAspect.TestJobTask(activeVehicle, currentFuel, maxFuel, fuelDecreaseRatio);
            }
        }
    }
}
