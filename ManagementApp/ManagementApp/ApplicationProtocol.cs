using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    public class ApplicationProtocol
    {
        public const int CONNECTIONTONCC = 0;
        public const int KILL = 1;
        public const int TOOTHERNCC = 2;
        public const int D = 3;
        public const int E = 4;
        public const int F = 5;
        public const int G = 6;
        public const int H = 7;

        private int state;
        private List<String> connectionToNcc;
        private List<int> connectionToOtherNcc;

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

        public List<String> ConnectionToNcc
        {
            get
            {
                return connectionToNcc;
            }

            set
            {
                connectionToNcc = value;
            }
        }

        public List<int> ConnectionToOtherNcc
        {
            get
            {
                return connectionToOtherNcc;
            }

            set
            {
                connectionToOtherNcc = value;
            }
        }
    }
}
