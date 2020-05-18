using System;
using System.Threading;
using SomeProject.Library.Server;

namespace SomeProject.TcpServer
{
    public class EnteringPointServer
    {
        static void Main(string[] args)
        {
           try
           {
                Server server = new Server();
                server.TurnOnListener().Wait();
                
                //server.turnOffListener();
           }
            catch(Exception e)
           {
                Console.WriteLine(e.Message);
           }

            Console.ReadKey();
        }
    }
}
