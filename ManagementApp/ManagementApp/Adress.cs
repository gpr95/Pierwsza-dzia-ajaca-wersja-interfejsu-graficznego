using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    public class Address
    {
        public int type;
        public int domain;
        public int subnet;
        public int space;

        public Address(String addres)
        {
            String[] addressArray = addres.Split('.');
            int.TryParse(addressArray[0], out this.type);
            int.TryParse(addressArray[1], out this.domain);
            int.TryParse(addressArray[2], out this.subnet);
            int.TryParse(addressArray[3], out this.space);
        }

        public Address(bool isClient, int domain, int subnet, int space)
        {
            if(isClient)
            {
                this.type = 192;
            }
            else
            {
                this.type = 10;
            }
            this.domain = domain;
            this.subnet = subnet;
            this.space = space;
        }

        public Address(int type, int domain, int space)
        {
            this.type = type;
            this.domain = domain;
            this.subnet = 0;
            this.space = space;
        }

        public Address(int type, int domain, int subnet, int space)
        {
            this.type = type;
            this.domain = domain;
            this.space = space;
        }

        public String getName()
        {
            return type + "." + domain + "." + subnet + "." + space;
        }
    }
}
