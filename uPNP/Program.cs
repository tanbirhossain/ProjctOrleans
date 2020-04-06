
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Libarius.Network;
using Open.Nat;

namespace uPNP
{
    class Program
    {
        public static int _neoTcpPort = 1701;
        public static int _openTcpPort = 1702;
        public static int _localTcpPort = 1703;

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            bool isOpenPort = false;
            Console.WriteLine("© Mohammed Tanbir Hosssain \n");

            Console.WriteLine("Discover started ");
            Console.WriteLine($"N_Port:{_neoTcpPort}");
            Console.WriteLine($"O_Port:{_openTcpPort}");
            Console.WriteLine($"Local_Port:{_localTcpPort}");
            Console.WriteLine("-----------------------------");

            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Your Private Ip: {0}", IpHelper.PrivateIpAddress);
                Console.WriteLine("Your public Ip: {0}", IpHelper.PublicIpAddress);

                //----
                Console.WriteLine("-----------------------------");
                Console.WriteLine("Looking for N style discovering");
                if (DiscoverNeo())
                {

                    StartListener_Neo();
                    isOpenPort = true;
                    Console.WriteLine($"You _N_port: {_neoTcpPort}  listing. please go to https://canyouseeme.org/ and confirm.");
                }
                else
                {
                    Console.WriteLine($"Failed : Sorry _N_  port {_neoTcpPort} ");
                }
                Console.WriteLine("-----------------------------");
                Console.WriteLine("Looking for O style discovering");
                if (await DiscoverOpenNatAsync())
                {
                    //listener
                    StartListener_Open();
                    isOpenPort = true;
                    Console.WriteLine($"You _O_port: { _openTcpPort}  listing. please go to https://canyouseeme.org/ and confirm.");
                }
                else
                {
                    Console.WriteLine($"Failed : Sorry _0_  port {_openTcpPort} ");
                }
                Console.WriteLine("-----------------------------");
                Console.WriteLine("Looking for local discovering");
                if (DiscoverLocal())
                {
                    StartListener_Local();
                    Console.WriteLine($"You _L_port: { _localTcpPort}  listing. please go to https://canyouseeme.org/ and confirm.");
                }
                else
                {
                    Console.WriteLine($"Failed : Sorry _L_  port {_localTcpPort} ");
                }

                Console.WriteLine("-----------------------------");
                //Delete port 
                if (isOpenPort)
                {
                    Console.WriteLine("Are you want to delete your mapping? ");
                    Console.WriteLine("Type 'Y' OR 'N'");
                    var ans = Console.ReadLine();
                    if (ans.ToLower() == "y")
                    {
                        try {await DeleteOpenPortAsync(); } catch (Exception) { }
                        try { DeleteNeoPort(); } catch (Exception) { }
                    }

                    Console.WriteLine(" UPnp Mapping List");
                    Console.WriteLine("-----------------------------");
                    var discoverer = new NatDiscoverer();

                    // we don't want to discover forever, just 5 senconds or less
                    var cts = new CancellationTokenSource(5000);

                    // we are only interested in Upnp NATs because PMP protocol doesn't allow to list mappings
                    var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

                    foreach (var mapping in await device.GetAllMappingsAsync())
                    {
                        Console.WriteLine(mapping);
                    }
                    Console.WriteLine("-----------------------------");

                }


            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Discover Error occurred.", ex.Message);
                Console.WriteLine("Your router doesn't support Discover.");
                Console.WriteLine("Dont't worry , one day you will find your desire solution.");
            }
            Console.ResetColor();
            Console.WriteLine("-----------------------------");
            Console.WriteLine("Discover Finished");
            Console.ReadKey();

        }


        private static bool DiscoverNeo()
        {
            if (UPnP.Discover())
            {
                Console.WriteLine("You have an UPnP-enabled router and your IP is: " + UPnP.ExternalIp);
                UPnP.ForwardPort(_neoTcpPort, ProtocolType.Tcp, "NEO Tcp");

                return true;
            }
            return false;
        }
        private static bool DeleteNeoPort()
        {
            UPnP.DeleteForwardingRule(_neoTcpPort, ProtocolType.Tcp);
            return true;
        }
        private static async System.Threading.Tasks.Task<bool> DeleteOpenPortAsync()
        {
            var discoverer = new NatDiscoverer();

            // we don't want to discover forever, just 5 senconds or less
            var cts = new CancellationTokenSource(5000);

            // we are only interested in Upnp NATs because PMP protocol doesn't allow to list mappings
            var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
            foreach (var mapping in await device.GetAllMappingsAsync())
            {
                // in this example we want to delete the "Skype" mappings
                if (mapping.Description.Contains("OPEN"))
                {
                    Console.WriteLine("Deleting {0}", mapping);
                    await device.DeletePortMapAsync(mapping);
                }
            }
            return true;
        }

        private static bool DiscoverLocal()
        {


            return true;

        }

        private static async System.Threading.Tasks.Task<bool> DiscoverOpenNatAsync()
        {

            try
            {
                var discoverer = new NatDiscoverer();

                // we don't want to discover forever, just 5 senconds or less
                var cts = new CancellationTokenSource(5000);

                // we are only interested in Upnp NATs because PMP protocol doesn't allow to list mappings
                var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

                foreach (var mapping in await device.GetAllMappingsAsync())
                {
                    Console.WriteLine(mapping);
                }


                foreach (var mapping in await device.GetAllMappingsAsync())
                {
                    // in this example we want to delete the "Skype" mappings
                    if (mapping.Description.Contains("OPEN"))
                    {
                        Console.WriteLine("Deleting {0}", mapping);
                        await device.DeletePortMapAsync(mapping);
                    }
                }
                // display the NAT's IP address
                Console.WriteLine("The external IP Address is: {0} ", await device.GetExternalIPAsync());

                // create a new mapping in the router [external_ip:1702 -> host_machine:1602]
                await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, _openTcpPort, _openTcpPort, "Discover For OPEN"));


            }
            catch (NatDeviceNotFoundException e)
            {
                Console.WriteLine("wasn't able to find an Upnp device ;(");
                return false;
            }
            catch (MappingException me)
            {
                switch (me.ErrorCode)
                {
                    case 718:
                        Console.WriteLine("The external port already in use.");
                        StartListener_Open();
                        break;
                    case 728:
                        Console.WriteLine("The router's mapping table is full.");
                        return true;
                        break;
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }


        private static void StartListener_Neo()
        {
            // configure a TCP socket listening on port 1602
            var endPoint = new IPEndPoint(IPAddress.Any, _neoTcpPort);
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            socket.Bind(endPoint);
            socket.Listen(4);
        }
        private static void StartListener_Open()
        {
            // configure a TCP socket listening on port 1602
            var endPoint = new IPEndPoint(IPAddress.Any, _openTcpPort);
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            socket.Bind(endPoint);
            socket.Listen(4);
        }
        private static void StartListener_Local()
        {
            // configure a TCP socket listening on port 1602
            var endPoint = new IPEndPoint(IPAddress.Any, _localTcpPort);
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            socket.Bind(endPoint);
            socket.Listen(4);
        }
    }
}


//dotnet publish -c Release -r linux-x64 /p:PublishSingleFile=true
//https://qiita.com/kaysquare1231/items/700be91a1f4d410c1d5e