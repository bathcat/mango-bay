using System;
using System.Net;
using System.Net.Sockets;

namespace MBC.Endpoints.Security;

public static class IPAddressExtensions
{
    public static IPAddress GetSubnet(this IPAddress ipAddress)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = ipAddress.GetAddressBytes();
            bytes[3] = 0;
            return new IPAddress(bytes);
        }

        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            var bytes = ipAddress.GetAddressBytes();
            Array.Clear(bytes, 8, 8);
            return new IPAddress(bytes);
        }

        throw new ArgumentException($"Unsupported address family: {ipAddress.AddressFamily}", nameof(ipAddress));
    }
}

