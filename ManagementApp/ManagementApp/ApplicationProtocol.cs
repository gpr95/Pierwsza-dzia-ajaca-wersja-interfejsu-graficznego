using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    public class ApplicationProtocol
    {
        public static readonly int A = 0;
        public static readonly int KILL = 1;
        public static readonly int C = 2;
        public static readonly int D = 3;
        public static readonly int E = 4;
        public static readonly int F = 5;
        public static readonly int G = 6;
        public static readonly int H = 7;

        private int state;

        public int State
        {
            get
            {
                return state;
            }

            set
            {
                state = value;
            }
        }
    }
}
