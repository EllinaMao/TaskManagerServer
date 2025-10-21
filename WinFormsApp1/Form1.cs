using TaskManagerServer;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private readonly SynchronizationContext? _uiContext;
        TaskManagerServer.ControlAsync controls;


        public Form1()
        {
            _uiContext = SynchronizationContext.Current;
            InitializeComponent();
            controls = new TaskManagerServer.ControlAsync(49200, _uiContext);
            controls.LogMessage += Controls_LogMessage;
            controls.ClientConnected += Controls_ClientConnected;
            controls.ClientDisconnected += Controls_ClientDisconnected;
            controls.ProcessesReceived += Controls_ProcessesReceived;
        }

        private async void StartProgram_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            await controls.StartAsync();
        }

        private async void Refresh_Click(object sender, EventArgs e)
        {
            await controls.SendCommandAsync(ProcessCodes.GetProcesses); // команда обновить список процессов
        }

        private async void KillProcess_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem == null) return;

            var parts = listBox2.SelectedItem.ToString()?.Split('—');
            if (parts?.Length > 0 && int.TryParse(parts[0].Trim(), out int id))
            {
                await controls.SendCommandAsync(ProcessCodes.KillProcess, id);
            }
        }

        private async void StartNewProcess_Click(object sender, EventArgs e)
        {
            string path = textBoxProcessName.Text;
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("You need to enter what process to start");
            }
            await controls.SendCommandAsync(ProcessCodes.CreateProcess, path);

        }

        private void Controls_LogMessage(string message)
        {
            _uiContext?.Post(_ => listBox2.Items.Add(message), null);
        }

        private void Controls_ClientConnected(string ep)
        {
            _uiContext?.Post(_ => listBox1.Items.Add($"Клиент подключился: {ep}"), null);
        }

        private void Controls_ClientDisconnected(string ep)
        {
            _uiContext?.Post(_ => listBox1.Items.Add($"Клиент отключился: {ep}"), null);
        }

        private void Controls_ProcessesReceived(List<ProcessInfo> processes)
        {
            _uiContext?.Post(_ =>
            {
                listBox2.Items.Clear();
                foreach (var p in processes)
                {
                    listBox2.Items.Add($"{p.Id} — {p.Name}");
                }
            }, null);
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            controls.Stop();
        }
    }
}
