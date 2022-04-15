using System;
using System.Collections.Generic;
using System.Text;


namespace Crypto_Monitor
{
    public class CoinList
    {
        public List<Coin>? data { get; set; }
    }

    public class Coin
    {
        public int? id { get; set; }
        public string? symbol { get; set; }
        public string? name { get; set; }
        public int? rank { get; set; }
        public decimal? price_usd { get; set; }
        public float? percent_change_24h { get; set; }
        public float? percent_change_7d { get; set; }
        public decimal? volume24 { get; set; }
        public decimal? market_cap_usd { get; set; }


    }   
}

