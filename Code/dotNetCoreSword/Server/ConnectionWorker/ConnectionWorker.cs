using Sword.CommandBus;
using Sword.Server.Parsers;
using Sword.Utils;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Sword.Server.ConnectionWorker
{
    public class ConnectionWorker:IDisposable
    {
        public ConnectionWorker(Socket socket)
        {
            this.Socket = socket;

            this.parser = new CommandParser<Command>();

            this.saea_receive = new SocketAsyncEventArgs();

            this.receiveBuffer = new byte[1024*1024];

            this.saea_receive.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);

            this.saea_receive.Completed += SocketAsyncEventArgs_Completed;
        }

        private byte[] receiveBuffer;
        public Socket Socket { get; set; }
        public SocketAsyncEventArgs saea_receive { get; set; }
        public DateTime LastActiveTime { get; set; }
        private CommandParser<Command> parser;
        public bool IsTagged { get; set; }

        public void StartReceive()
        {
            var delayed=this.Socket.ReceiveAsync(this.saea_receive);

            if (!delayed)
            {
                //Console.WriteLine("88888888888888888888888--->");
                //Console.WriteLine("88888888888888888888888--->"+ this.saea_receive.BytesTransferred);
                if (this.saea_receive.BytesTransferred>0)
                    ProcessReceive(this.saea_receive.Buffer, this.saea_receive.BytesTransferred);

                StartReceive();
            }
            //Console.WriteLine("StartReceive");
        }

        private void SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            //Console.WriteLine("88888888888888888888888new event--->" + e.LastOperation.ToString());
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                if (e.SocketError != SocketError.Success)
                {
                    //Console.WriteLine("Error in ConnectionWorker!");
                    return;
                }

                if (e.BytesTransferred == 0)
                {
                    //Console.WriteLine("Client disconnected. " + this.Socket.RemoteEndPoint.ToString());
                    return;
                }

                this.LastActiveTime = DateTime.Now;
                ProcessReceive(e.Buffer, e.BytesTransferred);

                if (!this.IsTagged)
                    StartReceive();
            }
            else
            {
                //Console.WriteLine("SocketAsyncEventArgs_Completed ................");
            }
        }

        private void ProcessReceive(byte[] buffer, int length)
        {
            parser.ProcessReceive(buffer, length);

            //Console.WriteLine(Thread.CurrentThread.ManagedThreadId + "--GetDTOs 111111");
            var cmds = parser.GetDTOs();
            //Console.WriteLine(Thread.CurrentThread.ManagedThreadId + "--GetDTOs 22222");

            if (cmds != null && cmds.Count > 0)
            {
                cmds.ForEach(cmd =>
                {
                    cmd.ConnectionWorker = this;
                    ServerRuntime.AddCommandToIncomingQueueRepository(cmd);
                });
            }
        }

        internal void SendResponse(CommandResult result)
        {
            MemoryStream ms = new MemoryStream();
            BufferedStream bs = new BufferedStream(ms);
            BinaryWriter bw = new BinaryWriter(bs, UnicodeEncoding.UTF8);

            var cmdBytes = SerializerUtility.Instance().BinSerialize(result);
            bw.Write(cmdBytes.Length);
            bw.Write(cmdBytes);

            bw.Flush();

            if (!this.IsTagged)
            {
                try
                {
                    this.Socket.Send(ms.ToArray());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Socket.Send失败");
                }
            }
        }

        public void Dispose()
        {
            if(this.Socket!=null)
            {
                TryCatchHelper.Do(() => { this.Socket.Shutdown(SocketShutdown.Both);});

                TryCatchHelper.Do(() => { this.Socket.Close();});

                TryCatchHelper.Do(() => { this.Socket.Dispose(); });

                this.Socket = null;
            }

            this.receiveBuffer = null;

            this.saea_receive = null;

            parser = null;

            Console.WriteLine("ConnectionWorker disposed");
        }
    }
}
