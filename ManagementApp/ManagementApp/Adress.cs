using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    class Address
    {
        private int type;
        private int filler;
        private int domain;
        private int space;

        public Address(String addres)
        {
            String[] addressArray = addres.Split('.');
            int.TryParse(addressArray[0], out this.type);
            int.TryParse(addressArray[1], out this.filler);
            int.TryParse(addressArray[2], out this.domain);
            int.TryParse(addressArray[3], out this.space);
        }

        public Address(bool isClient, int domain, int space)
        {
            if(isClient)
            {
                this.type = 192;
                this.filler = 168;
            }
            else
            {
                this.type = 10;
                this.filler = 0;
            }
            this.domain = domain;
            this.space = space;
        }

        public Address(int type, int domain, int space)
        {
            this.type = type;
            if(type == 10)
                this.filler = 0;
            else
                this.filler = 168;
            this.domain = domain;
            this.space = space;
        }

        public Address(int type, int filler, int domain, int space)
        {
            this.type = type;
            this.filler = filler;
            this.domain = domain;
            this.space = space;
        }

        public String getName()
        {
            return type + "." + filler + "." + domain + "." + space;
        }
    }
}
