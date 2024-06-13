using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Lab16OOP
{
    public class ServerObject
    {
        static TcpListener tcpListener;
        List<ClientObject> clients = new List<ClientObject>();

        // Додавання клієнта до списку підключених
        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }

        // Видалення клієнта зі списку підключених
        protected internal void RemoveConnection(string id)
        {
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
            {
                clients.Remove(client);
                client.Close(); // Закриття з'єднання клієнта
            }
        }

        // Прослуховування нових з'єднань
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        // Розсилка повідомлення всім клієнтам, крім відправника
        protected internal void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            foreach (ClientObject client in clients)
            {
                if (client.Id != id)
                {
                    client.Stream.Write(data, 0, data.Length);
                }
            }
        }

        // Відключення сервера та всіх клієнтів
        protected internal void Disconnect()
        {
            tcpListener.Stop();
            foreach (ClientObject client in clients)
            {
                client.Close(); // Закриття всіх з'єднань клієнтів
            }
            Environment.Exit(0);
        }
    }
}
