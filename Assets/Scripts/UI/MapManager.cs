using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;
    public GameObject minimap;
    public GameObject largeMap;
    public Transform mapCam;
    public float minimapCamDistance = -50;
    public float largeMapCamDistance = -100;

    private int mapState = 2;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        MapState2();
    }

    private void MapState3()
    {
        minimap.SetActive(false);
        largeMap.SetActive(true);
        mapCam.position = new Vector3(mapCam.position.x, mapCam.position.y, largeMapCamDistance);
    }

    private void MapState2()
    {
        minimap.SetActive(true);
        largeMap.SetActive(false);
        mapCam.position = new Vector3(mapCam.position.x, mapCam.position.y, minimapCamDistance);
    }

    private void MapState1()
    {
        minimap.SetActive(false);
        largeMap.SetActive(false);        
    }

    public void ToggleLargeMap(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            mapState++;
            if (mapState > 3)
            {
                mapState = 1;
            }

            if (mapState == 1)
            {
                MapState1();
            }
            else if (mapState == 2)
            {
                MapState2();
            }
            else
            {
                MapState3();
            }
        }
        
    }
}
