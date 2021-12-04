using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alaric.DB.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Alaric.DB
{
    public class DbInitializer : IDbInitializer
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DbInitializer(IServiceScopeFactory scopeFactory)
        {
            this._scopeFactory = scopeFactory;
        }



        public void SeedData()
        {
            using (var serviceScope = _scopeFactory.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetRequiredService<DataContext>())
                {
                    context.DisableTrack = true;
                    var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MOCK_DATA.json"));
                    var alldata = JsonConvert.DeserializeObject<IEnumerable<Trade>>(json).ToList();
                    var uniquetrades = alldata.GroupBy(a => a.sym).Select((IGrouping<string, Trade> arg) => arg.FirstOrDefault());
                    context.Trades.AddRange(uniquetrades);
                    context.SaveChanges();

                }
            }
        }
    }
}
