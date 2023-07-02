using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSpriteMaker
{
    internal class AnimationGroup
    {

        public byte FrameStart { get; set; } = 0;
        public byte FrameEnd { get; set; } = 0;
        public byte FrameSpeed { get; set; } = 0;
        public string FrameName { get; set; } = "NoName";

        public AnimationGroup(byte frameStart, byte frameEnd, byte frameSpeed, string frameName)
        {
            this.FrameStart = frameStart;
            this.FrameEnd = frameEnd;
            this.FrameSpeed = frameSpeed;
            this.FrameName = frameName;
        }

        public override string ToString()
        {
            return FrameName + " : " + FrameStart.ToString("D2") + "-" + FrameEnd.ToString("D2") + "  Timer " + FrameSpeed.ToString("D3");
        }
    }
}
