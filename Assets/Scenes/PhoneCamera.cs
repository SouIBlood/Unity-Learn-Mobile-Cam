using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PhoneCamera: MonoBehaviour
{
    public RawImage background;
    public AspectRatioFitter fitter;

    private bool camAvailable;

    private WebCamTexture backCam;

    private Texture2D camTexture;

    List<IMultipartFormSection> formData;

    private YieldInstruction oneSecond;
    private YieldInstruction qSecond;

    // Start is called before the first frame update
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if(devices.Length == 0)
        {
            Debug.Log("No camera detected");
            this.camAvailable = false;
            return;
        }
        
        for(int i = 0; i < devices.Length; ++ i)
        {
            if(!devices[i].isFrontFacing)
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

        this.backCam.Play();

        // Apply Camera Ratio to background
        float ratio = (float)backCam.width / (float)backCam.height;
        this.fitter.aspectRatio = ratio;

        float scaleY = backCam.videoVerticallyMirrored ? -1f: 1f;
        this.background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -backCam.videoRotationAngle;
        this.background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);

        this.camAvailable = true;

        StartCoroutine(processImage());
    }

    private IEnumerator processImage()
    {
        if(this.camAvailable)
        {
            Debug.Log("Wait 1 seconds");
            if(this.oneSecond == null)
            {
                this.oneSecond = new WaitForSeconds(1.0f);
                this.qSecond = new WaitForSeconds(0.05f);
            }

            yield return this.oneSecond;

            if(this.camTexture == null)
            {
                this.camTexture = new Texture2D(backCam.width, backCam.height);
            }
            if(this.formData == null)
            {
                this.formData = new List<IMultipartFormSection>();
            }
            
            while (true)
            {
                this.camTexture.SetPixels32(this.backCam.GetPixels32());
                this.camTexture.Apply();


                this.formData.Add(new MultipartFormFileSection("image", this.camTexture.EncodeToJPG(), "image.jpg", "image/jpeg"));

                UnityWebRequest www = UnityWebRequest.Post("http://192.168.0.111:5000/upload", this.formData);
                www.downloadHandler = new DownloadHandlerTexture();
                Debug.Log("Begin Upload");

                yield return www.SendWebRequest();
                Debug.Log("Done Post");

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Request Failed " + www.error);
                }
                else
                {
                    
                    this.background.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    Debug.Log("OK");
                }


                www.uploadHandler.Dispose();
                www.downloadHandler.Dispose();
                www.Dispose();

                formData.Clear();
                Debug.Log("Wait 1 second");

                yield return this.qSecond;
            }
        }


    }


    // Update is called once per frame
    void Update()
    {
    }


}
