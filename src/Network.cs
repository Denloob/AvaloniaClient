using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Numerics;
using System.IO;
using Newtonsoft.Json;
using Tmds.DBus.Protocol;

namespace Network
{
    public enum RequestCode
    {
        EXIT,

        LOGIN,
        LOGOUT,
        SIGNUP,

        ROOM_CREATE,
        ROOM_CLOSE,
        ROOM_JOIN,
        ROOM_LEAVE,

        GET_ROOMS,
        GET_PLAYERS_IN_ROOM,
        GET_PERSONAL_STATISTICS,
        GET_HIGH_SCORE,
        GET_ROOM_STATE,
        GET_GAME_RESULT,

        GAME_START,
        GAME_LEAVE,

        QUESTION_ADD,
        QUESTION_GET,

        SUBMIT_ANSWER,
    }

    public class Communicator
    {
        private const string defaultIP = "127.0.0.1";

        const bool useOTP = false;

        public TcpClient? client;

        public string OTPkey;

        // Singleton pattern
        private static readonly Lazy<Communicator> lazy = new(() => new Communicator());
        public static Communicator Instance => lazy.Value;

        private Communicator()
        {
        }

        public void Connect(int port, string? ip = null)
        {
            this.client = new TcpClient();
            this.client.Connect(ip ?? defaultIP, port);
            if (useOTP)
            {
                string jsonOTPKey = Receive();
                var jsonOTPValue = JsonConvert.DeserializeAnonymousType(jsonOTPKey, new { SharedOTPKey = "" });
                OTPkey = jsonOTPValue.SharedOTPKey;
                Log("The key is:");
                Log(OTPkey);
            }
        }

        public void Disconnect()
        {
            if (this.client == null)
                throw new Exception("Client not connected");
            this.client.Close();
            this.client = null;
            Log("Disconnected from server.");
        }

        public string Request(RequestCode code, object request)
        {
            if (this.client == null)
                throw new Exception("Client not connected");
            string json = JsonConvert.SerializeObject(request);
            if (useOTP)
            {
                json = EncodeToBase64(Encrypt(json , OTPkey));
            }
            byte[] data = FormatMessage(code, json);
            NetworkStream stream = this.client.GetStream();
            stream.Write(data, 0, data.Length);
            return Receive();
        }

        /**
         * @brief Convert some data into a byte protocol buffer in the form of
         *  +---------------+--------------------+------+
         *  | code [1 byte] | data_len [4 bytes] | data |
         *  +---------------+--------------------+------+
         */
        private static byte[] FormatMessage(RequestCode code, string data)
        {
            byte[] codeBytes = { (byte)code };
            byte[] dataLengthBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            byte[] message = new byte[codeBytes.Length + dataLengthBytes.Length + dataBytes.Length];
            Buffer.BlockCopy(codeBytes, 0, message, 0, codeBytes.Length);
            Buffer.BlockCopy(dataLengthBytes, 0, message, codeBytes.Length, dataLengthBytes.Length);
            Buffer.BlockCopy(dataBytes, 0, message, codeBytes.Length + dataLengthBytes.Length, dataBytes.Length);

            return message;
        }

        private string Receive()
        {
            if (this.client == null)
                throw new Exception("Client not connected");
            NetworkStream stream = this.client.GetStream();

            byte[] codeBytes = new byte[1];
            stream.Read(codeBytes, 0, 1);
            bool success = codeBytes[0] == 1;

            byte[] dataLengthBytes = new byte[4];
            stream.Read(dataLengthBytes, 0, 4);
            int dataLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(dataLengthBytes, 0));

            byte[] data = new byte[dataLength];
            stream.Read(data, 0, dataLength);

            string strData = Encoding.UTF8.GetString(data);
            if (!success)
            {
                var message = JsonConvert.DeserializeAnonymousType(strData, new { message = "" })?.message;
                message ??= "Something went wrong";

                throw new ArgumentException(message);
            }

            return strData;
        }

        public string Encrypt(string message, string sharedOTPKey)
        {
            Log(message);
            StringBuilder cypher = new StringBuilder(message.Length);
            for (int i = 0; i < message.Length; ++i)
            {
                cypher.Append((char)(message[i] ^ sharedOTPKey[i]));
            }
            return cypher.ToString();
        }

        public string Decrypt(string cypher, string sharedOTPKey)
        {
            StringBuilder message = new StringBuilder(cypher.Length);

            for (int i = 0; i < cypher.Length; ++i)
            {
                message.Append((char)(cypher[i] ^ sharedOTPKey[i]));
            }

            return message.ToString();
        }
        public string EncodeToBase64(string data)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(data);
            Log(Convert.ToBase64String(plainTextBytes));
            return Convert.ToBase64String(plainTextBytes);
        }

        public string DecodeFromBase64(string base64Data)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64Data);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        private static void Log(string message)
        {
            using (StreamWriter writer = new StreamWriter("logs.txt", true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}
