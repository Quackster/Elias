using System.Collections.Generic;

namespace EliasLibrary
{
    public class EliasFrame
    {
        public int Loop = -1;
        public int FramesPerSecond = -1;
        public List<string> Frames;

        public EliasFrame()
        {
            this.Frames = new List<string>();
        }
    }
}