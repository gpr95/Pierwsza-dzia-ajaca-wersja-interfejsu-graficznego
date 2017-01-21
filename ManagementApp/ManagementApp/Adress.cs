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

        Address(String addres)
        {
            String[] addressArray = addres.Split('.');
            int.TryParse(addressArray[0], out this.type);
            int.TryParse(addressArray[1], out this.filler);
            int.TryParse(addressArray[2], out this.domain);
            int.TryParse(addressArray[3], out this.space);
        }
        Address(int type, int domain, int space)
        {
            this.type = type;
            if(type == 10)
                this.filler = 0;
            else
                this.filler = 168;
            this.domain = domain;
            this.space = space;
        }
        Address(int type, int filler, int domain, int space)
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
