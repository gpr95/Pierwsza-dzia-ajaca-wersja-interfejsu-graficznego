using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNode
{
    public class ControlInterface
    {
        //CPCC
        public static string CALL_REQUEST = "call request"; //call request accept NCC log
        public static string CALL_ACCEPT = "call accept";
        public static string CALL_RELEASE_OUT = "call release out";
        public static string CALL_RELEASE_IN = "call release in";
        public static string CALL_MODIFICATION_REQUEST = "call modification request"; //?
        public static string CALL_MODIFICATION_ACCEPT = "call_modification_accept";
        //NCC
        public static string CALL_REQUEST_ACCEPT = "call request accept";
        public static string NETWORK_CALL_COORDINATION_IN = "network call coordination in";
        public static string NETWORK_CALL_COORDINATION_OUT = "network call coordination out";
        public static string CALL_INDICATION = "call indication"; //wywołanie klienta docelowego, dla formalnosci NCC->CPCC
        public static string CALL_MODIFICATION_INDICATION = "call modification indication"; //zmiana parametrow polaczenia NCC->CPCC
        public static string CONNECTION_REQUEST_OUT = "connection request out";
    }
}
