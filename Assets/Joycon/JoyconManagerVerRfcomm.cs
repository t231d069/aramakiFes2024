using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WINDOWS_UWP
using Windows.Devices.Bluetooth;
#endif

public class JoyconManagerVerRfcomm : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Platform : " + Application.platform);

#if WINDOWS_UWP

#endif

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
