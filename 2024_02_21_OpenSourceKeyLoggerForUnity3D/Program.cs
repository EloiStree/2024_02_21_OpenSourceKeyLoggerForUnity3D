using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace KeyboardInputConsoleApp
{
    class Program
    {
        // UDP configuration
        const int Port = 12346;
        static readonly IPAddress BroadcastAddress = IPAddress.Parse("127.0.0.1"); // Change to the desired broadcast IP address

        // Import user32.dll for accessing GetAsyncKeyState function
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        static bool[] previousKeyState = new bool[256]; // Array to keep track of previous key states

        static void Main(string[] args)
        {
            int commandId = 565464;
            Console.WriteLine("Cmd Int: "+ commandId);
            Console.WriteLine("Press any key (press 'q' to quit):");

            // Create UDP client
            UdpClient udpClient = new UdpClient();

            while (true)
            {
                for (int i = 0; i < 256; i++)
                {
                    bool isKeyPressed = (GetAsyncKeyState(i) & 0x8000) != 0;

                    if (isKeyPressed != previousKeyState[i])
                    {
                        string message = isKeyPressed ? $"Key Pressed: {i}/{commandId}" : $"Key Released: {-i}/{commandId}";
                        Console.WriteLine(message);
                        SendUdpMessage(udpClient, commandId, isKeyPressed? i:-i);

                        previousKeyState[i] = isKeyPressed;
                    }
                }

                // Check if 'q' is pressed to quit the program
                if (previousKeyState[(int)'q'])
                {
                    Console.WriteLine("\nExiting program...");
                    udpClient.Close();
                    return;
                }
                Thread.Sleep(1);
            }
        }

        
        static void SendUdpMessage(UdpClient client, int intIndex, int intCmd )
        {

            byte[] bytes1 = BitConverter.GetBytes(intIndex);
            byte[] bytes2 = BitConverter.GetBytes(intCmd);

            byte[] data = new byte[bytes1.Length + bytes2.Length];
            Buffer.BlockCopy(bytes1, 0, data, 0, bytes1.Length);
            Buffer.BlockCopy(bytes2, 0, data, bytes1.Length, bytes2.Length);
            client.Send(data, data.Length, new IPEndPoint(BroadcastAddress, Port));

        }
    }
}
