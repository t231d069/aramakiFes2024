using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
#endif

public class JoyconManager_Rfcomm : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Platform : " + Application.platform);

#if ENABLE_WINMD_SUPPORT
        Debug.Log("ENABLE_WINMD_SUPPORT");
#endif

    }

    // Update is called once per frame
    void Update()
    {

    }
}
