using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;

namespace CommerceTraining.SupportingClasses
{
    public static class CheckIfOnLine
    {
        public static bool IsInternetAvailable
        {
            get { return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() && _CanPingGoogle(); }
        }

        private static bool _CanPingGoogle()
        {
            const int timeout = 1000;
            const string host = "google.com";

            var ping = new Ping();
            var buffer = new byte[32];
            var pingOptions = new PingOptions();

            try
            {
                var reply = ping.Send(host, timeout, buffer, pingOptions);
                return (reply != null && reply.Status == IPStatus.Success);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool GetTheBool()
        {
            return false;
        }


    }
}