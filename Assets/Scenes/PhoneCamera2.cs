using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PhoneCamera2: MonoBehaviour
{
    // The RowImage link to Unity UI Component At Unity Editor
    public RawImage background;
    // The AspectRatioFitter link to Unity UI Component At Unity Editor
    public AspectRatioFitter fitter;

    private bool camAvailable;
    // The live image based on camera
    private WebCamTexture backCam;

    // Start is called before the first frame update
    void Start()
    {
        // Scan available camera from devices
        WebCamDevice[] devices = WebCamTexture.devices;

        if(devices.Length == 0)
        {
            Debug.Log("No camera detected");
            this.camAvailable = false;
            return;
        }
        
        for(int i = 0; i < devices.Length; ++ i)
        {
            //if(!devices[i].isFrontFacing)
            {
                this.backCam = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
            }
        }

        if(this.backCam == null)
        {
            Debug.Log("No back camera detected");
            this.camAvailable = false;
            return;
        }

        // Start camera
        this.backCam.Play();

        // Attach back cam texture to background RawImage to display on screen
        this.background.texture = this.backCam;

        // Apply Camera Ratio to background
        float ratio = (float)backCam.width / (float)backCam.height;
        this.fitter.aspectRatio = ratio;

        float scaleY = backCam.videoVerticallyMirrored ? -1f: 1f;
        this.background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -backCam.videoRotationAngle;
        this.background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);

        this.camAvailable = true;
    }



    // Update is called once per frame
    void Update()
    {
    }


}
