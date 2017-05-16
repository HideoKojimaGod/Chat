using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class Client : Form
    {
        Thread receiveThread;
        TextBox userNameTextBox;
        TextBox chatTextBox;
        TextBox messageTextBox;
        Button loginButton;
        Button logoutButton;
        Button sendButton;
        static string userName;
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;

        public Client()
        {
            Label userNameLabel = new Label()
            {
                Text = "Введите имя",
                Location = new Point(5, 5)
            };

            userNameTextBox = new TextBox()
            {
                Location = new Point(5, userNameLabel.Bottom + 5)
            };

            loginButton = new Button()
            {
                Location = new Point(userNameTextBox.Right + 5, 5),
                Text = "login"
            };

            logoutButton = new Button()
            {
                Location = new Point(userNameTextBox.Right + 5, userNameLabel.Bottom + 5),
                Text = "logout"
            };

            chatTextBox = new TextBox()
            {
                Location = new Point(5, userNameTextBox.Bottom + 5),
                Multiline = true,
                ReadOnly = true,
                Height = 170,
                Width = 200,
                ScrollBars = ScrollBars.Vertical
        };
            messageTextBox = new TextBox()
            {
                Location = new Point(5, chatTextBox.Bottom + 5)
            };
            sendButton = new Button()
            {
                Location = new Point(messageTextBox.Right + 5, chatTextBox.Bottom + 5),
                Text = "send"
            };

            Controls.Add(userNameTextBox);
            Controls.Add(chatTextBox);
            Controls.Add(messageTextBox);
            Controls.Add(userNameLabel);
            Controls.Add(loginButton);
            Controls.Add(logoutButton);
            Controls.Add(sendButton);
            logoutButton.Click += (sender, args) => Disconnect();
            loginButton.Click += (sender, args) => Connect();
            sendButton.Click += (sender, args) => SendMessage();
            FormClosing += (sender, args) => Disconnect();
            InitializeComponent();
        }


        void Connect()
        {
            client = new TcpClient();
            userName = userNameTextBox.Text;
            chatTextBox.Text = "Добро пожаловать, " + userName;
            client.Connect(host, port); 
            stream = client.GetStream(); 

            string message = userName;
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
            receiveThread = new Thread(new ThreadStart(ReceiveMessage));
            receiveThread.Start(); 
        }
        void SendMessage()
        {
            string message = messageTextBox.Text;
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
            if (chatTextBox.TextLength == 0) chatTextBox.Text += String.Format("{0}: {1}", userName, message);
            else chatTextBox.Text += "\r\n" + String.Format("{0}: {1}", userName, message);
            messageTextBox.Clear();
        }
        void ReceiveMessage()
        {
            while (true)
            {
                
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    if (chatTextBox.TextLength == 0) chatTextBox.Text += message;
                    else chatTextBox.Text += "\r\n" + message;
                }
                catch
                {
                    Disconnect();
                 }
        }
}

        void Disconnect()
        {
            if (stream != null)
               stream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0);
        }

    }
}
