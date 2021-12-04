using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Alaric.DB;
using Alaric.DB.Models;
using Alaric.Messaging.RabbitMq;
using Alaric.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Alaric.Messaging
{
    public class Orchestrator
    {  
        private readonly IServiceScopeFactory factory;
        private readonly ILogger<RabbitMqConsumer> logger;

        public Orchestrator(IServiceScopeFactory factory, IOptions<BaseOptions> config, ILogger<RabbitMqConsumer> logger)
        {
            this.factory = factory;
            Config = config;
            this.logger = logger;
        }

        public IOptions<BaseOptions> Config { get; }

        public async Task MergeAsync(QueueModel model)
        {
            if (model.AuditLog.InsertedByApp != Config.Value.ApplicationId)
            {


                using (var scope = this.factory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                    context.DisableTrack = true;
                    Trade trade = await context.Trades.FindAsync(model.AuditLog.RecordId);
                    bool insert = false;
                    if (trade == null)
                    {
                        trade = new Trade();
                        insert = true;
                    }
                    else
                    {
                        var getlatestlog = context.AuditLogs.Where(x => x.RecordId == model.AuditLog.RecordId).OrderByDescending(x => x.TimeStamp).FirstOrDefault();
                        if (getlatestlog == null || getlatestlog.TimeStamp < model.AuditLog.TimeStamp)
                        {
                            foreach (var item in model.AuditLog.LogDetails)
                            {
                                PropertyInfo propertyInfo = typeof(Trade).GetProperty(item.PropertyName);
                                propertyInfo.SetValue(trade, Convert.ChangeType(item.NewValue, propertyInfo.PropertyType), null);
                            }
                            if (insert)
                            {
                                context.Trades.Add(trade);

                            }
                            else
                            {
                                context.Trades.Update(trade);

                            }
                        }
                    }

                    context.AuditLogs.Add(model.AuditLog);
                    await context.SaveChangesAsync(default);
                    logger.LogDebug("Received And Merged :" + JsonConvert.SerializeObject(model.AuditLog));

                }

            }
            else
            {
                logger.LogDebug("Didnt Merge, Passed Of Application :" + JsonConvert.SerializeObject(model.AuditLog.InsertedByApp));

            }

        }
    }
}
