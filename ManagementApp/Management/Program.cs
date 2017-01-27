namespace Management
{
    class Program
    {
        static void Main(string[] args)
        {
            //args[0] - APPPORT, args[1] - NODEPORT
            //string[] parameters = new string[] { args[0], args[1] };
            //string[] parameters = new string[] { "7777", "7778" };
            ManagementPlane management = new ManagementPlane(args);
            
        }
    }
}
