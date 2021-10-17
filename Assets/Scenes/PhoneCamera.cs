using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;

public class PhoneCamera: MonoBehaviour
{
    private bool camAvailable;
    private WebCamTexture backCam;
    private Texture defaultBackground;

    public RawImage background;
    public AspectRatioFitter fitter;




    // Start is called before the first frame update
    void Start()
    {
        defaultBackground = background.texture;
        WebCamDevice[] devices = WebCamTexture.devices;

        if(devices.Length == 0)
        {
            Debug.Log("No camera detected");
            camAvailable = false;
            return;
        }
        
        for(int i = 0; i < devices.Length; ++ i)
        {
            if(!devices[i].isFrontFacing)
            {
                backCam = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
            }
        }

        if(backCam == null)
        {
            Debug.Log("No back camera detected");
            camAvailable = false;
            return;
        }

        backCam.Play();
        // background.texture = backCam;

        camAvailable = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!camAvailable)
        {
            return;
        }

        GetComponent<Renderer>().material.mainTexture = backCam;
        Mat frame = OpenCvSharp.Unity.TextureToMat(backCam);
        
        
        FrameToHSV(frame);
        // float ratio = (float)backCam.width / (float)backCam.height;
        // fitter.aspectRatio = ratio;

        // float scaleY = backCam.videoVerticallyMirrored ? -1f: 1f;
        // background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        // int orient = -backCam.videoRotationAngle;
        // background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    }

    private void FrameToHSV(Mat frame)
    {
        Mat greyFrame = new Mat();

        Cv2.CvtColor(frame, greyFrame, ColorConversionCodes.RGB2GRAY);

        background.texture = OpenCvSharp.Unity.MatToTexture(greyFrame);
    }
}
