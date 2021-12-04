using System;
using System.Threading.Tasks;
using Alaric.DB.Models;

namespace Alaric.Messaging
{
    public interface IConsumer
    {
        Task HandleMessageAsync(QueueModel model);
    }
}
