using Unity.Entities;
using UnityEngine;

namespace RaycastCar
{
    public class FuelBarrelAuthoring : MonoBehaviour
    {
        public float _fuelAddAmount;
        public float _pickUpTriggerRange;

        class Baker : Baker<FuelBarrelAuthoring>
        {
            public override void Bake(FuelBarrelAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FuelBarrelData()
                {
                    FuelAddAmount = authoring._fuelAddAmount,
                    TriggerRange = authoring._pickUpTriggerRange
                });

                AddComponent<FuelBarrel>(entity);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(transform.position, _pickUpTriggerRange);
        }
    }

    public struct FuelBarrelData : IComponentData
    {
        public float FuelAddAmount;
        public float TriggerRange;
    }

    public struct FuelBarrel : IComponentData
    {
    }


}
