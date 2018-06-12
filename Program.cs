using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Printing;
using System.Net.Http;
using static ProcessMSWebOrders.Models;

namespace ProcessWebOrders
{
    class Program
    {
        private static List<string> printLines = new List<string>();

        static void Main(string[] args)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(ConfigurationManager.AppSettings["GetOrdersURI"]).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    // by calling .Result you are synchronously reading the result
                    string responseString = responseContent.ReadAsStringAsync().Result;
                    List<Order> Orders = JsonConvert.DeserializeObject<List<Order>>(responseString);

                    foreach (Order order in Orders)
                    {
                        printLines.Clear();
                        printLines.Add(order.name);
                        printLines.Add(order.phone);
                        printLines.Add(order.pickUpTime.ToShortTimeString());
                        foreach (OrderItem item in order.orderItems)
                        {
                            printLines.Add(string.Format("{0} ({1})", item.name, item.qty));
                            printLines.Add(item.instructions == null? "": item.instructions);                            
                        }
                        Print(ConfigurationManager.AppSettings["Printer"]);
                    }
                }
            }

        }

        static void Print(string PrinterName)
        {
            PrintDocument doc = new PrintDocument();
            doc.PrinterSettings.PrinterName = PrinterName;
            doc.PrintPage += new PrintPageEventHandler(PrintHandler);
            doc.Print();
        }

        private static void PrintHandler(object sender, PrintPageEventArgs ppeArgs)
        {
            int xpos = 10;
            int ypos = 20;
            Font FontNormal = new Font("Verdana", 16, FontStyle.Bold);
            Graphics g = ppeArgs.Graphics;

            foreach (string line in printLines)
            {
                g.DrawString(line.ToUpper() + Environment.NewLine, FontNormal, Brushes.Black, xpos, ypos, new StringFormat());
                ypos += 30;
            }
        }
    }
}

