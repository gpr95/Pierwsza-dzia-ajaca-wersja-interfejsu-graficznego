using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementApp
{
    public class Node
    {
        protected Point position;
        protected String name;
        protected const int GAP = 10;
        protected Thread threadHandle;
        //Porty
        public Point Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public Thread ThreadHandle
        {
            get
            {
                return threadHandle;
            }

            set
            {
                threadHandle = value;
            }
        }
    }
}
