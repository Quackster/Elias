using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elias
{
    class EliasDirection
    {
        public SortedDictionary<int, int> Coords;

        public string Props
        {
            get { return Coords[0] + "," + Coords[1] + "," + Coords[2] + "," + Coords[3] + "," + Coords[4] + "," + Coords[5] + "," + Coords[6] + "," + Coords[7]; }
        }

        public EliasDirection()
        {
            this.Coords = new SortedDictionary<int, int>();
            this.Coords.Add(0, 0);
            this.Coords.Add(1, 0);
            this.Coords.Add(2, 0);
            this.Coords.Add(3, 0);
            this.Coords.Add(4, 0);
            this.Coords.Add(5, 0);
            this.Coords.Add(6, 0);
            this.Coords.Add(7, 0);
        }
    }
}
