using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alaric.DB;
using Alaric.DB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Alaric.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TradeController : ControllerBase
    {
        private readonly ILogger<TradeController> _logger;

        public DataContext _context { get; }

        public TradeController(ILogger<TradeController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpPost]
        public async void Write(Trade s)
        {
            var trade = await _context.Trades.FindAsync(s.sym);
            if (trade == null)
            {
                _context.Trades.Add(s);
            }
            else
            {
                trade.tradeprice = s.tradeprice;
                trade.tradesize = s.tradesize;
                trade.partid = s.partid;
                _context.Trades.Update(trade);
            }
            
       
            await _context.SaveChangesAsync(new System.Threading.CancellationToken()); 
        }
        [HttpGet("{sym}")]
        public async Task<ActionResult<Trade>> Read(string sym)
        {
            var trade = await _context.Trades.FindAsync(sym);

            if (trade == null)
            {
                return NotFound();
            }

            return trade;
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Trade>>> GetAll()
        {
            return await _context.Trades.ToListAsync();


        }
    }
}
