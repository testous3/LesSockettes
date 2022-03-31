using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client;

namespace WindowsFormsSample
{
    public partial class ChatForm : Form
    {
        private HubConnection _connection;

        public ChatForm()
        {
            InitializeComponent();
        }

        private void ChatForm_Load(object sender, EventArgs e)
        {
            addressTextBox.Focus();
        }

        private void addressTextBox_Enter(object sender, EventArgs e)
        {
            AcceptButton = connectButton;
        }

        private async void connectButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(usernameTextBox.Text))
            {
                Log(Color.Red, "Username Required.");
                usernameTextBox.Focus();
            }
            else
            {
                UpdateState(connected: false);

                Log(Color.Gray, "Starting connection...");
                try
                {
                    _connection = new HubConnectionBuilder()
                    .WithUrl(addressTextBox.Text + "?username=" + usernameTextBox.Text)
                   
                    .Build();

                    _connection.On<string, string>("SystemMessage", (s1, s2) => OnSend(s1, s2));
                    _connection.On<string, string>("ReceiveMessage", (s1, s2) => OnReceive(s1, s2));
                    _connection.On<string>("SystemInfo", (s1) => OnReceiveInfo(s1));

                    await _connection.StartAsync();
                }
                catch (Exception ex)
                {
                    Log(Color.Red, ex.ToString());
                    return;
                }

                Log(Color.Gray, "Connection established.");

                UpdateState(connected: true);

                messageTextBox.Focus();
            }
        }

        private async void disconnectButton_Click(object sender, EventArgs e)
        {
            Log(Color.Gray, "Stopping connection...");
            try
            {
                await _connection.StopAsync();
            }
            catch (Exception ex)
            {
                Log(Color.Red, ex.ToString());
            }

            Log(Color.Gray, "Connection terminated.");

            NbConnexionLabel.Text = string.Empty;
            UpdateState(connected: false);
        }

        private void messageTextBox_Enter(object sender, EventArgs e)
        {
            AcceptButton = sendButton;
        }

        private async void sendButton_Click(object sender, EventArgs e)
        {
            try
            {
                await _connection.InvokeAsync("SendMessage", usernameTextBox.Text, messageTextBox.Text);
                messageTextBox.Text = string.Empty;
                messageTextBox.Focus();
            }
            catch (Exception ex)
            {
                Log(Color.Red, ex.ToString());
            }
        }

        private void UpdateState(bool connected)
        {
            disconnectButton.Enabled = connected;
            connectButton.Enabled = !connected;
            addressTextBox.Enabled = !connected;
            usernameTextBox.Enabled = !connected;

            messageTextBox.Enabled = connected;
            sendButton.Enabled = connected;
        }

        private void OnSend(string name, string message)
        {
            Log(Color.Gray, name + ": " + message);
        }

        private void OnReceive(string name, string message)
        {

            Log(Color.Black, name + ": " + message);
        }

        private void OnReceiveInfo(string message)
        {

            NbConnexionLabel.Text = message;
        }

        private void Log(Color color, string message)
        {
            Action callback = () =>
            {
                messagesList.Items.Add(new LogMessage(color, message));
            };

            Invoke(callback);
        }

        private class LogMessage
        {
            public Color MessageColor { get; }

            public string Content { get; }

            public LogMessage(Color messageColor, string content)
            {
                MessageColor = messageColor;
                Content = content;
            }
        }

        private void messagesList_DrawItem(object sender, DrawItemEventArgs e)
        {
            var message = (LogMessage)messagesList.Items[e.Index];
            e.Graphics.DrawString(
                message.Content,
                messagesList.Font,
                new SolidBrush(message.MessageColor),
                e.Bounds);
        }
    }
}
