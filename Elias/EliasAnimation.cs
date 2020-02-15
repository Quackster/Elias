using System.Collections.Generic;

namespace EliasLibrary
{
    public class EliasAnimation
    {
        public SortedDictionary<int, EliasFrame> States;

        public EliasAnimation()
        {
            States = new SortedDictionary<int, EliasFrame>();
        }
    }
}