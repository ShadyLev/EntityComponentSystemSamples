using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
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

        public void AddFuel(ref SystemState state, float amount)
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

            if (SystemAPI.HasComponent<VehicleFuel>(activeVehicle))
            {
                var vehicleFuel = SystemAPI.GetComponent<VehicleFuel>(activeVehicle);
                float newFuelAmount = vehicleFuel.CurrentFuel + amount;

                newFuelAmount = Mathf.Clamp(newFuelAmount, 0, vehicleFuel.MaxFuel);

                SystemAPI.SetComponent<VehicleFuel>(activeVehicle, new VehicleFuel
                {
                    MaxFuel = vehicleFuel.MaxFuel,
                    CurrentFuel = newFuelAmount,
                    FuelDecreaseRatio = vehicleFuel.FuelDecreaseRatio
                });

            }
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
                // If there is no active car do nothing
                return;
            }

            if (activeBarrelsQuery.CalculateEntityCount() == 0)
                return;

            foreach (var (fueladd, barrel, entity) in SystemAPI.Query<RefRO<FuelBarrelAdder>, RefRO<FuelBarrel>>().WithEntityAccess())
            {
                float3 vehiclePos = state.EntityManager.GetComponentData<LocalToWorld>(activeVehicle).Position;

                Entity ce = barrel.ValueRO.Barrel;
                if (ce == Entity.Null)
                {
                    return;
                }

                float3 barrelPos = state.EntityManager.GetComponentData<LocalToWorld>(ce).Position;

                float distance = Vector3.Distance(vehiclePos, barrelPos);

                if(distance < 15f)
                {
                    //add fuel
                    AddFuel(ref state, fueladd.ValueRO.FuelAddAmount);
                    state.EntityManager.DestroyEntity(ce);
                }
                else
                {
                    return;
                }
            }
        }
    }
}
