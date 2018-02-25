using System;
using System.Net.Sockets;
using System.IO;

namespace Talky_Client.Connection
{
    public class ServerConnection : IDisposable
    {

        public string Host { get; }
        public int Port { get; }
        public string Username { get; set; }
        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;

        private readonly Config _config;


        private static ServerConnection _currentConnection;
        private static readonly object Lock = new object();

        public ServerConnection(Config config) 
        {
            Host = config.Hostname;
            Port = config.Port;
            _config = config;
            _currentConnection = this;
            Connect();
        }

        public void Connect()
        {
            try
            {
                Disconnect();
                _client = new TcpClient(Host, Port);
                _reader = new StreamReader(_client.GetStream());
                _writer = new StreamWriter(_client.GetStream());

                if (_config.Password.Equals(""))
                {
                    Send("M:/name " + _config.UserName);
                }
                else
                {
                    Send("M:/auth " + _config.UserName + " " + _config.Password);
                }

                Send("S:Client");
                Send("S:Account");
                Send("S:ChannelClientList");
            }
            catch
            {
                _client = null;
            }
        }

        public bool IsConnected()
        {
            lock (Lock)
            {
                return _currentConnection != null && _currentConnection._client.Connected;
            }
        }

        public void Send(string msg)
        {
            _writer.WriteLine(msg);
            _writer.Flush();
        }

        public void Disconnect()
        {
            lock (Lock)
            {
                try
                {
                    _reader?.Dispose();
                    _writer?.Dispose();
                    _client.Close();
                    ((IDisposable)_client)?.Dispose();
                    _currentConnection = null;
                }
                catch (Exception e)
                {
                    //log exceptions somewhere?
                }
            }
        }

        public string Read()
        {
            var msg = string.Empty;

            try
            {
                msg = _reader.ReadLine();
            }
            catch (Exception e)
            {
                //todo::
            }

            return msg;
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
