using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using CameraPreview;
using System.Linq;

public class CamReader : MonoBehaviour
{
    private CameraPreviewCapture capture;
    private ComputeBuffer pointsBuffer, whitesBuffer;
    private int kernelCalc;
    private int[] whites;

    public GameObject targetObject, infoObject;
    private bool updateView = false;
    private byte[] buffer;
    private byte[] currBuffer;
    private byte[] prevBuffer;
    private int bufferWidth, bufferHeight;
    private TextureFormat bufferFormat;

    public ComputeShader compute;

    public IndChange IndChange;

    private float[] hsv;
    private int Gflag = 0;//緑を検知するフラグ

    private int ulflag = 0, umflag = 0, urflag = 0;
    private int dlflag = 0, dmflag = 0, drflag = 0;


    // Start is called before the first frame update
    async void Awake()
    {
        Debug.Log("start creating video capture.");
        await CameraPreviewCapture.CreateAsync(OnCaptureCreated);


        //kernelCalc = compute.FindKernel("CalcWhite");
    }
    async void OnCaptureCreated(CameraPreviewCapture createdObject)
    {
        if (createdObject!=null)
        {
            Debug.Log("capture created.");
            capture = createdObject;
            capture.OnFrameArrived += OnFreameArrive;
            await capture.StartVideoModeAsync(false);
        } 
        else
        {
            Debug.Log("capture not created.");
        }
    }
    void OnFreameArrive(int width, int height, TextureFormat fmt, bool isHololens)
    {
        if (buffer == null)
        {
            buffer = new byte[width * height * 4];
            bufferWidth = width;
            bufferHeight = height;
            bufferFormat = fmt;
            isHololens = false;

            //this.whites = new int[width * height];
        }
        updateView = true;
    }
    private void Start()
    {
        Gflag = 0;
        if (hsv == null)
        {
            hsv = new float[bufferWidth * bufferHeight];
        }
    }

    private void Update()
    {
        if (updateView&&buffer!=null)
        {

            //if (pointsBuffer == null)
            //{
            //    if (compute==null)
            //    {
            //        infoObject.GetComponent<TextMesh>().text ="No shader";
            //    }
            //    else
            //    {
            //        compute.SetInt("Width", bufferWidth);
            //        compute.SetInt("Height", bufferHeight);
            //        compute.SetInt("Length", bufferWidth * bufferHeight);
            //        compute.SetBool("IsRGBA", bufferFormat == TextureFormat.RGBA32);


            //        pointsBuffer = new ComputeBuffer(bufferWidth * bufferHeight, 4);
            //        compute.SetBuffer(kernelCalc, "Pixels", pointsBuffer);

            //        whitesBuffer = new ComputeBuffer(bufferWidth * bufferHeight, sizeof(int));
            //        compute.SetBuffer(kernelCalc, "Whites", whitesBuffer);

            //        Debug.LogFormat("pointsBuffer {0} whitesBuffer {1} width {2}, height {3} bsize {4}",
            //            pointsBuffer.count, whitesBuffer.count, bufferWidth, bufferHeight,
            //            buffer.Length);
            //    }
            //}
            updateView = false;
            if (targetObject != null)
            {
                TakePhoto(targetObject.transform.GetComponent<Renderer>());
                //pointsBuffer.SetData(buffer);

                ////Debug.Log("Prepared for calc.");
                //try
                //{
                //    compute.Dispatch(kernelCalc,
                //        (bufferWidth + 7) / 8,
                //        (bufferHeight + 7) / 8,
                //        1);
                //} catch (UnityException e)
                //{
                //    infoObject.GetComponent<TextMesh>().text = e.Message;
                //}
                //whitesBuffer.GetData(whites);

                //int count = 0;
                //foreach (var p in whites) count += p;

                //infoObject.GetComponent<TextMesh>().text = count.ToString();
                //Debug.Log("Compute end.");
                //Debug.LogFormat("white[0] {0} count {1}", whites[0], count);
            }
        }
        //MakeHSV();
        //CheckGreen();
        //IndChange.changeIndicator(Gflag);
        //Debug.LogFormat("Gflug: {0} total {1}", Gflag, hsv.Length);

        CheckWhite();
        IndChange.changeUIndicator(ulflag, umflag, urflag);
        IndChange.changeDIndicator(dlflag, dmflag, drflag);
        Debug.LogFormat("ulflug: {0} umflug: {1} urflug: {2}", ulflag, umflag, urflag);
        Debug.LogFormat("dlflug: {0} dmflug: {1} drflug: {2}", dlflag, dmflag, drflag);


    }

    async void OnDestroy()
    {
        //pointsBuffer.Release();
        //whitesBuffer.Release();

        if (capture!=null)
        {
            await capture.Dispose();
            capture = null;
        }
    }

    void TakePhoto(Renderer preview)
    {
        if (preview.material.mainTexture == null)
            preview.material.mainTexture = new Texture2D(bufferWidth, bufferHeight, bufferFormat, false);
        var tex = preview.material.mainTexture as Texture2D;
        //capture.CopyFrameToTexture(tex); 
        capture.CopyFrameToBuffer(buffer);//bufferにフレームのバイナリをセット
        tex.LoadRawTextureData(buffer);
        tex.Apply();
    }

    /*
    public void TakePhotoToPreview(Renderer preview)
    {
        //TakePhoto(preview.material.mainTexture);

        // update the aspect ratio to match webcam
        //float aspectRatio = (float)image.width / (float)image.height;
        //Vector3 scale = preview.transform.localScale;
        //scale.x = scale.y * aspectRatio;
        //preview.transform.localScale = scale;
    }
    */

    public void InstantiatePhoto(GameObject prefab)
    {
        Debug.Log("Take a photo!");
        TakePhoto(targetObject.transform.GetComponent<Renderer>());
    }
    
    public void GrayScale()
    {
        for (int y = 0; y < bufferHeight; y++)
        {
            for (int x = 0; x < bufferWidth; x++)
            {
                int p = (x + bufferWidth * y) * 4;
                
                byte a = buffer[p + 3];
                byte r = buffer[p + 2];
                byte g = buffer[p + 1];
                byte b = buffer[p];                
                double gray = (r * 0.29891 + g * 0.58661 + b * 0.11448);
                buffer[p + 2] = (byte)gray;
                buffer[p + 1] = (byte)gray;
                buffer[p] = (byte)gray;
                
            }
        }
    }

    public void MakeHSV()
    {
        for (int y = 0; y < bufferHeight; y++)
        {
            for (int x = 0; x < bufferWidth; x++)
            {
                int p = (x + bufferWidth * y) * 4;
                int q = (x + bufferWidth * y);

                byte a = buffer[p + 3];
                byte r = buffer[p + 2];
                byte g = buffer[p + 1];
                byte b = buffer[p];

                float h;
                float s;
                float v;

                Color.RGBToHSV(new Color(r/255f, g/225f, b/255f), out h, out s, out v);

                hsv[q] = v;

                
            }
        }
    }

    public void CheckGreen()
    {
       Gflag = 0;
        for(int i = 0; i < hsv.Length; i++)
        {
            if (hsv[i] > 0.98f)
            {
                Gflag += 1;
            }
        }
        
    }

    public void CheckWhite()
    {
        ulflag = 0;
        umflag = 0;
        urflag = 0;
        dlflag = 0;
        dmflag = 0;
        drflag = 0;

        for (int y = 0; y < bufferHeight; y++)
        {
            for (int x = bufferWidth/2; x < bufferWidth; x++)
            {
                int p = (x + bufferWidth * y) * 4;
                byte r = buffer[p + 2];
                byte g = buffer[p + 1];
                byte b = buffer[p];

                if(r > 250 && g >250 && b > 250)
                {
                    if (0 < y && y < bufferHeight / 3)
                    {
                        if (bufferWidth/2 < x && x < bufferWidth * 2 / 3)
                        {
                            ulflag++;
                        }
                        else if (bufferWidth * 2 / 3 < x && x < bufferWidth * 5 / 6)
                        {
                            umflag++;
                        }
                        else if (bufferWidth * 5 / 6 < x && x < bufferWidth)
                        {
                            urflag++;
                        }
                    }
                    if (bufferHeight / 3 < y && y < (bufferHeight * 2) / 3)
                    {
                        if (bufferWidth / 2 < x && x < bufferWidth * 2 / 3)
                        {
                            ulflag++;
                            dlflag++;
                        }
                        else if (bufferWidth * 2 / 3 < x && x < bufferWidth * 5 / 6)
                        {
                            umflag++;
                            dmflag++;
                        }
                        else if (bufferWidth * 5 / 6 < x && x < bufferWidth)
                        {
                            urflag++;
                            drflag++;
                        }
                    }
                    if ((bufferHeight * 2) / 3 < y && y < bufferHeight)
                    {
                        if (bufferWidth / 2 < x && x < bufferWidth * 2 / 3)
                        {
                            dlflag++;
                        }
                        else if (bufferWidth * 2 / 3 < x && x < bufferWidth * 5 / 6)
                        {
                            dmflag++;
                        }
                        else if (bufferWidth * 5 / 6 < x && x < bufferWidth)
                        {
                            drflag++;
                        }
                    }
                }

                

            }
        }
    }

    public void compare(byte[] prev, byte[] curr)
    {
        
        if (prev == null)
            return;
        for (int y = 0; y < bufferHeight; y++)
        {
            for (int x = 0; x < bufferWidth; x++)
            {
                int p = (x + bufferWidth * y) * 4;



                if (0 < y && y < bufferHeight / 3)
                {
                    if (0 < x && x < bufferWidth / 3)
                    {
                        if (prev[p]-10 < curr[p] && curr[p] < prev[p]+10)
                            
                        continue;
                    }
                    else if (bufferWidth / 3 < x && x < (bufferWidth * 2) / 3)
                    {
                        if (prev[p] - 100 < curr[p] && curr[p] < prev[p] + 100)
                        continue;
                    }
                    else if ((bufferWidth * 2) / 3 < x && x < bufferWidth)
                    {
                        if (prev[p] - 10 < curr[p] && curr[p] < prev[p] + 10)
                        continue;
                    }
                }
                if (bufferHeight / 3 < y && y < (bufferHeight * 2) / 3)
                {
                    if (0 < x && x < bufferWidth / 3)
                    {
                        if (prev[p] - 10 < curr[p] && curr[p] < prev[p] + 10)
                        continue;
                    }
                    else if (bufferWidth / 3 < x && x < (bufferWidth * 2) / 3)
                    {
                        if (prev[p] - 10 < curr[p] && curr[p] < prev[p] + 10)
                        continue;
                    }
                    else if ((bufferWidth * 2) / 3 < x && x < bufferWidth)
                    {
                        if (prev[p] - 10 < curr[p] && curr[p] < prev[p] + 10)
                        continue;
                    }
                }
                if ((bufferHeight * 2) / 3 < y && y < bufferHeight)
                {
                    if (0 < x && x < bufferWidth/ 3)
                    {
                        if (prev[p] - 10 < curr[p] && curr[p] < prev[p] + 10)
                        continue;
                    }
                    else if (bufferWidth / 3 < x && x < (bufferWidth * 2) / 3)
                    {
                        if (prev[p] - 10 < curr[p] && curr[p] < prev[p] + 10)
                        continue;
                    }
                    else if ((bufferWidth * 2) / 3 < x && x < bufferWidth)
                    {
                        if (prev[p] - 10 < curr[p] && curr[p] < prev[p] + 10)
                        continue;
                    }
                }

            }
        }
    }

    

}
