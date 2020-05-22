using System;
using System.Windows.Forms;
using SomeProject.Library.Client;
using SomeProject.Library;

namespace SomeProject.TcpClient
{
    public partial class ClientMainWindow : Form
    {
        public ClientMainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик кнопки для отправки сообщения на сервер
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMsgBtnClick(object sender, EventArgs e)
        {
            Client client = new Client();
            Result res = client.SendMessageToServer(textBox.Text).Result;
            
            serverOutput.Text += "\n" + client.ReceiveMessageFromServer().Message;
            
            if(res == Result.OK)
            {
                textBox.Text = "";
                labelRes.Text = "Message was sent succefully!";
            }
            else
            {
                labelRes.Text = "Cannot send the message to the server.";
            }
            timer.Interval = 2000;
            timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            labelRes.Text = "";
            timer.Stop();
        }

        /// <summary>
        /// Обработчик кнопки для отправки фалйа на сервер
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendFileBtn_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            var dialogResult = openFileDialog.ShowDialog();
            if (dialogResult == DialogResult.Yes || dialogResult == DialogResult.OK)
            {
                var client = new Client();
                var res = client.SendFileToServer(openFileDialog.FileName).Result;
            
                serverOutput.Text += "\n" + client.ReceiveMessageFromServer().Message;
            
                if(res == Result.OK)
                {
                    textBox.Text = "";
                    labelRes.Text = "File was sent succefully!";
                }
                else
                {
                    labelRes.Text = "Cannot send the file to the server.";
                }
                timer.Interval = 2000;
                timer.Start();
            }
        }

        /// <summary>
        /// Метод для дебага сервера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void twoClientsBtn_Click(object sender, EventArgs e)
        {
            Client client1 = new Client();
            Client client2 = new Client();
            Result res1 = client1.SendMessageToServer(textBox.Text).Result;
            Result res2 = client2.SendMessageToServer(textBox.Text).Result;
            
            serverOutput.Text += "\n" + client1.ReceiveMessageFromServer().Message;
            serverOutput.Text += "\n" + client2.ReceiveMessageFromServer().Message;

        }
    }
}
