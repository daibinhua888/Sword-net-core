using Sword.CommandBus;
using Sword.Server.Cleaner;
using Sword.Server.ConnectionWorker;
using Sword.Server.MessageReactors;
using Sword.Server.Pipes;
using Sword.Server.PipeSelectors;
using Sword.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sword.Server
{
    public static class ServerRuntime
    {
        internal static ConnectionWorkerReactor master;
        private static IncomingMessageReactor incomingMessageReactor;
        private static OutgoingMessageReactor outgoingMessageReactor;
        private static CleanerReactor cleanerReactor;

        static ServerRuntime()
        {
            ServiceRegistry.RegisterSwordServices();
        }

        private static bool _setupOk=false;
        public static void Setup(int maxPoolSize, int timeoutSeconds)
        {
            incomingMessageReactor = new IncomingMessageReactor(maxPoolSize);
            outgoingMessageReactor = new OutgoingMessageReactor();
            cleanerReactor = new CleanerReactor(timeoutSeconds);

            _setupOk =true;
        }

        public static void Start(int port)
        {
            if (master != null)
                throw new Exception("已经Start了");

            if (!_setupOk)
                Setup(20, 30);

            master = new ConnectionWorkerReactor(port);
            master.Start();

            incomingMessageReactor.Start();
            
            outgoingMessageReactor.Start();

            cleanerReactor.Start();
        }

        internal static void AddCommandToIncomingQueueRepository(Command cmd)
        {
            incomingMessageReactor.Enqueue(cmd);
        }

        internal static void AddCommandResultToOutgoingQueueRepository(CommandResult cmdResult)
        {
            outgoingMessageReactor.Enqueue(cmdResult);
        }
    }
}
