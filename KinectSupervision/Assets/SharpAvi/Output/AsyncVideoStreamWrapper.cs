using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;



namespace SharpAvi.Output
{
    /// <summary>
    /// Adds asynchronous writes support for underlying stream.
    /// </summary>
    internal class AsyncVideoStreamWrapper : VideoStreamWrapperBase
    {
        

        public AsyncVideoStreamWrapper(IAviVideoStreamInternal baseStream)
            : base(baseStream)
        {
            
        }

        public override void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            base.WriteFrame(isKeyFrame, frameData, startIndex, length);
        }


     


        public override void FinishWriting()
        {
            // Perform all pending writes and then let the base stream to finish
            // (possibly writing some more data synchronously)
            //writeInvoker.WaitForPendingInvocations();

            base.FinishWriting();
        }
    }
}
