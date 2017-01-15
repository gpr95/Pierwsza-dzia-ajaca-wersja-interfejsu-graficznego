using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNode
{
    class ControlProtocol
    {
        //CPCC
        public static string CALL_REQUEST = "call_request"; //call request accept NCC log
        public static string CALL_ACCEPT = "call_accept";
        public static string CALL_RELEASE_OUT = "call_release_out";
        public static string CALL_RELEASE_IN = "call_release_in";
        public static string CALL_MODIFICATION_REQUEST = "call_modification_request"; //?
        public static string CALL_MODIFICATION_ACCEPT = "call_modification_accept";
        //NCC
        public static string NETWORK_CALL_COORDINATION_IN = "network_call_coordination_in";
        public static string NETWORK_CALL_COORDINATION_OUT = "network_call_coordination_out";
        public static string CALL_INDICATION = "call_indication"; //wywołanie klienta docelowego, dla formalnosci NCC->CPCC
        public static string CALL_MODIFICATION_INDICATION = "call_modification_indication"; //zmiana parametrow polaczenia NCC->CPP
    }
}
