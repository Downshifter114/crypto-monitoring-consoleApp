using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Timers;
using System.Collections.Generic;


namespace Crypto_Monitor
{
    class Program
    {
        static Timer timer;
        static string url;
        //User Input variables.
        static string userInput;
        static int pageInput;
        static string orderInput;
        static void Main(string[] args)
        {
            WakeUp();    
            while (true)
            {
                if (userInput == "clear") Console.Clear();
                if (userInput == "help") Help();
                if (userInput == "end") Stop();
                if (userInput == "quit") System.Environment.Exit(0);
                if (userInput == "start") Start(); //Start the timer.
                GetInput();
            }
        }      
        
        //Fundamental Methods.
        private static void WakeUp()
        {
            int seconds = 240;
            //Setting up the timer
            timer = new System.Timers.Timer();
            timer.Interval = seconds * 1000;
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            //WakeUp
            Help();
            GetInput();            
        }
        static void GetInput()
        {
            Write("\nYour Input:", ConsoleColor.DarkYellow);
            userInput = Console.ReadLine();
            //Assigning userInput variables.
            if (userInput != "clear" && userInput != "end" && userInput != "quit" && userInput != "help" && !isValid(userInput)) 
                userInput = "nonesense";
            else if (isValid(userInput))
            {
                List<string> primaryList = userInput.Split(", ").ToList();
                List<string> secondaryList = primaryList[1].Split(' ').ToList();
                pageInput = int.Parse(secondaryList[1]);
                secondaryList = primaryList[2].Split(' ').ToList();
                orderInput = secondaryList[1];
                userInput = "start";
            }
            //Generating url according to page number.
            url = "https://api.coinlore.net/api/tickers/?start=" + ((pageInput - 1) * 50).ToString() + "&limit=50";
        }
        static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Console.Clear();
            CoinList coinList = getMarketInfo(url, orderInput);
            WriteTable(coinList);
        }
        static void TableInAdvance()
        {
            Console.Clear();
            CoinList coinList = getMarketInfo(url, orderInput);
            WriteTable(coinList);
        }
        static void Start()
        {
            Write("\nBringing the results...\n", ConsoleColor.Cyan);
            TableInAdvance();
            timer.Enabled = true;
        }
        static void Stop()
        {            
            timer.Enabled = false;
            Console.Clear();
            Write("Process ended. To see the commands, type", ConsoleColor.DarkYellow); Write(" 'help'\n", ConsoleColor.DarkRed);
        }
        static void WriteTable(CoinList list)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine(String.Format("{0,-4} | {1,-30} | {2, -14} | {3, -10} | {4, -10} | {5, -20} | {6, -20} |", "Rank", "Name", "Price", "24Hours%", "7Days%", "24HoursVolume", "Marketcap"));
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------------------");
            foreach (var coin in list.data)
            {
                if (coin.percent_change_7d >= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(String.Format("{0,-4} | {1,-30} | {2, 14} | {3, 10} | {4, 10} | {5, -20} | {6, -20} |",
                    "#" + coin.rank,
                    coin.name + '[' + coin.symbol + ']',
                    coin.price_usd + "$",
                    coin.percent_change_24h + "%",
                    coin.percent_change_7d + "%",
                    coin.volume24 + "$",
                    coin.market_cap_usd + "$"));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(String.Format("{0,-4} | {1,-30} | {2, 14} | {3, 10} | {4, 10} | {5, -20} | {6, -20} |",
                    "#" + coin.rank,
                    coin.name + '[' + coin.symbol + ']',
                    coin.price_usd + "$",
                    coin.percent_change_24h + "%",
                    coin.percent_change_7d + "%",
                    coin.volume24 + "$",
                    coin.market_cap_usd + "$"));
                }

            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------------------");
            Write("\nThis table will be refreshing every 4mins, to do something else please type, 'end' first.\n\n", ConsoleColor.Cyan);
        }                
        static CoinList getMarketInfo(string url, string orderby)
        {
            var _url = url;
            string JSONstring;
            WebRequest request = WebRequest.Create(_url);
            WebResponse response = request.GetResponse();
            Write("\n\nServer Connection Status: " + ((HttpWebResponse)response).StatusDescription + "\n", ConsoleColor.Cyan);
            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                JSONstring = reader.ReadToEnd();
            }
            CoinList coinList = JsonConvert.DeserializeObject<CoinList>(JSONstring);
            CoinList sortedPrice = new CoinList();
            response.Close();

            //Order
            if (orderby == "price")
                sortedPrice.data = coinList.data.OrderByDescending(x => x.price_usd).ToList();
            if (orderby == "24hrs")
                sortedPrice.data = coinList.data.OrderByDescending(x => x.percent_change_24h).ToList();
            if (orderby == "7days")
                sortedPrice.data = coinList.data.OrderByDescending(x => x.percent_change_7d).ToList();
            if (orderby == "24volume")
                sortedPrice.data = coinList.data.OrderByDescending(x => x.volume24).ToList();
            if (orderby == "marketcap")
                sortedPrice.data = coinList.data.OrderByDescending(x => x.market_cap_usd).ToList();
            if (orderby == null || orderby == "rank")
                sortedPrice = coinList;

            return sortedPrice;
        }
        static bool isValid(string input)
        {
            //General Check (checks 'start' spesifically).
            List<string> primaryList = input.Split(", ").ToList();
            if (primaryList.Count != 3) return false;
            if (primaryList[0] != "start") return false;
            if (!primaryList[1].Contains("page ")) return false;
            if (!primaryList[2].Contains("orderby ")) return false;
            //Spesific check for page input.
            List<string> secondaryList = primaryList[1].Split(' ').ToList();
            if (secondaryList.Count != 2) return false;
            if (!int.TryParse(secondaryList[1], out int ignoreMe)) return false;
            //Spesific check for orderby input.
            secondaryList = primaryList[2].Split(' ').ToList();
            if (secondaryList.Count != 2) return false;
            if (secondaryList[1] != "rank" && secondaryList[1] != "24hrs" && secondaryList[1] != "7days" && secondaryList[1] != "price" && secondaryList[1] != "24volume" && secondaryList[1] != "marketcap") return false;
            //If passed all tests, return true.
            return true;
        }
        

        //Helper Methods.
        static void Help()
        {
            Write("\nWELCOME!", ConsoleColor.DarkYellow);
            Write("\n-The format to start monitoring is:", ConsoleColor.DarkYellow); Write(" 'start, page x, orderby x'", ConsoleColor.DarkRed);
            Write("\n-Page options are:", ConsoleColor.DarkYellow); Write(" numbers. (0, 1, 2, ...)", ConsoleColor.DarkRed);
            Write("\n-Orderby options are:", ConsoleColor.DarkYellow); Write(" ('price', '24hrs', '7days', '24volume', 'marketcap')", ConsoleColor.DarkRed);
            Write("\n-To end the repeating process type", ConsoleColor.DarkYellow); Write(" 'end'", ConsoleColor.DarkRed);
            Write("\n-To close the app, type", ConsoleColor.DarkYellow); Write(" 'quit'", ConsoleColor.DarkRed);
            Write("\n-To see this message again, type", ConsoleColor.DarkYellow); Write(" 'help'", ConsoleColor.DarkRed);
            Write("\n-To clean the console, type", ConsoleColor.DarkYellow); Write(" 'clear'", ConsoleColor.DarkRed);
            Write("\nEXAMPLE: ", ConsoleColor.DarkYellow); Write("start, page 1, orderby rank\n", ConsoleColor.DarkRed);
        }
        static void Write(string writeText, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(writeText);
            Console.ResetColor();
        }        
    }
}
