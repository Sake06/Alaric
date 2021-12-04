using System;
using System.ComponentModel.DataAnnotations;

namespace Alaric.DB.Models
{
    public class Trade
    {
        [Key]
        public string sym { get; set; }
        public decimal tradeprice { get; set; }
        public int tradesize { get; set; }
        public char partid { get; set; }
    }
}
 
