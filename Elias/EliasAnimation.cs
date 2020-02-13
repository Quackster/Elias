using System.Collections.Generic;

namespace EliasLibrary
{
    public class EliasAnimation
    {
        public Dictionary<int, EliasFrame> States;

        public EliasAnimation()
        {
            States = new Dictionary<int, EliasFrame>();
        }
    }
}