namespace ManagementApp
{
    static class PortAggregation
    {
        static private int cableCloudPort = 7776;
        static private int managementPort = 7777;
        static private int managementNodePort = 7700;
        static private int clientPort = 8000;
        static private int netPort = 8500;
        static private int nccPort = 9000;

        public static int CableCloudPort
        {
            get
            {
                return cableCloudPort++;
            }
        }

        public static int ManagementPort
        {
            get
            {
                return managementPort++;
            }
        }

        public static int ClientPort
        {
            get
            {
                return clientPort++;
            }
        }

        public static int NetPort
        {
            get
            {
                return netPort++;
            }
        }

        public static int NccPort
        {
            get
            {
                return nccPort++;
            }
        }

        public static int ManagementNodePort
        {
            get
            {
                return managementNodePort++;
            }
        }
    }
}
