using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SSO.Util.Client
{
    public class TcpListenerHelper
    {
        int port = 0;
        int buffer = 256;
        public TcpListenerHelper(int port, int buffer = 256)
        {
            this.port = port;
            this.buffer = buffer;
        }
        public void Run(Action<string> action)
        {
            IPAddress localAddr = IPAddress.Parse(GetLocalIp());
            TcpListener server = new TcpListener(localAddr, port);
            try
            {
                server.Start();
                Byte[] bytes = new Byte[buffer];
                String data = null;
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    data = null;
                    NetworkStream stream = client.GetStream();
                    int i;
                    try
                    {
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                            action(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log4Net.ErrorLog(ex);
                    }
                    client.Close();
                }
            }
            catch (SocketException ex)
            {
                Log4Net.ErrorLog(ex);
            }
            finally
            {
                server.Stop();
            }
        }
        public string GetLocalIp()
        {
            ///获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }
    }
}
