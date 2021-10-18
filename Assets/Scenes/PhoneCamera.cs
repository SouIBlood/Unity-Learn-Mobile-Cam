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

    private IEnumerator coroutine;

    private WebCamTexture backCam;
    private Texture2D outTexture;


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

        coroutine = processImage();
        StartCoroutine(coroutine);
    }

    private IEnumerator processImage()
    {
        if(this.camAvailable)
        {
            while (true)
            {
                Debug.Log("Process Image");

                List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

                Color32[] data = new Color32[backCam.width * backCam.height];
                backCam.GetPixels32(data);

                Debug.Log("Process Image 1");

                formData.Add(new MultipartFormFileSection("image", Color32ArrayToByteArray(data), "image.jpg", "image/jpeg"));
                Debug.Log("From Data Add");

                UnityWebRequest www = UnityWebRequest.Post("http://192.168.0.135:5000/upload", formData);

                Debug.Log("From Data Post");

                yield return www.SendWebRequest();
                Debug.Log("Request complete");

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Request Failed " + www.error);
                }
                else
                {
                    Debug.Log("Success");

                    outTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    background.texture = outTexture;

                    Debug.Log("OK");
                }
                Debug.Log("Wait");
                yield return new WaitForSeconds(1.0f);
            }
        }


    }


    private static byte[] Color32ArrayToByteArray(Color32[] colors)
    {
        if (colors == null || colors.Length == 0)
            return null;

        int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
        int length = lengthOfColor32 * colors.Length;
        byte[] bytes = new byte[length];

        GCHandle handle = default(GCHandle);
        try
        {
            handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
            System.IntPtr ptr = handle.AddrOfPinnedObject();
            Marshal.Copy(ptr, bytes, 0, length);
        }
        finally
        {
            if (handle != default(GCHandle))
                handle.Free();
        }

        return bytes;
    }


    // Update is called once per frame
    void Update()
    {
    }


}
