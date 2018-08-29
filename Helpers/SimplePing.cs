using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JAMTech.Helpers
{
    public static class SimplePing
    {
        public static long Ping(string hostname, int timeout = 5000)
        {
            try
            {
                using (var host = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp))
                {
                    var endPoint = new IPEndPoint(Dns.GetHostAddresses(hostname)[0], 0) as EndPoint;
                    var packet = new ICMP()
                    {
                        Type = 0x08,
                        Code = 0x00,
                        Checksum = 0
                    };

                    Buffer.BlockCopy(BitConverter.GetBytes((short)1), 0, packet.Message, 0, 2);
                    Buffer.BlockCopy(BitConverter.GetBytes((short)1), 0, packet.Message, 2, 2);

                    var bitOffset = 4;
                    var data = Encoding.ASCII.GetBytes(new String('a', 32));
                    Buffer.BlockCopy(data, 0, packet.Message, bitOffset, data.Length);
                    packet.MessageSize = data.Length + bitOffset;
                    packet.Checksum = packet.getChecksum();

                    var timer = Stopwatch.StartNew();
                    var ping = host.SendToAsync(packet.getBytes(), SocketFlags.None, endPoint);
                    while (!ping.IsCompleted && timer.ElapsedMilliseconds < timeout)
                        Thread.Sleep(10);

                    if (!ping.IsCompleted) return -1;
                    data = new byte[1024];
                    var recv = host.ReceiveAsync(data, SocketFlags.None);
                    while (!recv.IsCompleted && timer.ElapsedMilliseconds < timeout)
                        Thread.Sleep(10);
                    if (!recv.IsCompleted) return -1;
                    timer.Stop();
                    host.Close();
                    //var icmp = new ICMP(data, recv.Result);
                    return timer.ElapsedMilliseconds;
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("No response from remote host");
                return -1;
            }
        }
    }

    class ICMP
    {
        public byte Type;
        public byte Code;
        public UInt16 Checksum;
        public int MessageSize;
        public byte[] Message = new byte[1024];

        public ICMP()
        {
        }

        public string ReadMessage()
        {
            if (Message != null)
                return System.Text.UTF8Encoding.Default.GetString(Message);
            return null;
        }
        public ICMP(byte[] data, int size)
        {
            Type = data[20];
            Code = data[21];
            Checksum = BitConverter.ToUInt16(data, 22);
            MessageSize = size - 24;
            Buffer.BlockCopy(data, 24, Message, 0, MessageSize);
        }

        public byte[] getBytes()
        {
            byte[] data = new byte[MessageSize + 9];
            Buffer.BlockCopy(BitConverter.GetBytes(Type), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(Code), 0, data, 1, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(Checksum), 0, data, 2, 2);
            Buffer.BlockCopy(Message, 0, data, 4, MessageSize);
            return data;
        }

        public UInt16 getChecksum()
        {
            UInt32 chcksm = 0;
            byte[] data = getBytes();
            int packetsize = MessageSize + 8;
            int index = 0;

            while (index < packetsize)
            {
                chcksm += Convert.ToUInt32(BitConverter.ToUInt16(data, index));
                index += 2;
            }
            chcksm = (chcksm >> 16) + (chcksm & 0xffff);
            chcksm += (chcksm >> 16);
            return (UInt16)(~chcksm);
        }
    }


}
