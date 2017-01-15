using ClientWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ClientWindow
{
    class CPCC
    { 
        private ClientWindow clientWindowHandler;
        private int controlPort;
    
        public CPCC(ClientWindow clientWindowHandler)
        {
            this.clientWindowHandler = clientWindowHandler;
            this.readConfig();
        }

        private void readConfig()
        {
            XDocument doc = XDocument.Load("config.xml");
            string value = doc.XPathSelectElement("//config[1]/controlPort").Value;
            bool res = int.TryParse(value, out controlPort);
        }
    }
}
