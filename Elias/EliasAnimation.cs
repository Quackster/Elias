using System.Collections.Generic;

namespace EliasLibrary
{
    public class EliasAnimation
    {
        public int Loop = -1;
        public int FramesPerSecond = -1;
        public Dictionary<int, EliasFrame> States;

        public EliasAnimation()
        {
            States = new Dictionary<int, EliasFrame>();
        }
    }
}