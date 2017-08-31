using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sword.Server.ConnectionWorker
{
    internal class ConnectionAcceptor
    {
        private ConnectionWorkerReactor master;
        public SocketAsyncEventArgs SocketAsyncEventArgs { get; set; }
        private Socket serverSocket { get; set; }

        public ConnectionAcceptor(ConnectionWorkerReactor master, Socket socket)
        {
            this.master = master;
            this.serverSocket = socket;

            this.SocketAsyncEventArgs = new SocketAsyncEventArgs();
            this.SocketAsyncEventArgs.Completed += SocketAsyncEventArgs_Completed;
        }

        public void Start()
        {
            this.SocketAsyncEventArgs.AcceptSocket = null;

            var delayed=this.serverSocket.AcceptAsync(this.SocketAsyncEventArgs);

            if (!delayed)
            {
                //Console.WriteLine("333333-if");
                ProcessAccept(this.SocketAsyncEventArgs.AcceptSocket);
            }
            else {
                //Console.WriteLine("333333-else");
            }
        }

        private void SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            //Console.WriteLine("ServerAcceptor==>" + e.LastOperation.ToString());

            if (e.LastOperation == SocketAsyncOperation.Accept)
            {
                if (e.SocketError != SocketError.Success)
                {
                    //Console.WriteLine("Error in ConnectionAcceptor!");
                    return;
                }

                //Console.WriteLine("New client[{0}] connected.", e.AcceptSocket.RemoteEndPoint.ToString());

                ProcessAccept(e.AcceptSocket);
                //Console.WriteLine("111");
            }

            Start();
            //Console.WriteLine("222");
        }

        private void ProcessAccept(Socket acceptSocket)
        {
            var worker = new ConnectionWorker(acceptSocket);
            worker.LastActiveTime = DateTime.Now;

            this.master.AddWorker(worker);

            worker.StartReceive();
        }
    }
}
