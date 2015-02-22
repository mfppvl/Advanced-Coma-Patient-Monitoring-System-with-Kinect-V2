﻿using System;

namespace SharpAvi.Output
{
    internal class AviVideoStream : AviStreamBase, IAviVideoStreamInternal
    {
        private readonly IAviStreamWriteHandler writeHandler;
        private FourCC streamCodec;
        private int width;
        private int height;
        private BitsPerPixel bitsPerPixel;
        private int framesWritten;

        public AviVideoStream(int index, IAviStreamWriteHandler writeHandler, 
            int width, int height, BitsPerPixel bitsPerPixel)
            : base(index)
        {
         

            this.writeHandler = writeHandler;
            this.width = width;
            this.height = height;
            this.bitsPerPixel = bitsPerPixel;
            this.streamCodec = KnownFourCCs.Codecs.Uncompressed;
        }


        public int Width
        {
            get { return width; }
            set
            {
                CheckNotFrozen();
                width = value;
            }
        }

        public int Height
        {
            get { return height; }
            set
            {
                CheckNotFrozen();
                height = value;
            }
        }

        public BitsPerPixel BitsPerPixel
        {
            get { return bitsPerPixel; }
            set
            {
                CheckNotFrozen();
                bitsPerPixel = value;
            }
        }

        public FourCC Codec
        {
            get { return streamCodec; }
            set
            {
                CheckNotFrozen();
                streamCodec = value;
            }
        }

        public void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int count)
        {
            writeHandler.WriteVideoFrame(this, isKeyFrame, frameData, startIndex, count);
            System.Threading.Interlocked.Increment(ref framesWritten);
        }




        public int FramesWritten
        {
            get { return framesWritten; }
        }


        public override FourCC StreamType
        {
            get { return KnownFourCCs.StreamTypes.Video; }
        }

        protected override FourCC GenerateChunkId()
        {
            return KnownFourCCs.Chunks.VideoFrame(Index, Codec != KnownFourCCs.Codecs.Uncompressed);
        }

        public override void WriteHeader()
        {
            writeHandler.WriteStreamHeader(this);
        }

        public override void WriteFormat()
        {
            writeHandler.WriteStreamFormat(this);
        }
    }
}
