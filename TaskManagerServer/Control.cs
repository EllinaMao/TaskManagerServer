using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskManagerServer
{
    public class ControlAsync
    {
        private int _port;
        private Socket? _listener;
        private bool _running;


        public event Action<string>? LogMessage;
        public event Action<string>? ClientConnected;
        public event Action<string>? ClientDisconnected;
        public event Action<List<ProcessInfo>>? ProcessesReceived;

        private Socket? _clientSocket;

        private SynchronizationContext _uiContext = null;//winforms

        public ControlAsync(int port = 49200, SynchronizationContext uiContext = null)
        {
            _port = port;
            _uiContext = uiContext ?? new SynchronizationContext();

        }
        private void Log(string msg)
        {
            if (_uiContext != null)
                _uiContext.Post(d => LogMessage?.Invoke(msg), null);
            else
                LogMessage?.Invoke(msg);
        }

        public async Task StartAsync()
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 49200);
            _listener.Bind(ipEndPoint);
            _listener.Listen(100);
            _running = true;

            Log($"Server started on port {_port}");

            while (_running)
            {
                try
                {
                    var clientSocket = await _listener.AcceptAsync();
                    _clientSocket = clientSocket;
                    string? ep = clientSocket?.RemoteEndPoint?.ToString();
                    ClientConnected?.Invoke(ep);
                    _ = HandleClientAsync(_clientSocket, ep);
                }
                catch (Exception ex)
                {
                    Log("Accept error: " + ex.Message);
                }
            }
        }
        public async Task SendCommandAsync(int commandCode, object? data = null)
        {
            if (_clientSocket == null) return;

            var message = new
            {
                Command = commandCode,
                Data = data
            };
            string json = JsonSerializer.Serialize(message);
            byte[] msg = Encoding.UTF8.GetBytes(json);
            await _clientSocket.SendAsync(msg, SocketFlags.None);
        }
        private List<ProcessInfo> ParseProcessList(string json)
        {
            try
            {
                var processes = JsonSerializer.Deserialize<List<ProcessInfo>>(json);
                return processes ?? new List<ProcessInfo>();
            }
            catch (Exception ex)
            {
                Log($"Ошибка десериализации JSON: {ex.Message}");
                return new List<ProcessInfo>();
            }
        }
        public void Stop()
        {
            _running = false;
            try
            {
                _listener?.Close();
                _clientSocket?.Close();

            }
            catch (Exception ex)
            {
                Log("Close error: " + ex.Message);
            }
        }
        private async Task HandleClientAsync(Socket client, string ep)
        {
            try
            {
                byte[] bytes = new byte[1024];
                StringBuilder sb = new StringBuilder();

                while (_running)
                {
                    int bytesRec = await client.ReceiveAsync(bytes, SocketFlags.None);
                    if (bytesRec == 0)
                    {
                        break;
                    }
                    sb.Append(Encoding.UTF8.GetString(bytes, 0, bytesRec));
                }
                string json = sb.ToString();
                var processes = ParseProcessList(json);

                Log($"Получено {processes.Count} процессов от {ep}");
                DisplayProcesses(processes, ep);
                ProcessesReceived?.Invoke(processes);


            }
            catch (Exception ex)
            {
                Log($"Client {ep} error {ex.Message}");
            }
            finally
            {
                try
                {
                    client.Close();
                    ClientDisconnected?.Invoke(ep);
                }
                catch (Exception ex)
                {
                    {
                        ClientDisconnected?.Invoke(ep);
                        _clientSocket = null;

                    }
                }

            }
        }

        private List<string> FormatProcesses(List<ProcessInfo> processes)
        {
            var lines = new List<string>();
            foreach (var p in processes)
            {
                lines.Add($"Id={p.Id}, Name={p.Name}");
            }
            return lines;
        }

        private void DisplayProcesses(List<ProcessInfo> processes, string ep)
        {
            Log($"Получено {processes.Count} процессов от {ep}");
            foreach (var p in processes)
            {
                Log($"Id={p.Id}, Name={p.Name}");
            }
        }
    }
}