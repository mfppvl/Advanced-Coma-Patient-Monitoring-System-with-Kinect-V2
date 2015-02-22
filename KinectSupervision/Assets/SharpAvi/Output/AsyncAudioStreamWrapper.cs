using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;


namespace SharpAvi.Output
{
    /// <summary>
    /// Adds asynchronous writes support for underlying stream.
    /// </summary>
    internal class AsyncAudioStreamWrapper : AudioStreamWrapperBase
    {
        

        public AsyncAudioStreamWrapper(IAviAudioStreamInternal baseStream)
            : base(baseStream)
        {
            
        }

        public override void WriteBlock(byte[] data, int startIndex, int length)
        {
            base.WriteBlock(data, startIndex, length);
        }




        public override void FinishWriting()
        {
            // Perform all pending writes and then let the base stream to finish
            // (possibly writing some more data synchronously)
          //  writeInvoker.WaitForPendingInvocations();

            base.FinishWriting();
        }
    }
}
