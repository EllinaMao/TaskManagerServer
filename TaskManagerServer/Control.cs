using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskManagerServer
{
    // Основной класс сервера, который управляет подключениями и обменом данными с клиентом
    public class ControlAsync
    {
        private int _port;                    // Порт, на котором будет работать сервер
        private Socket? _listener;            // Сокет, который принимает подключения от клиентов
        private bool _running;                // Флаг состояния сервера — работает ли он сейчас

        // События, чтобы сообщать о разных ситуациях (лог, подключение, отключение, получение данных)
        public event Action<string>? LogMessage;                    // Для логов
        public event Action<string>? ClientConnected;                // Когда клиент подключился
        public event Action<string>? ClientDisconnected;             // Когда клиент отключился
        public event Action<List<ProcessInfo>>? ProcessesReceived;   // Когда пришёл список процессов от клиента

        private Socket? _clientSocket;       // Сокет, с которым ведётся общение после подключения
        private SynchronizationContext? _uiContext = null; // Контекст синхронизации для UI (чтобы вызывать события из правильного потока, напр. WinForms)

        // Конструктор. Задаёт порт и контекст UI
        public ControlAsync(int port = 49200, SynchronizationContext? uiContext = null)
        {
            _port = port;
            _uiContext = uiContext ?? new SynchronizationContext();
        }

        // Метод логирования — безопасно вызывает LogMessage в UI-потоке
        private void Log(string msg)
        {
            if (_uiContext != null)
                _uiContext.Post(d => LogMessage?.Invoke(msg), null);
            else
                LogMessage?.Invoke(msg);
        }

        // Асинхронный запуск сервера
        public async Task StartAsync()
        {
            // Создаём TCP-сокет
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Создаём конечную точку, принимающую подключения на всех IP по указанному порту
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 49200);
            _listener.Bind(ipEndPoint);   // Привязываем сокет к адресу
            _listener.Listen(100);        // Начинаем слушать
            _running = true;

            Log($"Server started on port {_port}");

            while (_running)
            {
                try
                {
                    // Асинхронно ждём подключение
                    var clientSocket = await _listener.AcceptAsync();
                    _clientSocket = clientSocket;

                    // Получаем адрес клиента
                    string? ep = clientSocket?.RemoteEndPoint?.ToString();
                    ClientConnected?.Invoke(ep);

                    // Обрабатываем подключение клиента в отдельной задаче
                    _ = HandleClientAsync(_clientSocket, ep);
                }
                catch (Exception ex)
                {
                    Log("Accept error: " + ex.Message);
                }
            }
        }

        // Метод отправки команды клиенту
        public async Task SendCommandAsync(int commandCode, object? data = null)
        {
            if (_clientSocket == null) return; // Нет подключения — выходим

            // Формируем сообщение в виде объекта
            var message = new
            {
                Command = commandCode, // код команды (например: 1 - показать процессы, 2 - убить процесс и т.д.)
                Data = data            // дополнительные данные (например, имя процесса)
            };

            // Сериализуем в JSON
            string json = JsonSerializer.Serialize(message);
            // Преобразуем JSON в байты
            byte[] msg = Encoding.UTF8.GetBytes(json);

            // Отправляем данные клиенту
            await _clientSocket.SendAsync(msg, SocketFlags.None);
        }


        // Метод для десериализации JSON-строки в список процессов
        public List<ProcessInfo> ParseProcessList(string json)
        {
            try
            {
                Log($"Parse process list: {json}");
                var processes = JsonSerializer.Deserialize<List<ProcessInfo>>(json);
                return processes ?? new List<ProcessInfo>();
            }
            catch (Exception ex)
            {
                Log($"Ошибка десериализации JSON: {ex.Message}");
                return new List<ProcessInfo>();
            }
        }

        // Остановка сервера и закрытие сокетов
        public void Stop()
        {
            _running = false;
            try
            {
                _listener?.Close();       // Закрываем серверный сокет
                _clientSocket?.Close();   // Закрываем клиентский сокет
            }
            catch (Exception ex)
            {
                Log("Close error: " + ex.Message);
            }
        }

        // Асинхронная обработка клиента
        private async Task HandleClientAsync(Socket client, string ep)
        {
            // Буфер для накопления данных. Выносим из цикла!
            StringBuilder messageBuilder = new StringBuilder();
            byte[] buffer = new byte[1024]; // Буфер для чтения из сокета

            try
            {
                while (true)
                {
                    // 1. Асинхронно получаем данные
                    int bytesRec = await client.ReceiveAsync(buffer);

                    // 2. ВАЖНО: Проверяем на разрыв соединения
                    if (bytesRec == 0)
                    {
                        Log($"Client {ep} disconnected.");
                        break; // Выходим из цикла
                    }

                    // 3. Добавляем полученные данные в наш накопитель
                    messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRec));

                    // 4. Проверяем, есть ли у нас хотя бы одно *полное* сообщение
                    // (Мы используем \n как разделитель сообщений)
                    string allData = messageBuilder.ToString();
                    int messageEndIndex;

                    // 5. Обрабатываем ВСЕ полные сообщения, которые могли прийти
                    while ((messageEndIndex = allData.IndexOf('\n')) != -1)
                    {
                        // Вырезаем полное сообщение (до \n)
                        string fullJsonMessage = allData.Substring(0, messageEndIndex);

                        // Удаляем это сообщение (и \n) из накопителя
                        allData = allData.Substring(messageEndIndex + 1);

                        // Теперь у нас есть полный JSON, можно парсить
                        if (!string.IsNullOrEmpty(fullJsonMessage))
                        {
                            var processes = ParseProcessList(fullJsonMessage);

                            if (processes.Count > 0)
                            {
                                Log($"Получено {processes.Count} процессов от {ep}");
                                ProcessesReceived?.Invoke(processes);
                            }
                            else
                            {
                                Log($"Получены данные (не список) от {ep}: {fullJsonMessage}");
                            }
                        }
                    }

                    // 6. Обновляем накопитель, оставляя в нем "остаток" (начало следующего сообщения)
                    messageBuilder = new StringBuilder(allData);
                }
            }
            catch (SocketException ex)
            {
                Log($"Client {ep} socket error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log($"Client {ep} error: {ex.Message}");
            }
            finally
            {
                client.Close(); // Не забываем закрывать сокет
            }
        }
        // Форматирует список процессов в читаемые строки
        private List<string> FormatProcesses(List<ProcessInfo> processes)
        {
            var lines = new List<string>();
            foreach (var p in processes)
            {
                lines.Add($"Id={p.Id}, Name={p.Name}");
            }
            return lines;
        }

        // Логирует список процессов в окно/консоль
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
