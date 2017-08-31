using Sword.Server.Pipes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Sword.CommandBus;

namespace Sword.Server.MessageReactors
{
    class IncomingMessageReactor
    {
        private IncomingQueueRepository incomingQueueRepository = new IncomingQueueRepository();
        private PipeProcessorPool pipeProcessorPool;

        public IncomingMessageReactor(int maxPoolSize)
        {
            this.pipeProcessorPool = new PipeProcessorPool(maxPoolSize);
        }

        public void Start()
        {
            pipeProcessorPool.PrepareIdlePipeProcessors();

            Task.Factory.StartNew(() =>
            {
                _start();
            }, TaskCreationOptions.LongRunning);
        }

        private void _start()
        {
            while (true)
            {
                var incomingMsg = incomingQueueRepository.DequeueBlock();

                if (incomingMsg.ConnectionWorker.IsTagged)
                    continue;

                PipeProcessor pipe = pipeProcessorPool.PickOneIdle();

                pipe.GiveTask(incomingMsg);
            }
        }

        internal void Enqueue(Command cmd)
        {
            incomingQueueRepository.Enqueue(cmd);
        }
    }
}
