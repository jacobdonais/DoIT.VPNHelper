using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace TAMUVPNApplication {
    /* Constants used by this application. Please make edits here */
    static class Constants {
        public const int MAX_ATTEMPTS = 5;
        public const string RASDIAL   = "rasdial.exe";
        public const string VPN_NAME  = "TAMU VPN";
        public const string LOG_DIR   = @".\Logs\";
    }

    /* Log is used for logging the program */
    class Log {
        private StreamWriter log;
        private int status;

        /* Default constructor. Will create a file with date format if filename is null. */
        public Log(string filename = null) {
            EnsureDirectoryExists(Constants.LOG_DIR);

            if (filename == null) {
                filename = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_log.txt";
            }
            log = new StreamWriter(Constants.LOG_DIR + filename);

            log.WriteLine(">>> [TAMU VPN - https://github.com/jacobdonais/DoIT.VPNHelper]");
            log.WriteLine(">>> " +
                          DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") +
                          ": Section start");
            status = 0;
            log.AutoFlush = true;
        }

        /* Finalizer for log. Will close the log file. */
        ~Log() {
            log.WriteLine("<<< [" +
                          DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") +
                          ": Section end]");
            log.WriteLine("<<< [Exit Status(" + status + ")]");
            log.Close();
        }

        /* Name:        SetStatus
        /* Description: This function is used to set the status of the log. */
        public void SetStatus(int _status) {
            status = _status;
        }

        /* Name:        WriteLine
        /* Description: This function is used to write a line with a newline in the log. */
        public void WriteLine(string msg) {
            log.WriteLine(msg);
        }

        /* Name:        Write
        /* Description: This function is used to write a line with no newline in the log. */
        public void Write(string msg) {
            log.Write(msg);
        }

        /* Name:        EnsureDirectoryExists
        /* Description: This function is used to ensure a directory exists. */
        private void EnsureDirectoryExists(string filePath) { 
            FileInfo fi = new FileInfo(filePath);
            if (!fi.Directory.Exists) { 
                System.IO.Directory.CreateDirectory(fi.DirectoryName); 
            } 
        }
    }

    class TAMUVPN {
        /* Name:        GetPassword()
        /* Description: Function will read the keys pressed to create the
        password string. Will accept the password once the enter key is pressed and
        will accept backspace to delete a character. */
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

        /* Name:        CreateVPN()
        /* Description: This command will use PowerShell to create the VPN.
        Configuration for the VPN comes from the L2TP guide from TAMU. */
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

        /* Name:        DeleteVPN()
        /* Description: This will delete the VPN off the computer. */
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

        /* Name:        ConnectVPN()
        /* Description: This function will connect to the VPN.*/
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

        /* Name:        TerminateVPN()
        /* Description: This function will end the VPN session */
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

        /* Name:        GetIPAddress()
        /* Description: This function will search for the IP address for the TAMU
        VPN and will return it. If it isn't found, then it will return null. */
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

        static void Main(string[] args) {
            string username = null;
            string password = null;
            string ip =       null;
            int count =       Constants.MAX_ATTEMPTS;
            string hostName = Dns.GetHostName();
            string logName =  DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_log.txt";

            // 0. Create the Log file
            Log log = new Log(logName);
            
            // 1. Create the VPN
            log.WriteLine(" Creating the VPN...");
            try {
                CreateVPN();
                log.WriteLine("  Successfully created the VPN.");
            }
            catch {
                log.WriteLine("  Failed to create the VPN.");
                log.SetStatus(-1);
                throw new System.ArgumentException("Failed to create the VPN");
            }
            
            // 2. Get the user's username and password
            Console.WriteLine("Please enter your NetID credentials.");
            Console.Write("Username: ");
            username = Console.ReadLine();
            Console.Write("Passowrd: ");
            password = GetPassword();
            log.WriteLine(" Using username = " + username);

            // 3/4. Connect to the VPN and get the IP address
            log.WriteLine(" Attempting to connect to TAMU VPN...");
            while(ip == null && count > 0) {
                log.WriteLine("  Attempt = " + (Constants.MAX_ATTEMPTS - count + 1) + " out of " + Constants.MAX_ATTEMPTS);

                try {
                    ConnectVPN(username, password);
                }
                catch {
                    log.SetStatus(-2);
                    throw new System.ArgumentException("Failed to execute ConnectVPN()");
                }

                try {
                    ip = GetIPAddress();
                }
                catch {
                    log.SetStatus(-3);
                    throw new System.ArgumentException("Failed to get IP address");
                }
                count--;
            }
            if (count <= 0) {
                Console.WriteLine("Failed to connect to TAMU VPN; please try again later.");
                log.WriteLine("  Failed to conect to TAMU VPN.");
                log.SetStatus(-4);
                return;
            }
            else {
                // 5. Output the info to the user.
                log.WriteLine("  Attempts needed = " + (Constants.MAX_ATTEMPTS - count));
                log.WriteLine("  Successfully connected at " + DateTime.Now.ToString("HH:mm:ss yyyy/MM/dd"));
                Console.WriteLine();
                Console.WriteLine("Hostname: " + hostName);
                log.WriteLine("  Hostname: " + hostName);
                Console.WriteLine("IP Address: " + ip);
                log.WriteLine("  IP Address: " + ip);
                Console.WriteLine();

                Console.Write("Press f to disconnect");
                while (Console.ReadKey(true).Key != ConsoleKey.F) {
                }
                
                // 6. Disconnect from the VPN
                log.WriteLine(" Attempting to disconnect from VPN...");
                try {
                    TerminateVPN();
                    log.WriteLine("  Successfully disconnected at " + DateTime.Now.ToString("HH:mm:ss yyyy/MM/dd"));
                }
                catch {
                    log.WriteLine("  Failed to disconnect from the VPN.");
                    log.SetStatus(-5);
                    throw new System.ArgumentException("Failed to execute TerminateVPN()");
                }
            }
            
            // 7. Delete the VPN
            log.WriteLine(" Attempting to delete the VPN...");
            try {
                DeleteVPN();
                log.WriteLine("  Successfully deleted the VPN.");
            }
            catch {
                log.WriteLine("  Failed to delete the VPN.");
                log.SetStatus(-6);
                throw new System.ArgumentException("Failed to execute TerminateVPN()");
            }

            log.SetStatus(1);
        }
    }
}
