using System;
using Lib.Render;

namespace Lib
{

public partial class MyImGuiRenderer
{
    private struct GcData
    {
        public float[][] GcBuffer;
        public int[] LastGcVal;

        public float[] FullGcBuffer;
        public int FullGcLastVal;

        public bool ResetOnNextUpdate;

        public static GcData Default
        {
            get
            {
                GcData t = new()
                {
                    GcBuffer = new float[3][],
                    FullGcBuffer = new float[PlotBufferSize],
                    LastGcVal = new int[3],
                    FullGcLastVal = 0
                };

                for (int i = 0; i < t.GcBuffer.Length; i++)
                    t.GcBuffer[i] = new float[PlotBufferSize];

                return t;
            }
        }

        private void Reset()
        {
            ResetOnNextUpdate = false;
            for (int i = 0; i < LastGcVal.Length; i++)
                Array.Clear(GcBuffer[i], 0, GcBuffer[i].Length);
            Array.Clear(FullGcBuffer, 0, FullGcBuffer.Length);
        }

        public void Update(UpdateInfo updateInfo)
        {
            if (ResetOnNextUpdate)
                Reset();

            for (int i = 0; i < LastGcVal.Length; i++)
            {
                float newVal = LastGcVal[i] == updateInfo.gc[i] ? 0 : 1;
                UpdateBuffer(GcBuffer[i], newVal);
                LastGcVal[i] = updateInfo.gc[i];
            }

            {
                float newVal = updateInfo.FullGcApproaching ? 0.1f :
                    FullGcLastVal == updateInfo.FullGc ? 0 : 1;
                UpdateBuffer(FullGcBuffer, newVal);
                FullGcLastVal = updateInfo.FullGc;
            }
        }
    }
}

}