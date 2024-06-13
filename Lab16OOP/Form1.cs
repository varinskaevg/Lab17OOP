using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab16OOP
{
    public partial class Form1 : Form
    {
        bool alive = false; // чи буде працювати потік для приймання
        TcpClient client;
        NetworkStream stream;
        const int port = 8888; // Порт для сервера та клієнта
        string userName; // ім’я користувача в чаті
        public Font ChatFont { get; set; }


        public Form1()
        {
            InitializeComponent();
            loginButton.Enabled = true; // кнопка входу
            logoutButton.Enabled = false; // кнопка виходу
            sendButton.Enabled = false; // кнопка отправки
            chatTextBox.ReadOnly = true; // поле для повідомлень

            lblHost.Text = $"Host: 127.0.0.1";
            lblPort.Text = $"Port: {port}";
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            userName = userNameTextBox.Text;
            userNameTextBox.ReadOnly = true;
            try
            {
                client = new TcpClient();
                client.Connect("127.0.0.1", port);
                stream = client.GetStream();

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // Відображення повідомлення про вхід користувача в chatTextBox
                string loginMessage = $"Користувач {userName} увійшов до чату";
                this.Invoke(new MethodInvoker(() =>
                {
                    chatTextBox.AppendText(loginMessage + "\r\n");
                    SaveLog(loginMessage);
                }));

                Task receiveTask = new Task(ReceiveMessages);
                receiveTask.Start();

                loginButton.Enabled = false;
                logoutButton.Enabled = true;
                sendButton.Enabled = true;

                // Показати шлях до файлу логу
                ShowLogFilePath();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ReceiveMessages()
        {
            alive = true;
            try
            {
                while (alive)
                {
                    if (client != null && client.Connected)
                    {
                        byte[] data = new byte[64];
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        do
                        {
                            bytes = stream.Read(data, 0, data.Length);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (stream.DataAvailable);

                        string message = builder.ToString();
                        this.Invoke(new MethodInvoker(() =>
                        {
                            string time = DateTime.Now.ToShortTimeString();
                            string logMessage = $"{time} {message}";
                            chatTextBox.AppendText(logMessage + "\r\n");
                            SaveLog(logMessage);
                        }));
                    }
                    else
                    {
                        MessageBox.Show("З'єднання з сервером було втрачено.");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (client != null && client.Connected)
                {
                    string message = $"{userName}: {messageTextBox.Text}";
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                    SaveLog(message);

                    // Додати відправлене повідомлення в chatTextBox
                    string sentMessage = $"{DateTime.Now.ToShortTimeString()} {message}";
                    this.Invoke(new MethodInvoker(() =>
                    {
                        chatTextBox.AppendText(sentMessage + "\r\n");
                        SaveLog(sentMessage);
                    }));

                    messageTextBox.Clear();
                }
                else
                {
                    MessageBox.Show("З'єднання з сервером було втрачено.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void logoutButton_Click(object sender, EventArgs e)
        {
            ExitChat();
        }

        // вихід з чату
        private void ExitChat()
        {
            try
            {
                if (client != null && client.Connected)
                {
                    string message = $"{userName} покидає чат";
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
                alive = false;
                client.Close(); // Закриття з'єднання
                client = null; // Скидання посилання на клієнта
                loginButton.Enabled = true;
                logoutButton.Enabled = false;
                sendButton.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (alive)
                ExitChat();
        }

        private void btnFont_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                ChatFont = fontDialog1.Font;
                chatTextBox.Font = ChatFont;
                messageTextBox.Font = ChatFont;
            }
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.Host = "127.0.0.1";
            settingsForm.Port = port;

            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                // налаштування сервера тут
                lblHost.Text = $"Host: {settingsForm.Host}";
                lblPort.Text = $"Port: {settingsForm.Port}";
            }
        }

        private void SaveLog(string message)
        {
            string logFilePath = Path.Combine(Application.StartupPath, "chatLog.txt");
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now.ToShortTimeString()} {message}");
            }
        }

        private void ShowLogFilePath()
        {
            string logFilePath = Path.Combine(Application.StartupPath, "chatLog.txt");
            MessageBox.Show($"Журнал чату зберігається в: {logFilePath}", "Шлях до файлу журналу чату", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
