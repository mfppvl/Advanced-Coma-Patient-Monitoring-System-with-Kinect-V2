using UnityEngine;
using Windows.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;


public class KinectManager : MonoBehaviour
{
    private RenderTexture renderTexture;
    private Texture2D sourceRender;


    #region Viewers
    public int MaxFilesCount = 300;
    public GameObject depthView;
    public GameObject bodyIndexView;
    public GameObject infraredView;
    public GameObject bodyView;
    public GameObject faceView;
    public GameObject colorView;

    #endregion

    #region Buffers

    ushort[] _infraredData;
    ushort[] _depthData;

    byte[] _BodyIndexData;
    byte[] colorArray;

    byte[] depthArray;
    byte[] bodyArray;
    byte[] bodyIndexArray;
    byte[] infraredArray;

    #endregion

    #region Textures
    Texture2D infraredTexture;
    Texture2D depthTexture;
    Texture2D bodyIndexTexture;
    Texture2D bodyTexture;
    Texture2D colorTexture;
    Texture2D faceTexture;
    #endregion

    #region Kinect objects
    Dictionary<Windows.Kinect.JointType, CameraSpacePoint> lastBodyFrame = new Dictionary<JointType, CameraSpacePoint>();
    Dictionary<Windows.Kinect.JointType, float> SkeletonThreasholds = new Dictionary<JointType, float>();
    public float AnkleLeft = 0;
    public float AnkleRight = 0;
    public float ElbowLeft = 0;
    public float ElbowRight = 0;
    public float FootLeft = 0;
    public float FootRight = 0;
    public float HandLeft = 0;
    public float HandRight = 0;
    public float HandTipLeft = 0;
    public float HandTipRight = 0;
    public float Head = 0;
    public float HipLeft = 0;
    public float HipRight = 0;
    public float KneeLeft = 0;
    public float KneeRight = 0;
    public float Neck = 0;
    public float ShoulderLeft = 0;
    public float ShoulderRight = 0;
    public float SpineBase = 0;
    public float SpineMid = 0;
    public float SpineShoulder = 0;
    public float ThumbLeft = 0;
    public float ThumbRight = 0;
    public float WristLeft = 0;
    public float WristRight = 0;

    Windows.Kinect.KinectSensor Sensor;
    Windows.Kinect.ColorFrameReader colorReader;
    Windows.Kinect.DepthFrameReader depthReader;
    Windows.Kinect.BodyFrameReader bodyReader;
    Microsoft.Kinect.Face.FaceFrameSource[] faceSource;
    Microsoft.Kinect.Face.FaceFrameReader[] faceReader;
    Microsoft.Kinect.Face.FaceFrameResult[] faceFrameResults;
    Windows.Kinect.BodyIndexFrameReader bodyIndexReader;
    Windows.Kinect.InfraredFrameReader infraredReader;

    Windows.Kinect.Body[] bodies;
    Windows.Kinect.FrameDescription description;
    Windows.Kinect.CoordinateMapper mapper;
    Windows.Kinect.ColorSpacePoint[] colorSpace;
    #endregion

    #region Other objects

    public int MinVideoRecordTime = 30;
    int MaxAlertCount = 100;

    private static readonly uint[] BodyColor =
        {
            0x0000FF00,
            0x00FF0000,
            0xFFFF4000,
            0x40FFFF00,
            0xFF40FF00,
            0xFF808000,
        };


    int AlertTime = 0;
    public static Dictionary<string, string> VideoFiles = new Dictionary<string, string>();
    public static SharpAvi.Recorder videoRecorder;
    Dictionary<Microsoft.Kinect.Face.FaceProperty, DetectionResult> lastFaceResult = new Dictionary<Microsoft.Kinect.Face.FaceProperty, DetectionResult>();
    public static List<string> colorFiles = new List<string>();
    public static string WorkingDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\KinectSupervision\\";
    public static string VideoFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SkyDrive\\Documents\\";
    private const int MapDepthToByte = 8000 / 256;
    private System.IO.Stream currentVideoStream;



    public static List<string> Alerts = new List<string>();
    #endregion
    // Use this for initialization
    void Start()
    {

        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        sourceRender = new Texture2D(Screen.width, Screen.height);

        if (!System.IO.Directory.Exists(WorkingDir))
        {
            System.IO.Directory.CreateDirectory(WorkingDir);
        }
        else
        {
            System.IO.Directory.Delete(WorkingDir, true);
            System.IO.Directory.CreateDirectory(WorkingDir);
        }

        if (!System.IO.Directory.Exists(VideoFolder))
        {
            System.IO.Directory.CreateDirectory(VideoFolder);
        }



        #region initialize skeleton threasholds
        SkeletonThreasholds.Add(Windows.Kinect.JointType.AnkleLeft, AnkleLeft);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.AnkleRight, AnkleRight);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.ElbowLeft, ElbowLeft);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.ElbowRight, ElbowRight);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.FootLeft, FootLeft);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.FootRight, FootRight);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.HandLeft, HandLeft);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.HandRight, HandRight);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.HandTipLeft, HandTipLeft);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.HandTipRight, HandTipRight);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.Head, Head);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.HipLeft, HipLeft);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.HipRight, HipRight);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.KneeLeft, KneeLeft);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.KneeRight, KneeRight);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.Neck, Neck);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.ShoulderLeft, ShoulderLeft);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.ShoulderRight, ShoulderRight);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.SpineBase, SpineBase);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.SpineMid, SpineMid);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.SpineShoulder, SpineShoulder);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.ThumbLeft, ThumbLeft);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.ThumbRight, ThumbRight);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.WristLeft, WristLeft);
        SkeletonThreasholds.Add(Windows.Kinect.JointType.WristRight, WristRight);
        #endregion

        #region initialize buffer body joint array
        lastBodyFrame.Add(Windows.Kinect.JointType.AnkleLeft, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.AnkleRight, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.ElbowLeft, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.ElbowRight, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.FootLeft, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.FootRight, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.HandLeft, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.HandRight, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.HandTipLeft, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.HandTipRight, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.Head, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.HipLeft, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.HipRight, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.KneeLeft, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.KneeRight, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.Neck, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.ShoulderLeft, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.ShoulderRight, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.SpineBase, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.SpineMid, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.SpineShoulder, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.ThumbLeft, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.ThumbRight, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.WristLeft, new CameraSpacePoint());
        lastBodyFrame.Add(Windows.Kinect.JointType.WristRight, new CameraSpacePoint());
        #endregion



    }

    // Update is called once per frame
    void Update()
    {
        if (Sensor == null || !Sensor.IsOpen)
        {
            Sensor = Windows.Kinect.KinectSensor.GetDefault();
            #region start kinect
            if (Sensor != null)
            {
                Sensor.Open();
                // allocate storage to store body objects
                this.bodies = new Windows.Kinect.Body[this.Sensor.BodyFrameSource.BodyCount];
                Debug.Log("bodies count " + bodies.Length.ToString());
                // specify the required face frame results
                Microsoft.Kinect.Face.FaceFrameFeatures faceFrameFeatures =
                            Microsoft.Kinect.Face.FaceFrameFeatures.BoundingBoxInColorSpace
                    | Microsoft.Kinect.Face.FaceFrameFeatures.BoundingBoxInInfraredSpace
                    | Microsoft.Kinect.Face.FaceFrameFeatures.PointsInColorSpace
                    | Microsoft.Kinect.Face.FaceFrameFeatures.PointsInInfraredSpace
                                | Microsoft.Kinect.Face.FaceFrameFeatures.RotationOrientation
                    | Microsoft.Kinect.Face.FaceFrameFeatures.LeftEyeClosed
                    | Microsoft.Kinect.Face.FaceFrameFeatures.RightEyeClosed
                                | Microsoft.Kinect.Face.FaceFrameFeatures.LookingAway
                    | Microsoft.Kinect.Face.FaceFrameFeatures.MouthMoved
                    | Microsoft.Kinect.Face.FaceFrameFeatures.MouthOpen
                                ;
                int bodyCount = this.Sensor.BodyFrameSource.BodyCount;
                faceReader = new Microsoft.Kinect.Face.FaceFrameReader[bodyCount];
                faceSource = new Microsoft.Kinect.Face.FaceFrameSource[bodyCount];



                for (int i = 0; i < this.Sensor.BodyFrameSource.BodyCount; i++)
                {
                    faceSource[i] = Microsoft.Kinect.Face.FaceFrameSource.Create(Sensor, 0, faceFrameFeatures);//create face source object
                    faceReader[i] = faceSource[i].OpenReader();// open face reader object
                }


                // open the corresponding readers                

                colorReader = Sensor.ColorFrameSource.OpenReader();
                depthReader = Sensor.DepthFrameSource.OpenReader();
                infraredReader = Sensor.InfraredFrameSource.OpenReader();
                bodyReader = Sensor.BodyFrameSource.OpenReader();
                bodyIndexReader = Sensor.BodyIndexFrameSource.OpenReader();
                mapper = Sensor.CoordinateMapper;
                description = Sensor.ColorFrameSource.CreateFrameDescription(Windows.Kinect.ColorImageFormat.Rgba);
            }
        }

            #endregion
        if (Sensor != null && Sensor.IsOpen && Sensor.IsAvailable)
        {




            #region Color process
            if (colorReader != null)
                using (var frame = colorReader.AcquireLatestFrame())
                {
                    if (frame != null)
                    {

                        if (colorTexture == null)
                        {
                            Debug.Log(string.Format("Color Width {0}, Color Height {1}, BytesPerPixel {2}, LengthInPixels {3} ",
                                   frame.FrameDescription.Width, frame.FrameDescription.Height, frame.FrameDescription.BytesPerPixel, frame.FrameDescription.LengthInPixels));

                            colorTexture = new Texture2D(description.Width, description.Height, TextureFormat.RGBA32, false);
                            colorArray = new byte[description.BytesPerPixel * description.LengthInPixels];


                            faceTexture = new Texture2D(description.Width, description.Height, TextureFormat.RGBA32, false);
                        }
                        if (colorView != null)
                        {
                            frame.CopyConvertedFrameDataToArray(colorArray, Windows.Kinect.ColorImageFormat.Rgba);
                            colorTexture.LoadRawTextureData(colorArray);
                            colorTexture.Apply();
                            SaveToFile();
                            //SaveToFile();
                            colorView.renderer.material.mainTexture = colorTexture;
                        }
                    }
                }
            #endregion
            #region InfraredProcess

            if (infraredReader != null)
                using (var frame = infraredReader.AcquireLatestFrame())
                {
                    if (frame != null)
                    {
                        if (infraredTexture == null)
                        {
                            _infraredData = new ushort[frame.FrameDescription.LengthInPixels];
                            infraredTexture = new Texture2D(frame.FrameDescription.Width, frame.FrameDescription.Height, TextureFormat.BGRA32, false);
                            infraredArray = new byte[4 * frame.FrameDescription.LengthInPixels];
                        }
                        if (infraredView != null)
                        {
                            frame.CopyFrameDataToArray(_infraredData);
                            UpdateInfraredData();
                            infraredTexture.LoadRawTextureData(infraredArray);
                            infraredTexture.Apply();
                            infraredView.renderer.material.mainTexture = infraredTexture;
                        }
                    }
                }

            #endregion
            #region Depth process

            if (depthReader != null)
                using (var frame = depthReader.AcquireLatestFrame())
                {
                    if (frame != null)
                    {
                        using (Windows.Kinect.KinectBuffer depthBuffer = frame.LockImageBuffer())
                        {
                            if (depthTexture == null)
                            {
                                Debug.Log(string.Format("Width {0}, Height {1} ", frame.FrameDescription.Width, frame.FrameDescription.Height));
                                _depthData = new ushort[frame.FrameDescription.LengthInPixels];
                                //colorSpace = new Windows.Kinect.ColorSpacePoint[frame.FrameDescription.LengthInPixels];
                                depthTexture = new Texture2D(frame.FrameDescription.Width, frame.FrameDescription.Height, TextureFormat.RGBA32, false);
                                depthArray = new byte[4 * frame.FrameDescription.Width * frame.FrameDescription.Height];

                            }
                            if (depthView != null)
                            {
                                ushort maxDepth = ushort.MaxValue;
                                frame.CopyFrameDataToArray(_depthData);
                                UpdateDepthData(depthBuffer.Length, frame.DepthMinReliableDistance, maxDepth);
                                depthTexture.LoadRawTextureData(depthArray);
                                depthTexture.Apply();
                                depthView.renderer.material.mainTexture = depthTexture;
                            }
                        }
                    }
                }

            #endregion
            #region BodyIndex process

            if (depthReader != null)
                using (var frame = bodyIndexReader.AcquireLatestFrame())
                {
                    if (frame != null)
                    {
                        using (Windows.Kinect.KinectBuffer depthBuffer = frame.LockImageBuffer())
                        {
                            if (bodyIndexTexture == null)
                            {
                                Debug.Log(string.Format("Body index Width {0}, Body index Height {1}, BytesPerPixel {2}, LengthInPixels {3} ",
                                    frame.FrameDescription.Width, frame.FrameDescription.Height, frame.FrameDescription.BytesPerPixel, frame.FrameDescription.LengthInPixels));
                                _BodyIndexData = new byte[frame.FrameDescription.LengthInPixels * frame.FrameDescription.BytesPerPixel];
                                //colorSpace = new Windows.Kinect.ColorSpacePoint[frame.FrameDescription.LengthInPixels];
                                bodyIndexTexture = new Texture2D(frame.FrameDescription.Width, frame.FrameDescription.Height, TextureFormat.RGBA32, false);
                                bodyIndexArray = new byte[frame.FrameDescription.Width * frame.FrameDescription.Height * 4];
                                bodyTexture = new Texture2D(frame.FrameDescription.Width, frame.FrameDescription.Height, TextureFormat.RGBA32, false);
                                bodyArray = new byte[frame.FrameDescription.Width * frame.FrameDescription.Height * 4];

                            } if (bodyIndexView != null)
                            {
                                frame.CopyFrameDataToArray(_BodyIndexData);
                                UpdateBodyIndexData();
                                bodyIndexTexture.LoadRawTextureData(bodyIndexArray);
                                bodyIndexTexture.Apply();
                                bodyIndexView.renderer.material.mainTexture = bodyIndexTexture;
                            }
                        }
                    }
                }

            #endregion
            #region Body process
            if (bodyReader != null)
            {
                using (var frame = bodyReader.AcquireLatestFrame())
                {
                    if (frame != null)
                    {

                        if (bodyView != null)
                        {
                            frame.GetAndRefreshBodyData(bodies);
                            UpdateBodyData();
                            bodyView.renderer.material.mainTexture = bodyTexture;

                        }
                    }
                }
            }
            #endregion
            #region Face process

            for (int i = 0; i < this.bodies.Length; i++)
            {
                // check if a valid face is tracked in this face source		

                if (faceSource[i].IsTrackingIdValid)
                {
                    using (var frame = faceReader[i].AcquireLatestFrame())
                    {
                        if (frame != null)
                        {
                            var result = frame.FaceFrameResult;
                            var rect = result.FaceBoundingBoxInColorSpace;
                            //faceTexture.SetPixels(whiteArea);
                            int w = rect.Bottom - rect.Top;
                            int h = rect.Right - rect.Left;
                            int x = rect.Left - w / 2;
                            int y = rect.Top - h / 2;
                            if (x < 0)
                                x = 0;
                            if (y < 0)
                                y = 0;
                            if (w + x > colorTexture.width)
                                w = colorTexture.width - x;
                            if (h + y > colorTexture.height)
                                h = colorTexture.height - y;

                            if (w != 0 && h != 0)
                            {
                                //Debug.Log(string.Format("x {0}, y {1}, width {2}, height {3}",
                                //    x, y, w, h));
                                try
                                {
                                    var c = colorTexture.GetPixels(x, y, w * 2, h * 2);
                                    faceTexture.Resize(w * 2, h * 2);
                                    faceTexture.SetPixels(c);
                                    faceTexture.Apply();
                                    faceView.renderer.material.mainTexture = faceTexture;
                                    CheckFace(result);
                                }
                                catch {  Debug.Log(string.Format("x {0}, y {1}, width {2}, height {3}",
                                    x, y, w, h));};
                            }
                        }
                    }
                }
                else
                {
                    // check if the corresponding body is tracked 
                    if (bodies[i] != null && bodies[i].IsTracked)
                    {
                        // update the face frame source to track this body
                        faceSource[i].TrackingId = bodies[i].TrackingId;

                    }
                }
            }

            #endregion

        }
        if (videoRecorder!=null && AlertTime > 0 && Environment.TickCount - AlertTime >= MinVideoRecordTime * 1000)
        {

            VideoFiles.Add(videoRecorder.ShortName, videoRecorder.FileName );
            videoRecorder.Dispose();
            
            videoRecorder = null;
            AlertTime = 0;
            
            Debug.Log("Video stopped");
        }
    }

    void OnApplicationQuit()
    {
        if (videoRecorder != null)
            videoRecorder.Dispose();
    }


    
    

    #region Processing methods
    public void CheckFace(Microsoft.Kinect.Face.FaceFrameResult res)
    {
        if (lastFaceResult.Count == 0)
        {
            foreach (var f in res.FaceProperties)
            {   
                lastFaceResult.Add(f.Key, f.Value);
            }
        }
        else
        {
            foreach (var f1 in res.FaceProperties)
            {
                if (f1.Key!= Microsoft.Kinect.Face.FaceProperty.Engaged
                    &&
                    f1.Key != Microsoft.Kinect.Face.FaceProperty.Happy
                    &&
                    f1.Key != Microsoft.Kinect.Face.FaceProperty.WearingGlasses
                    &&
                    f1.Value != lastFaceResult[f1.Key] && f1.Value != DetectionResult.Maybe)
                {
                    string s = string.Format("face event {0}, old value {1}, new value {2}", f1.Key, lastFaceResult[f1.Key], f1.Value);
                    AddAlert(s);
                    return;
                }
            }
        }
    }



    private void SaveToFile()
    {


        camera.targetTexture = renderTexture;
        camera.Render();
        RenderTexture.active = renderTexture;
        sourceRender.ReadPixels(new Rect(0.0f, 0.0f, renderTexture.width, renderTexture.height), 0, 0);
      

        //int tm1 = Environment.TickCount;
        if (colorFiles.Count >= MaxFilesCount)
        {
            try
            {
                File.Delete(colorFiles[0]);
                colorFiles.RemoveAt(0);
            }
            catch { };
        }

        var filename = WorkingDir + "\\" + Guid.NewGuid().ToString() + ".jpg";
        var bytes= sourceRender.EncodeToJPG(70);
        File.WriteAllBytes(filename, bytes);
        colorFiles.Add(filename);

    }

    /// <summary>
    /// Save screenshot to jpeg file for continiosly writing to video
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="files"></param>



    /// <summary>
    /// process infrared data
    /// </summary>
    private void UpdateInfraredData()
    {
        int index = 0;
        foreach (var ir in _infraredData)
        {
            byte intensity = (byte)(ir >> 8);
            infraredArray[index++] = intensity;
            infraredArray[index++] = intensity;
            infraredArray[index++] = intensity;
            infraredArray[index++] = 255; // Alpha
        }
    }

    /// <summary>
    /// process depth data
    /// </summary>
    /// <param name="depthFrameDataSize"></param>
    /// <param name="minDepth"></param>
    /// <param name="maxDepth"></param>
    private void UpdateDepthData(uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
    {
        // convert depth to a visual representation
        for (int i = 0; i < depthArray.Length; i += 4)
        {
            // Get the depth for this pixel
            // Get the depth for this pixel
            ushort depth = _depthData[i / 4];

            // To convert to a byte, we're mapping the depth value to the byte range.
            // Values outside the reliable depth range are mapped to 0 (black).
            this.depthArray[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            this.depthArray[i + 1] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            this.depthArray[i + 2] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            this.depthArray[i + 3] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
        }
    }

    /// <summary>
    /// process body index data
    /// </summary>
    private void UpdateBodyIndexData()
    {
        if (colorArray == null)
            return;
        Resize(description.Width, description.Height, bodyIndexTexture.width, bodyIndexTexture.height);

        // convert body index to a visual representation
        for (int i = 0; i < bodyIndexArray.Length; i += 4)
        {

            // the BodyColor array has been sized to match
            // BodyFrameSource.BodyCount
            if (_BodyIndexData[i / 4] != 255)
            {
                continue;
            }
            else
            {
                // this pixel is not part of a player
                // display black
                this.bodyIndexArray[i] = 255;
                this.bodyIndexArray[i + 1] = 255;
                this.bodyIndexArray[i + 2] = 255;
                this.bodyIndexArray[i + 3] = 255;
            }

        }
    }
    /// <summary>
    /// process
    /// </summary>
    private void UpdateBodyData()
    {
        if (bodyTexture == null)
            return;
        // convert body index to a visual representation
        for (int i = 0; i < bodyIndexArray.Length; i += 4)
        {

            // the BodyColor array has been sized to match
            // BodyFrameSource.BodyCount
            if (_BodyIndexData[i / 4] != 255)
            {
                this.bodyArray[i] = (byte)BodyColor[_BodyIndexData[i / 4]];
                this.bodyArray[i + 1] = (byte)BodyColor[_BodyIndexData[i / 4]];
                this.bodyArray[i + 2] = (byte)BodyColor[_BodyIndexData[i / 4]];
                this.bodyArray[i + 3] = (byte)BodyColor[_BodyIndexData[i / 4]];
            }
            else
            {
                this.bodyArray[i] = 255;
                this.bodyArray[i + 1] = 255;
                this.bodyArray[i + 2] = 255;
                this.bodyArray[i + 3] = 255;

            }


        }
        bodyTexture.LoadRawTextureData(bodyArray);
        foreach (var body in bodies)
        {
            if (body.IsTracked)
            {
                for (int i = 0; i < body.Joints.Count; i++)
                {
                    JointType current = (JointType)i;
                    JointType parent = GetParentJoint((JointType)i);
                    if (body.Joints[(JointType)i].TrackingState != TrackingState.NotTracked &&
                       body.Joints[current].TrackingState != TrackingState.NotTracked)
                    {
                        Vector2 posParent = MapSpacePointToDepthCoords(body.Joints[parent].Position);
                        Vector2 posJoint = MapSpacePointToDepthCoords(body.Joints[current].Position);

                        if (posParent != Vector2.zero && posJoint != Vector2.zero)
                        {
                            //Color lineColor = playerJointsTracked[i] && playerJointsTracked[parent] ? Color.red : Color.yellow;
                            DrawLine(bodyTexture, (int)posParent.x, (int)posParent.y, (int)posJoint.x, (int)posJoint.y, Color.yellow);

                        }
                    }
                }

                if (CheckJoints(body))
                    UpdateJointValues(body);
            }
            bodyTexture.Apply();
        }
        //aTexture.Apply();
    }

    /// <summary>
    /// check joints for changes to alert 
    /// </summary>
    /// <param name="newBody"></param>
    /// <returns></returns>
    private bool CheckJoints(Body newBody)
    {
        foreach (var joint in newBody.Joints)
        {
            if (joint.Value.TrackingState != TrackingState.NotTracked)
            {

                if (Math.Abs(Math.Abs(joint.Value.Position.X) - Math.Abs(lastBodyFrame[joint.Key].X)) >
                    SkeletonThreasholds[joint.Key])
                {
                    string s = string.Format("Alert! Joint {0} old value X = {1}, new value X = {2}", joint.Key,
                        lastBodyFrame[joint.Key].X,
                        joint.Value.Position.X);
                    AddAlert(s);

                    return true;
                }
                if (Math.Abs(Math.Abs(joint.Value.Position.Y) - Math.Abs(lastBodyFrame[joint.Key].Y)) >
                    SkeletonThreasholds[joint.Key])
                {
                    string s = string.Format("Alert! Joint {0} old value Y = {1}, new value Y = {2}", joint.Key,
                         lastBodyFrame[joint.Key].Y,
                         joint.Value.Position.Y);
                    AddAlert(s);

                    return true;
                }
                if (Math.Abs(Math.Abs(joint.Value.Position.Z) - Math.Abs(lastBodyFrame[joint.Key].Z)) >
                    SkeletonThreasholds[joint.Key])
                {
                    string s = string.Format("Alert! Joint {0} old value Z = {1}, new value Z = {2}", joint.Key,
                         lastBodyFrame[joint.Key].Z,
                         joint.Value.Position.Z);
                    AddAlert(s);

                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// update last body frame to check next one
    /// </summary>
    /// <param name="newbody"></param>
    private void UpdateJointValues(Body newbody)
    {
        foreach (var joint in newbody.Joints)
        {
            JointType key = joint.Key;
            lastBodyFrame[key] = new CameraSpacePoint()
            {
                X = joint.Value.Position.X,
                Y = joint.Value.Position.Y,
                Z = joint.Value.Position.Z
            };
        }
    }

    /// <summary>
    /// resize color array image to new one with specific size
    /// </summary>
    /// <param name="oldWidth"></param>
    /// <param name="oldHeight"></param>
    /// <param name="newWidth"></param>
    /// <param name="newHeight"></param>
    private unsafe void Resize(int oldWidth, int oldHeight, int newWidth, int newHeight)
    {

        IntPtr sourceImg = Marshal.AllocHGlobal(colorArray.Length);
        IntPtr destImg = Marshal.AllocHGlobal(bodyIndexArray.Length);
        Marshal.Copy(colorArray, 0, sourceImg, colorArray.Length);
        Marshal.Copy(bodyIndexArray, 0, destImg, bodyIndexArray.Length);
        // get source image size
        int width = oldWidth;
        int height = oldHeight;

        int pixelSize = 4;
        int srcStride = oldWidth * 4;
        int dstOffset = newWidth * 4 - pixelSize * newWidth;
        double xFactor = (double)width / newWidth;
        double yFactor = (double)height / newHeight;

        // do the job
        byte* src = (byte*)sourceImg;
        byte* dst = (byte*)destImg;
        // coordinates of source points
        double ox, oy, dx1, dy1, dx2, dy2;
        int ox1, oy1, ox2, oy2;
        // width and height decreased by 1
        int ymax = height - 1;
        int xmax = width - 1;
        // temporary pointers
        byte* tp1, tp2;
        byte* p1, p2, p3, p4;

        // for each line
        for (int y = 0; y < newHeight; y++)
        {
            // Y coordinates
            oy = (double)y * yFactor;
            oy1 = (int)oy;
            oy2 = (oy1 == ymax) ? oy1 : oy1 + 1;
            dy1 = oy - (double)oy1;
            dy2 = 1.0 - dy1;

            // get temp pointers
            tp1 = src + oy1 * srcStride;
            tp2 = src + oy2 * srcStride;

            // for each pixel
            for (int x = 0; x < newWidth; x++)
            {
                // X coordinates
                ox = (double)x * xFactor;
                ox1 = (int)ox;
                ox2 = (ox1 == xmax) ? ox1 : ox1 + 1;
                dx1 = ox - (double)ox1;
                dx2 = 1.0 - dx1;

                // get four points
                p1 = tp1 + ox1 * pixelSize;
                p2 = tp1 + ox2 * pixelSize;
                p3 = tp2 + ox1 * pixelSize;
                p4 = tp2 + ox2 * pixelSize;

                // interpolate using 4 points
                for (int i = 0; i < pixelSize; i++, dst++, p1++, p2++, p3++, p4++)
                {
                    *dst = (byte)(
                        dy2 * (dx2 * (*p1) + dx1 * (*p2)) +
                        dy1 * (dx2 * (*p3) + dx1 * (*p4)));
                }
            }
            dst += dstOffset;
        }

        Marshal.Copy(destImg, bodyIndexArray, 0, bodyIndexArray.Length);
        Marshal.FreeHGlobal(destImg);
        Marshal.FreeHGlobal(sourceImg);
    }

    /// <summary>
    /// Draw skeleton lines
    /// </summary>
    /// <param name="a_Texture"></param>
    /// <param name="x1"></param>
    /// <param name="y1"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    /// <param name="a_Color"></param>
    private void DrawLine(Texture2D a_Texture, int x1, int y1, int x2, int y2, Color a_Color)
    {

        int width = a_Texture.width;
        int height = a_Texture.height;

        int dy = y2 - y1;
        int dx = x2 - x1;

        int stepy = 1;
        if (dy < 0)
        {
            dy = -dy;
            stepy = -1;
        }

        int stepx = 1;
        if (dx < 0)
        {
            dx = -dx;
            stepx = -1;
        }

        dy <<= 1;
        dx <<= 1;

        if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    a_Texture.SetPixel(x1 + x, y1 + y, a_Color);

        if (dx > dy)
        {
            int fraction = dy - (dx >> 1);

            while (x1 != x2)
            {
                if (fraction >= 0)
                {
                    y1 += stepy;
                    fraction -= dx;
                }

                x1 += stepx;
                fraction += dy;

                if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                            a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
            }
        }
        else
        {
            int fraction = dx - (dy >> 1);

            while (y1 != y2)
            {
                if (fraction >= 0)
                {
                    x1 += stepx;
                    fraction -= dy;
                }

                y1 += stepy;
                fraction += dx;

                if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                            a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
            }
        }

    }

    /// <summary>
    /// get parent joint of this one if exist 
    /// </summary>
    /// <param name="joint"></param>
    /// <returns></returns>
    private JointType GetParentJoint(JointType joint)
    {
        switch (joint)
        {
            case JointType.SpineBase:
                return JointType.SpineBase;

            case JointType.Neck:
                return JointType.SpineShoulder;

            case JointType.SpineShoulder:
                return JointType.SpineMid;

            case JointType.ShoulderLeft:
            case JointType.ShoulderRight:
                return JointType.SpineShoulder;

            case JointType.HipLeft:
            case JointType.HipRight:
                return JointType.SpineBase;

            case JointType.HandTipLeft:
                return JointType.HandLeft;

            case JointType.ThumbLeft:
                return JointType.WristLeft;

            case JointType.HandTipRight:
                return JointType.HandRight;

            case JointType.ThumbRight:
                return JointType.WristRight;
        }

        return (JointType)((int)joint - 1);
    }
    /// <summary>
    /// Get depth coordinates from skeleton point
    /// </summary>
    /// <param name="spacePos"></param>
    /// <returns></returns>
    private Vector2 MapSpacePointToDepthCoords(CameraSpacePoint spacePos)
    {
        Vector2 vPoint = Vector2.zero;
        CameraSpacePoint camPoint = new CameraSpacePoint();
        camPoint.X = spacePos.X;
        camPoint.Y = spacePos.Y;
        camPoint.Z = spacePos.Z;

        CameraSpacePoint[] camPoints = new CameraSpacePoint[1];
        camPoints[0] = camPoint;

        DepthSpacePoint[] depthPoints = new DepthSpacePoint[1];
        mapper.MapCameraPointsToDepthSpace(camPoints, depthPoints);

        DepthSpacePoint depthPoint = depthPoints[0];

        if (depthPoint.X >= 0 && depthPoint.X < depthTexture.width &&
           depthPoint.Y >= 0 && depthPoint.Y < depthTexture.height)
        {
            vPoint.x = depthPoint.X;
            vPoint.y = depthPoint.Y;
        }


        return vPoint;
    }

    int alertCount = 0;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    public void AddAlert(string s)
    {
        if (videoRecorder == null)
        {
            string fname = DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss") + ".avi";
            videoRecorder = new SharpAvi.Recorder(VideoFolder + fname, fname, SharpAvi.KnownFourCCs.Codecs.MotionJpeg, 70, 0, renderTexture.width, renderTexture.height, true);

        }
        AlertTime = Environment.TickCount;
        alertCount++;
        Debug.Log(s);

        if (Alerts.Count > MaxAlertCount)
        {
            Alerts.RemoveAt(0);
        }

        Alerts.Add(DateTime.Now.ToString("HH:mm:ss - ") + alertCount.ToString() + ") " + s);
    }
    #endregion
    

}
