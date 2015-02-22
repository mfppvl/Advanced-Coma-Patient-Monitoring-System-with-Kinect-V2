using System;
using System.Diagnostics;

using System.Runtime.InteropServices;
using System.Threading;
using SharpAvi.Codecs;
using SharpAvi.Output;


namespace SharpAvi
{
    public class Recorder : IDisposable
    {
        private readonly bool AllowConvertion = false;
        private readonly int Width;
        private readonly int Height;
        private readonly AviWriter writer;
        private readonly IAviVideoStream videoStream;
        
        
        private readonly Thread screenThread;        
        private readonly AutoResetEvent videoFrameWritten = new AutoResetEvent(false);
        

        public  string FileName = "";
        public string ShortName = "";
        public Recorder(string fileName,string shortName,
            FourCC codec, int quality,
            int audioBitRate,
            int width,
            int height, bool convert)
        {

            ShortName = shortName;
            AllowConvertion = convert;
            FileName = fileName;
            Width = width;
            Height = height;

            // Create AVI writer and specify FPS
            writer = new AviWriter(fileName)
            {
                FramesPerSecond = 10,
                EmitIndex1 = true,
            };

            // Create video stream
            videoStream = CreateVideoStream(codec, quality);
            // Set only name. Other properties were when creating stream, 
            // either explicitly by arguments or implicitly by the encoder used
            videoStream.Name = "Screencast";

            screenThread = new Thread(RecordScreen)
            {
                Name = typeof(Recorder).Name + ".RecordScreen",
                IsBackground = true
            };


            while (KinectManager.colorFiles.Count > 0)
            {

                var buffer = System.IO.File.ReadAllBytes(KinectManager.colorFiles[0]);
                videoStream.WriteFrame(true, buffer, 0, buffer.Length);
                System.IO.File.Delete(KinectManager.colorFiles[0]);
                KinectManager.colorFiles.RemoveAt(0);



            }
            screenThread.Start();



        }
        private IAviVideoStream CreateVideoStream(FourCC codec, int quality)
        {
            // Select encoder type based on FOURCC of codec
            if (codec == KnownFourCCs.Codecs.Uncompressed)
            {
                return writer.AddUncompressedVideoStream(Width, Height);
            }
            else
            {
                UnityEngine.Debug.Log(string.Format("CreateVideoStream w {0}, h {1} ",Width, Height));
                return writer.AddMotionJpegVideoStream(Width, Height, quality

);
            }
        }

        

        


        public void Dispose()
        {
            stopFlag = true;
            screenThread.Join();
          
            // Close writer: the remaining data is written to a file and file is closed
            writer.Close();
            

           
        }
        bool stopFlag = false;

        
        private void RecordScreen()
        {

            
            var frameInterval = TimeSpan.FromSeconds(1 / (double)writer.FramesPerSecond);
            
            var isFirstFrame = true;
            var timeTillNextFrame = TimeSpan.Zero;
            while (!stopFlag)
            {
                var timestamp = DateTime.Now;


                if (KinectManager.colorFiles.Count > 0)
                {
                    var buffer = System.IO.File.ReadAllBytes(KinectManager.colorFiles[0]);
                    videoStream.WriteFrame(true, buffer, 0, buffer.Length);
                    System.IO.File.Delete(KinectManager.colorFiles[0]);
                    KinectManager.colorFiles.RemoveAt(0);
                }
                else
                    Thread.Sleep(30);

                timeTillNextFrame = timestamp + frameInterval - DateTime.Now;
                if (timeTillNextFrame < TimeSpan.Zero)
                    timeTillNextFrame = TimeSpan.Zero;

                isFirstFrame = false;
            }




        }

        private bool IsAudioWriting = false;
       
    }
}
