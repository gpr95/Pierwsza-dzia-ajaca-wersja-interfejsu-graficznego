using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNode
{
    public class ControlPacket
    {
        public const int ACCEPT = 0;
        public const int REJECT = 1;
        public const int IN_PROGRESS = 2;
        public const int RATE_1 = 1;
        public const int RATE_2 = 2;
        public const int RATE_3 = 3;
        public const int INTRADOMAIN = 0;
        public const int INTERDOMAIN = 1;
        public string virtualInterface;
        public string originIdentifier;
        public string destinationIdentifier;
        public int state;
        public int speed;
        public int domain;
        private int vc11;
        private int vc12;
        private int vc13;

        public int Vc11
        {
            get
            {
                return vc11;
            }

            set
            {
                vc11 = value;
            }
        }

        public int Vc13
        {
            get
            {
                return vc13;
            }

            set
            {
                vc13 = value;
            }
        }

        public int Vc12
        {
            get
            {
                return vc12;
            }

            set
            {
                vc12 = value;
            }
        }

        public ControlPacket(string virtualInterface,int state,int speed, string destinationIdentifier, string originIdentifier)
        {
            this.virtualInterface = virtualInterface;
            this.destinationIdentifier = destinationIdentifier;
            this.originIdentifier = originIdentifier;
            this.state = state;
            this.speed = speed;
        }

       

    }
}
