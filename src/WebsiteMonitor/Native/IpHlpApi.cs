using System.Net;
using System.Runtime.InteropServices;

namespace WebsiteMonitor.Native;

/// <summary>
/// P/Invoke wrapper for IP Helper API to get TCP connection information.
/// </summary>
internal static class IpHlpApi
{
    private const int AF_INET = 2;  // IPv4
    private const int TCP_TABLE_OWNER_PID_LISTENER = 3;

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetExtendedTcpTable(
        IntPtr pTcpTable,
        ref int dwOutBufLen,
        bool sort,
        int ipVersion,
        int tblClass,
        uint reserved = 0);

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_TCPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_TCPROW_OWNER_PID
    {
        public uint dwState;
        public uint dwLocalAddr;
        public uint dwLocalPort;
        public uint dwRemoteAddr;
        public uint dwRemotePort;
        public uint dwOwningPid;
    }

    /// <summary>
    /// Gets all listening TCP ports grouped by process ID.
    /// </summary>
    /// <returns>Dictionary mapping PID to list of listening ports.</returns>
    public static Dictionary<int, List<int>> GetListeningPortsByPid()
    {
        var result = new Dictionary<int, List<int>>();
        int bufferSize = 0;

        // First call to get required buffer size
        uint ret = GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET, TCP_TABLE_OWNER_PID_LISTENER);

        IntPtr tcpTablePtr = Marshal.AllocHGlobal(bufferSize);
        try
        {
            ret = GetExtendedTcpTable(tcpTablePtr, ref bufferSize, true, AF_INET, TCP_TABLE_OWNER_PID_LISTENER);

            if (ret != 0)
            {
                return result;
            }

            // Read the number of entries
            var table = Marshal.PtrToStructure<MIB_TCPTABLE_OWNER_PID>(tcpTablePtr);
            int rowSize = Marshal.SizeOf<MIB_TCPROW_OWNER_PID>();
            IntPtr rowPtr = tcpTablePtr + Marshal.SizeOf<MIB_TCPTABLE_OWNER_PID>();

            for (int i = 0; i < table.dwNumEntries; i++)
            {
                var row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);

                int pid = (int)row.dwOwningPid;
                int port = IPAddress.NetworkToHostOrder((short)row.dwLocalPort) & 0xFFFF;

                if (!result.TryGetValue(pid, out var ports))
                {
                    ports = new List<int>();
                    result[pid] = ports;
                }
                ports.Add(port);

                rowPtr += rowSize;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(tcpTablePtr);
        }

        return result;
    }
}
