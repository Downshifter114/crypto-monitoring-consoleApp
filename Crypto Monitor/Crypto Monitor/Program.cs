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
        //Global variables.
        public static string command;
        public static string orderbyInput;
        public static int pageInput = 1;
        public static Timer newTimer = new Timer();
        static void Main(string[] args)
        {
            Help();
            GetInput();
            

            Console.ReadKey();
        }        

        public static void Help()
        {
            Write("WELCOME!", ConsoleColor.DarkYellow);
            Write("\n-The format to start monitoring is:", ConsoleColor.DarkYellow); Write(" 'start, page x, orderby x'", ConsoleColor.DarkRed);
            Write("\n-Page options are:", ConsoleColor.DarkYellow); Write(" numbers. (0, 1, 2, ...)", ConsoleColor.DarkRed);
            Write("\n-Orderby options are:", ConsoleColor.DarkYellow); Write(" ('price', '24hrs', '7days')", ConsoleColor.DarkRed);
            Write("\n-To end the repeating process type", ConsoleColor.DarkYellow); Write(" 'end'", ConsoleColor.DarkRed);
            Write("\n-To close the app, type", ConsoleColor.DarkYellow); Write(" 'quit'", ConsoleColor.DarkRed);
            Write("\n-To see this message again, type", ConsoleColor.DarkYellow); Write(" 'help'", ConsoleColor.DarkRed);
            Write("\nEXAMPLE: ", ConsoleColor.DarkYellow); Write("start, page 1, orderby rank", ConsoleColor.DarkRed);
        }

        //Write("\nInvalid input. Try again! EXAMPLE: ", ConsoleColor.DarkYellow); Write("start, page 1, orderby rank", ConsoleColor.DarkRed);
        public static void GetInput()
        {
            Console.Write("\nYour Input:");
            string userInput = Console.ReadLine();
            List<string> list = userInput.Split(", ").ToList();
            if (list.Count == 1)
            {
                if (list[0] == "quit")
                    System.Environment.Exit(0);
                else if (list[0] == "end")
                {
                    newTimer.Stop();
                    GetInput();
                }
                else
                    Invalid();

            }
            else if (list.Count == 3)
            {
                if (list[0] == "start" && list[1].Contains("page ") && list[2].Contains("orderby "))
                {
                    //Double checking
                    List<string> temporaryPage = list[1].Split(' ').ToList();
                    if (temporaryPage.Count != 2 || !IsNumeric(temporaryPage[1]) || temporaryPage[1].Count() > 2) 
                        Invalid();
                    List<string> temporaryOrder = list[2].Split(' ').ToList(); 
                    if (temporaryOrder.Count != 2 || temporaryOrder[1] != "price" && temporaryOrder[1] != "24hrs" && temporaryOrder[1] != "7days" && temporaryOrder[1] != "rank") 
                        Invalid();

                    command = list[0];
                    //pageinput
                    orderbyInput = temporaryPage[1];

                    if (command == "start")
                        StartTimer(10);

                    GetInput();
                }
                else
                    Invalid();
            }
            else            
                Invalid();
            
        }

        public static void Invalid()
        {
            Write("\nInvalid input. Try again! EXAMPLE: ", ConsoleColor.DarkYellow); Write("start, page 1, orderby rank", ConsoleColor.DarkRed);
            GetInput();
        }


        public static void StartTimer(int seconds)
        {
            int startPoint = (pageInput - 1) * 100;
            string url = "https://api.coinlore.net/api/tickers/?start=" + startPoint + "&limit=100";           
            //Timer Added Versions            
            newTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            newTimer.Interval = seconds * 1000;
            newTimer.Start();
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            int startPoint = (pageInput - 1) * 100;
            string url = "https://api.coinlore.net/api/tickers/?start=" + startPoint + "&limit=100";
            CoinList coinList = getMarketInfo(url, orderbyInput);
            WriteTable(coinList);
        }

        public static CoinList getMarketInfo(string url, string orderby)
        {
            var _url = url;
            string JSONstring;
            WebRequest request = WebRequest.Create(_url);
            WebResponse response = request.GetResponse();
            Console.WriteLine("\n\nServer Connection Status: " + ((HttpWebResponse)response).StatusDescription);
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
            if (orderby == null || orderby == "rank")
                sortedPrice = coinList;

            return sortedPrice;
        }

        public static void WriteTable(CoinList list)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine(String.Format("{0,-4} | {1,-27} | {2, -10} | {3, -10} | {4, -10}", "Rank", "Name", "Price", "24Hrs", "7Days"));
            Console.WriteLine("-------------------------------------------------------------------------------");
            foreach (var coin in list.data)
            {
                if (coin.percent_change_7d >= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(String.Format("{0,-4} | {1,-27} | {2, 10} | {3, 10} | {4, 10}",
                    "#" + coin.rank,
                    coin.name + '[' + coin.symbol + ']',
                    coin.price_usd + "$",
                    coin.percent_change_24h + "%",
                    coin.percent_change_7d + "%"));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(String.Format("{0,-4} | {1,-27} | {2, 10} | {3, 10} | {4, 10}",
                    "#" + coin.rank,
                    coin.name + '[' + coin.symbol + ']',
                    coin.price_usd + "$",
                    coin.percent_change_24h + "%",
                    coin.percent_change_7d + "%"));
                }

            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-------------------------------------------------------------------------------\n\n");
        }

        public static void Write(string writeText, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(writeText);
            Console.ResetColor();
        }

        public static bool IsNumeric(string value)
        {
            return value.All(char.IsNumber);
        }
    }


}
