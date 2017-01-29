using ManagementApp;
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
        private static Boolean quit = false;

        private static String nodeStart = null;

        private enum OPERATION
        { ENTRY, TABLE, SOFT, SHOW, INTERFACES, CLEAR, NONE }

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

        public static bool Quit
        {
            get
            {
                return quit;
            }

            set
            {
                quit = value;
            }
        }

        public static void showMenu()
        {
            while (!Quit)
            {
                //Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\n MENU: ");
                Console.WriteLine("\n --- Hard  Pernament --- ");
                Console.WriteLine("\n\t 1) Insert forwarding entry to Node");
                Console.WriteLine("\n\t 2) Insert forwarding table to Node");
                Console.WriteLine("\n --- Soft  Pernament --- ");
                Console.WriteLine("\n\t 3) Create soft pernament trail");
                Console.WriteLine("\n --- More Operations --- ");
                Console.WriteLine("\n\t 4) Show connection table of Node");
                Console.WriteLine("\n\t 5) Show interfaces of Node");
                Console.WriteLine("\n\t 6) Clear connection table of Node");
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
                            operation = OPERATION.SOFT;
                            management.getNodes(true);
                            break;
                        case 4:
                            operation = OPERATION.SHOW;
                            management.getNodes();
                            break;
                        case 5:
                            operation = OPERATION.INTERFACES;
                            management.getNodes();
                            break;
                        case 6:
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

        internal static void showDomain(int aPPLICATIONPORT)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            int temp = aPPLICATIONPORT - 7777;
            Console.WriteLine("\nManagement for domain: " + temp);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Title = "D" + temp + " Management";
        }

        public static void nodeList(List<Node> nodeList, bool onlyClients)
        {
            //log("#DEBUG3", ConsoleColor.Magenta);
            nodeDictionary = new Dictionary<int, Node>();
            int enumerate = 1;
            Console.ForegroundColor = ConsoleColor.White;
            foreach (Node node in nodeList)
            {
                if (onlyClients)
                {
                    if(node.Name.Contains("192."))
                    {
                        Console.WriteLine(enumerate + ") " + node.Name);
                        nodeDictionary.Add(enumerate++, node);
                    }
                }
                else
                {
                    Console.WriteLine(enumerate + ") " + node.Name);
                    nodeDictionary.Add(enumerate++, node);
                }
                
            }
            String s;
            Node n = null;
            //log("#DEBUG3.1", ConsoleColor.Magenta);
            Address a = null;
            if (nodeDictionary.Count != 0)
                while (true)
                {
                    s = Console.ReadLine();
                    if (s.Equals("q"))
                        return;
                    
                    try
                    {
                        a = new Address(s);
                        break;
                    }catch(Exception e)
                    {
                        int choice;
                        bool res = int.TryParse(s, out choice);
                        nodeDictionary.TryGetValue(choice, out n);
                        if (n != null)
                            break;
                    }
                }
            //if (n == null)
            //    return;
            if(!(operation == OPERATION.CLEAR || operation == OPERATION.SOFT))
                management.getInterfaces(n);
            //log("#DEBUG3.2", ConsoleColor.Magenta);
            switch (operation)
            {
                case OPERATION.ENTRY:
                    while (true)
                    {
                        log("Please enter forwarding entry: ", ConsoleColor.White);
                        log("Foramt: port 1/container 1/port 2/container 2 ", ConsoleColor.Cyan);
                        s = Console.ReadLine();
                        if (s.Split('/').Length == 4)
                        {
                            management.sendEntry(n, s);
                            break;
                        }
                        else if (s.Equals("q"))
                            break;
                        else
                            log("Wrong format, try again.", ConsoleColor.Red);
                    }
                    break;
                case OPERATION.TABLE:
                        log("Please enter forwarding table: ", ConsoleColor.White);
                        log("Foramt: port 1/container 1/port 2/container 2\nTo finish configuration enter 'end'", ConsoleColor.Cyan);
                        List<String> tableList = new List<string>();
                        while (true)
                        {
                            s = Console.ReadLine();
                            if (s.Split('/').Length == 4)
                            {
                                tableList.Add(s);
                            }
                            else if (s.Equals("end"))
                                break;
                            else if (s.Equals("q"))
                                return;
                            else
                                log("Wrong format, please try again.", ConsoleColor.Red);
                        }
                        management.sendTable(n, tableList);
                    break;
                case OPERATION.SOFT:
                    if (nodeStart == null)
                    {
                        if (n == null)
                            nodeStart = a.ToString();
                        else
                            nodeStart = n.Name;
                        management.getNodes(true);
                    }
                    else
                    {
                        if (n == null)
                            getSpeed(a.ToString());
                        else
                            getSpeed(n.Name);
                        nodeStart = null;
                    } 
                    break;
                case OPERATION.SHOW:
                    management.sendShowTable(n);
                    break;
                case OPERATION.INTERFACES:
                    break;
                case OPERATION.CLEAR:
                    log("Are you sure?", ConsoleColor.Red);
                    s = Console.ReadLine();
                    if (s.Equals("y"))
                        management.sendClear(n);
                    break;
                default:
                    operation = OPERATION.NONE;
                    break;
            }
        }

        private static void getSpeed(String n)
        {
            log("Enter speed(1, 2, 3):", ConsoleColor.Blue);
            String s = Console.ReadLine();
            int i = 0;
            int.TryParse(s, out i);
            management.createSoft(nodeStart, n, i);
        }

        internal static void showTable(List<FIB> routingTable)
        {
            foreach (var temp in routingTable)
            {
                Console.WriteLine(temp.toString());
            }
        }

        internal static void showInterfaces(Dictionary<int, string> dictionary)
        {
            foreach (var row in dictionary)
                log("Interface: " + row.Key + " connected to: " + row.Value, ConsoleColor.Cyan);
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
