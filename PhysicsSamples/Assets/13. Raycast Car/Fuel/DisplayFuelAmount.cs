 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.UI;

public class DisplayFuelAmount : MonoBehaviour
{
    private EntityManager _entityManager;
    private Entity _fuelEntity;

    public Text fuelTextDisplay;

    
    // Start is called before the first frame update
    IEnumerator Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        yield return new WaitForSeconds(0.5f);

        _fuelEntity = _entityManager.CreateEntityQuery(typeof(RaycastCar.ActiveVehicle)).GetSingletonEntity();
    }

    // Update is called once per frame
    void Update()
    {
        EntityQuery vehicleQuery = _entityManager.CreateEntityQuery(typeof(RaycastCar.ActiveVehicle));

        if (!vehicleQuery.IsEmpty)
        {
            _fuelEntity = vehicleQuery.GetSingletonEntity();
        }
        else
        {
            return;
        }

        if (!_entityManager.HasComponent<RaycastCar.VehicleFuel>(_fuelEntity))
        {
            return;
        }

        var currentFuelAmount = _entityManager.GetComponentData<RaycastCar.VehicleFuel>(_fuelEntity).CurrentFuel;
        var maxFuelAmount = _entityManager.GetComponentData<RaycastCar.VehicleFuel>(_fuelEntity).MaxFuel;

        currentFuelAmount = Mathf.Floor(currentFuelAmount);

        fuelTextDisplay.text = currentFuelAmount.ToString() + " / " + maxFuelAmount.ToString();
    }
}
