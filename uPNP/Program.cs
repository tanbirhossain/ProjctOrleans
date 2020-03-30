
using System;
using Libarius.Network;

namespace uPNP
{
    class Program
    {


        static void Main(string[] args)
        {
           
            Console.WriteLine("Discover started ");
            Console.WriteLine("-------------------");
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Your Private Ip: {0}", IpHelper.PrivateIpAddress);
                Console.WriteLine("Your public Ip: {0}", IpHelper.PublicIpAddress);
                Console.WriteLine(UPnP.Discover());
                Console.WriteLine("You have an UPnP-enabled router and your IP is: " + UPnP.ExternalIp);
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Discover Error occurred.", ex.Message);
                Console.WriteLine("Your router doesn't support Discover.");
                Console.WriteLine("Dont't worry , one day you will find your desire solution.");
            }
            Console.ResetColor();
            Console.WriteLine("--------------------");
            Console.WriteLine("Discover Finished");
            Console.ReadKey();

        }
  
    }
}
