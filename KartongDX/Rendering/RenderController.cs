using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Rendering
{

    class RenderController
    {
        public enum QueueType
        {
            Q3D,
            QUI
        }

        private RenderQueue renderQueue;
        private RenderQueue renderQueueUI;

        public RenderController()
        {
            renderQueue = new RenderQueue();
            renderQueueUI = new RenderQueue();
        }
        
        public RenderQueue GetRenderQueue(QueueType queueType)
        {
            if (queueType == QueueType.Q3D)
                return renderQueue;
            else
                return renderQueueUI;
        }

        public void AddToQueue(QueueType type, RenderItem renderItem)
        {
            
        }

        public void ClearRenderQueues()
        {
            renderQueue.Clear();
            renderQueueUI.Clear();
        }
    }
}
