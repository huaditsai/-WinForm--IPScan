using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace IPScan
{
    class IpDeal
    {
        private uint IpToInt(string newIp)//convert ip String to int
        {
            string[] strIPArray = newIp.Split('.');
            uint[] iIP = new uint[strIPArray.Length];

            for (int i = 0; i < strIPArray.Length; ++i)
                iIP[i] = uint.Parse(strIPArray[i]);

            return (iIP[0] << 24) | (iIP[1] << 16) | (iIP[2] << 8) | iIP[3];
        }
        private string IpToString(uint newIp)//convert ip int to String
        {
            return IPAddress.Parse(newIp.ToString()).ToString();
        }

        public string[] IpAll(string ip1, string ip2)//all ip in range
        {
            string[] allip = new string[IpToInt(ip2) - IpToInt(ip1) + 1];

            for (uint i = 0; i <= IpToInt(ip2) - IpToInt(ip1); i++)
            {
                allip[i] = IpToString(IpToInt(ip1) + i);
            }
            return allip;
        }

        //get hostname with timeout
        private delegate IPHostEntry GetHostEntryHandler(string ip);
        public string GetReverseDNS(string ip, int timeout)
        {
            try
            {
                GetHostEntryHandler callback = new GetHostEntryHandler(Dns.GetHostEntry);
                IAsyncResult result = callback.BeginInvoke(ip, null, null);
                if (result.AsyncWaitHandle.WaitOne(timeout, false))
                {
                    return callback.EndInvoke(result).HostName;
                }
                else
                {
                    return "n/a";
                }
            }
            catch (SocketException se)
            {
                return se.Message;
            }
        }


    }
}
