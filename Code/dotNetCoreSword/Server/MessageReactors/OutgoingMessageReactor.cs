using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Sword.CommandBus;

namespace Sword.Server.MessageReactors
{
    class OutgoingMessageReactor
    {
        private OutgoingQueueRepository outgoingQueueRepository = new OutgoingQueueRepository();

        internal void Start()
        {
            Task.Factory.StartNew(() =>
            {
                _start();
            }, TaskCreationOptions.LongRunning);
        }

        private void _start()
        {
            while (true)
            {
                var outgoingMsg = outgoingQueueRepository.DequeueBlock();

                if (outgoingMsg.ConnectionWorker.IsTagged)
                    continue;

                outgoingMsg.ConnectionWorker.SendResponse(outgoingMsg);
            }
        }

        internal void Enqueue(CommandResult cmdResult)
        {
            outgoingQueueRepository.Enqueue(cmdResult);
        }
    }
}
