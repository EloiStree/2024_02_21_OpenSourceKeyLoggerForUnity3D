using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace KeyboardInputConsoleApp
{
    class Program
    {
        
        static int Port = 7072;
        static IPAddress BroadcastAddress = IPAddress.Parse("127.0.0.1"); 
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        static bool[] previousKeyState = new bool[256];

        public class JsonConfig
        {
            public int m_offset { get; set; } = 160000;
            public int m_offset_press { get; set; } = 1000;
            public int m_offset_release { get; set; } = 2000;
            public int m_port { get; set; } = 7072;
            public string m_ip_target { get; set; } = "127.0.0.1";
        }

        static void Main(string[] args)
        {
            string filePath = "Config.json";
            JsonConfig config= new JsonConfig();

            if (!File.Exists(filePath))
            {
                // Read the JSON content from the file
                string outputJsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, outputJsonString);
            }
            if (File.Exists(filePath))
            {
                // Read the JSON content from the file
                string jsonString = File.ReadAllText(filePath);
                config = JsonSerializer.Deserialize<JsonConfig>(jsonString);
                Console.WriteLine(jsonString);

            }


            Port = config.m_port;
            BroadcastAddress = IPAddress.Parse(config.m_ip_target);

            int offset = config.m_offset;
            Console.WriteLine("Offset: " + offset);

            // Create UDP client
            UdpClient udpClient = new UdpClient();

            while (true)
            {
                for (int i = 0; i < 256; i++)
                {
                    bool isKeyPressed = (GetAsyncKeyState(i) & 0x8000) != 0;

                    if (isKeyPressed != previousKeyState[i])
                    {
                        int temp = isKeyPressed ? offset + config.m_offset_press + i : offset + config.m_offset_release + i;
                        string message = $"Key Pressed: {temp}" ;
                        Console.WriteLine(message);
                        SendUdpMessage(udpClient, temp);

                        previousKeyState[i] = isKeyPressed;
                    }
                }

                Thread.Sleep(TimeSpan.FromMicroseconds(10));
            }
        }


        static void SendUdpMessage(UdpClient client, int intValue)
        {

            byte[] bytes1 = BitConverter.GetBytes(intValue);
           
            client.Send(bytes1, bytes1.Length, new IPEndPoint(BroadcastAddress, Port));

        }
    }
}
