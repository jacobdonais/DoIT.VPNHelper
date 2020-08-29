using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace TAMUVPNApplication {
    static class Constants {
        public const int MAX_ATTEMPTS = 5;
        public const string RASDIAL = "rasdial.exe";
        public const string VPN_NAME = "TAMU VPN";
        public const string LOG_DIR = @".\Logs\";
    }

    class TAMUVPN {
        static string GetPassword() {
            string password = "";
            System.ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter) {
                if (info.Key != ConsoleKey.Backspace) {
                    password += info.KeyChar;
                }
                else {
                    if (!string.IsNullOrEmpty(password)) {
                        password = password.Substring(0, password.Length - 1);
                    }
                }
                info = Console.ReadKey(true);
            }
            return password;
        }

        static void CreateVPN() {
            try {
                PowerShell ps = PowerShell.Create();
                    ps.AddCommand("Add-VpnConnection");
                    ps.AddParameter("Name", Constants.VPN_NAME);
                    ps.AddParameter("ServerAddress", "connect.tamu.edu");
                    ps.AddParameter("TunnelType", "L2tp");
                    ps.AddParameter("L2tpPsk", "tamuvpn");
                    ps.AddParameter("EncryptionLevel", "Maximum");
                    ps.AddParameter("Force");
                    ps.Invoke();
            }
            catch {}
        }

        static void DeleteVPN() {
            try {
                PowerShell ps = PowerShell.Create();
                    ps.AddCommand("Remove-VpnConnection");
                    ps.AddParameter("Name", Constants.VPN_NAME);
                    ps.AddParameter("Force");
                    ps.Invoke();
            }
            catch {}
        }

        static void ConnectVPN(string username, string password) {
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = Constants.RASDIAL,
                    Arguments = "\"" + Constants.VPN_NAME + "\" " + username + " " + password,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                }
            };
            process.Start();
            process.WaitForExit();
        }

        static void TerminateVPN() {
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = Constants.RASDIAL, 
                    Arguments = "\"" + Constants.VPN_NAME + "\" /d",
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
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
            CreateVPN();

            string logName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_log.txt";
            EnsureDirectoryExists(Constants.LOG_DIR);
            StreamWriter log = new StreamWriter(Constants.LOG_DIR + logName);

            Console.WriteLine("Please enter your NetID credentials");
            string username = null;
            string password = null;
            Console.Write("Username: ");
            username = Console.ReadLine();
            Console.Write("Passowrd: ");
            password = GetPassword();

            string ip = null;
            int count = Constants.MAX_ATTEMPTS;
            log.WriteLine("Attempting to connect to TAMU VPN...");
            while(ip == null && count > 0) {
                ConnectVPN(username, password);
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
            DeleteVPN();
            log.Close();
        }
    }
}