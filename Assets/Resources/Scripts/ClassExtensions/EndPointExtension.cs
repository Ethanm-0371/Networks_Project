using System.Net;

namespace CustomExtensions
{
    public static class EndPointExtension
    {
        public static IPAddress GetIP(this EndPoint ep)
        {
            string[] subStrings = ep.ToString().Split(':');
            return IPAddress.Parse(subStrings[0]);
        }

        public static int GetPort(this EndPoint ep)
        {
            string[] subStrings = ep.ToString().Split(':');
            return int.Parse(subStrings[1]);
        }
    }
}