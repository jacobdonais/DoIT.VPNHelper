using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace TAMUVPNApplication {
    static class Constants {
        public const int MAX_ATTEMPTS = 5;
        public const string RASPHONE = @"C:\Windows\System32\rasphone.exe";
        public const string TAMU_VPN = @".\Resources\TAMU_VPN.pbk";
        public const string VPN_NAME = "TAMU VPN";
        public const string LOG_DIR = @".\Logs\";
    }

    class TAMUVPN {
        static void ConnectVPN() {
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = Constants.RASPHONE, 
                    Arguments = "-f " + Constants.TAMU_VPN + " -d \"" + Constants.VPN_NAME + "\""
                }
            };
            process.Start();
            process.WaitForExit();
        }

        static void TerminateVPN() {
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = Constants.RASPHONE, 
                    Arguments = "-h \"" + Constants.VPN_NAME + "\""
                }
            };
            process.Start();
            process.WaitForExit();
        }

        static string GetIPAddress() {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            NetworkInterface TAMU_VPN = Array.Find(networkInterfaces, element => element.Name == Constants.VPN_NAME);

            try {
                foreach (UnicastIPAddressInformation ip in TAMU_VPN.GetIPProperties().UnicastAddresses) {
                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                        return ip.Address.ToString();
                    }
                }
            }
            catch {
                return null;
            }
            return null;
        }

        static void EnsureDirectoryExists(string filePath) { 
            FileInfo fi = new FileInfo(filePath);
            if (!fi.Directory.Exists) { 
                System.IO.Directory.CreateDirectory(fi.DirectoryName); 
            } 
        }

        static void Main(string[] args) {
            if (!File.Exists(Constants.RASPHONE)) {
                Console.WriteLine("Program incorrectly configured. Missing " + Constants.RASPHONE);
                Console.ReadKey();
                return;
            }
            if (!File.Exists(Constants.TAMU_VPN)) {
                Console.WriteLine("Program incorrectly configured. Missing " + Constants.TAMU_VPN);
                Console.ReadKey();
                return;
            }

            string logName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_log.txt";
            EnsureDirectoryExists(Constants.LOG_DIR);
            StreamWriter log = new StreamWriter(Constants.LOG_DIR + logName);

            Console.WriteLine("Please enter your NetID credentials on the next window. Check phone for DUO push after entering credentials.");
            string ip = null;
            int count = Constants.MAX_ATTEMPTS;
            log.WriteLine("Attempting to connect to TAMU VPN...");
            while(ip == null && count > 0) {
                ConnectVPN();
                ip = GetIPAddress();
                count--;
            }
            if (count <= 0) {
                Console.WriteLine("Failed to connect to TAMU VPN; please try again later.");
                log.WriteLine("Failed to conect to TAMU VPN.");
                log.Close();
                return;
            }

            log.WriteLine("..attempts needed: " + (Constants.MAX_ATTEMPTS - count));
            log.WriteLine("Successfully connected at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            string HostName = Dns.GetHostName();
            Console.WriteLine();
            Console.WriteLine("Hostname: " + HostName);
            log.WriteLine("Hostname: " + HostName);
            Console.WriteLine("IP Address: " + ip);
            log.WriteLine("IP Address: " + ip);
            Console.WriteLine();

            Console.Write("Press f to disconnect");
            while (Console.ReadKey(true).Key != ConsoleKey.F) {
            }
            TerminateVPN();
            log.WriteLine("Successfully disconnected at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            log.Close();
        }
    }
}