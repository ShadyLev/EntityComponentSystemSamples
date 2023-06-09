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
        private readonly RefRW<FuelBarrelData> fuelBarrelData;

        public float GetFuelAddAmount()
        {
            return fuelBarrelData.ValueRO.FuelAddAmount;
        }
    }
}
