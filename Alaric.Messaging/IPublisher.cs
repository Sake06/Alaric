using System;
using System.Diagnostics;
using Alaric.DB.Models;

namespace Alaric.Messaging
{
    public interface IPublisher
    {
        void Send(QueueModel model);

        void Init();

    }
}
