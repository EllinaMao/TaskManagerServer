namespace TaskManagerServer
{
    internal class Program
    {
        static async Task Main()
        {
            var server = new ControlAsync();

            server.LogMessage += Console.WriteLine;
            server.ClientConnected += ep => Console.WriteLine($"Клиент подключился: {ep}");
            server.ClientDisconnected += ep => Console.WriteLine($"Клиент отключился: {ep}");

            Console.WriteLine("Запуск сервера...");
            _ = server.StartAsync(); // запускаем сервер

            Console.WriteLine("Нажмите Enter, чтобы отправить команду GetProcesses клиенту...");
            Console.ReadLine();

            await server.SendCommandAsync(TaskManagerServer.ProcessCodes.GetProcesses);
            Console.ReadLine();

            server.ProcessesReceived += processes =>
            {
                Console.WriteLine("Список процессов, присланных клиентом:");
                foreach (var p in processes)
                {
                    Console.WriteLine($"Id={p.Id}, Name={p.Name}");
                }
            };
            Console.WriteLine("Нажмите Enter для остановки сервера...");
            Console.ReadLine();
            server.Stop();

        }
    }
}
