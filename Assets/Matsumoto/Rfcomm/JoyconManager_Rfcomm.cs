using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WINDOWS_UWP
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
#endif

public class JoyconManager_Rfcomm : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Platform : " + Application.platform);

#if WINDOWS_UWP
        Debug.Log("Platform is WINDOWS_UWP");
#endif

    }

    // Update is called once per frame
    void Update()
    {

    }
}
