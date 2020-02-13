using System.Collections.Generic;

namespace EliasLibrary
{
    public class EliasAnimation
    {
        public int Loop = -1;
        public int FramesPerSecond = -1;
        public Dictionary<int, List<string>> Frames;

        public EliasAnimation()
        {
            Frames = new Dictionary<int, List<string>>();
        }
    }
}