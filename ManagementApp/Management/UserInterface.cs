using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Management
{
    static class UserInterface
    {
        private static ManagementPlane management;
        private static OPERATION operation;
        private static Dictionary<int, Node> nodeDictionary;

        private enum OPERATION
        {ENTRY, TABLE, SHOW, CLEAR, NONE }

        internal static ManagementPlane Management
        {
            get
            {
                return management;
            }

            set
            {
                management = value;
            }
        }

        public static void showMenu()
        {
            Boolean quit = false;
            while (!quit)
            {
                //Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\n MENU: ");
                Console.WriteLine("\n\t 1) Insert forwarding entry to Node");
                Console.WriteLine("\n\t 2) Insert forwarding table to Node");
                Console.WriteLine("\n\t 3) Show connection table of Node");
                Console.WriteLine("\n\t 4) Clear connection table of Node");
                Console.WriteLine("\n");

                int choice;
                bool res = int.TryParse(Console.ReadLine(), out choice);
                if (res)
                {
                    switch (choice)
                    {
                        case 1:
                            operation = OPERATION.ENTRY;
                            log("#DEBUG1", ConsoleColor.Magenta);
                            management.getNodes();
                            break;
                        case 2:
                            operation = OPERATION.TABLE;
                            management.getNodes();
                            break;
                        case 3:
                            operation = OPERATION.SHOW;
                            management.getNodes();
                            break;
                        case 4:
                            operation = OPERATION.CLEAR;
                            management.getNodes();
                            break;
                        default:
                            operation = OPERATION.NONE;
                            Console.WriteLine("\n Wrong option");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Wrong format");
                    showMenu();
                }
            }
        }

        public static void nodeList(List<Node> nodeList)
        {
            log("#DEBUG3", ConsoleColor.Magenta);
            nodeDictionary = new Dictionary<int, Node>();
            int enumerate = 1;
            Console.ForegroundColor = ConsoleColor.White;
            foreach (Node node in nodeList)
            {
                Console.WriteLine(enumerate + ") " + node.Name);
                nodeDictionary.Add(enumerate++, node);
            }
            String s;
            Node n = null;
            log("#DEBUG3.1", ConsoleColor.Magenta);
            if (nodeDictionary.Count != 0)
                while (true)
                {
                    s = Console.ReadLine();
                    if (s.Equals("q"))
                        break;
                    int choice;
                    bool res = int.TryParse(s, out choice);
                    nodeDictionary.TryGetValue(choice, out n);
                    if (n != null)
                        break;
                }
            if (n == null)
                return;
            if(operation != OPERATION.CLEAR)
                management.getInterfaces(n);
            log("#DEBUG3.2", ConsoleColor.Magenta);
            switch (operation)
            {
                case OPERATION.ENTRY:
                    
                    while (true)
                    {
                        log("Please enter forwarding entry: /n", ConsoleColor.White);
                        log("(Foramt: port 1/contener 1/port 2/contener2) /n", ConsoleColor.Blue);
                        s = Console.ReadLine();
                        if (s.Split('/').Length == 3)
                            break;
                        else
                            log("Wrong format, try again.", ConsoleColor.DarkRed);
                    }
                    break;
                case OPERATION.TABLE:

                    break;
                case OPERATION.SHOW:

                    break;
                case OPERATION.CLEAR:

                    break;
                default:
                    operation = OPERATION.NONE;
                    break;
            }
        }

        internal static void showInterfaces(Dictionary<int, string> dictionary)
        {
            foreach (var row in dictionary)
                log("Interface: " + row.Key + " connected to: " + row.Value, ConsoleColor.Blue);
        }

        public static void log(String msg, ConsoleColor cc)
        {
            Console.ForegroundColor = cc;
            Console.Write(DateTime.Now.ToLongTimeString() + ": " + msg);
            Console.Write(Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
