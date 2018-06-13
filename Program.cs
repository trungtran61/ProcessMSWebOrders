using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Net.Http;
using static ProcessMSWebOrders.Models;

namespace ProcessWebOrders
{
    class Program
    {
        private static List<string> printLines = new List<string>();
        private static int ticketNumber = 0;
        private static int firstLineTopPosition = Convert.ToInt16(ConfigurationManager.AppSettings["FirstLineTopPosition"]);
        private static int xpos = Convert.ToInt16(ConfigurationManager.AppSettings["LeftPosition"]);
        private static int ypos = Convert.ToInt16(ConfigurationManager.AppSettings["TopPosition"]);
        private static int lineSpacing = Convert.ToInt16(ConfigurationManager.AppSettings["LineSpacing"]);
        private static string fontFamily = ConfigurationManager.AppSettings["FontFamily"];
        private static int fontSize = Convert.ToInt16(ConfigurationManager.AppSettings["FontSize"]);
        private static int smallFontSize = Convert.ToInt16(ConfigurationManager.AppSettings["SmallFontSize"]);
        private static int maxStringLength = Convert.ToInt16(ConfigurationManager.AppSettings["MaxStringLength"]);

        private static string contactInfo = string.Empty;
        private static string ticketTitle = string.Empty;

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
                        //ticketNumber = createNewTicket(order);
                        printLines.Clear();
                        contactInfo = string.Format("{0} {1}", order.phone, order.name.ToUpper());
                        ticketTitle = "PICK UP AT " + order.pickUpTime.ToShortTimeString();
                        foreach (OrderItem item in order.orderItems)
                        {
                            printLines.Add(string.Format("#{0} {1} ({2})", item.id, item.name, item.qty));
                            if (item.instructions != null)
                            {
                                foreach (ItemOption itemOption in item.instructions)
                                {
                                    printLines.Add(string.Format("     {0} {1}", itemOption.option, itemOption.item));
                                }
                            }
                            printLines.Add(Environment.NewLine);
                        }
                        Print(ConfigurationManager.AppSettings["Printer"]);                        
                    }
                }
            }

        }

        static int createNewTicket(Order order)
        {
            DataTable orderItems = new DataTable();
            orderItems.Columns.Add("ItemNumber", typeof(string));
            orderItems.Columns.Add("ItemName", typeof(string));
            orderItems.Columns.Add("Qty", typeof(string));
           
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["MSSalesDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("CreateTicket", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@WebOrderId", SqlDbType.Int).Value = order.id;
                        cmd.Parameters.Add("@CustomerName", SqlDbType.VarChar, 50).Value = order.name;
                        cmd.Parameters.Add("@Phone", SqlDbType.VarChar, 20).Value = order.phone;
                        cmd.Parameters.Add("@OrderItems", SqlDbType.Structured, 0).Value = orderItems;
                        con.Open();
                        ticketNumber = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    con.Close();
                }
            }
            catch
            {
                throw;
            }

            return ticketNumber;
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
            Font FontNormal = new Font(fontFamily, fontSize, FontStyle.Bold);
            Font FontSmall = new Font(fontFamily, smallFontSize, FontStyle.Regular);
            Graphics g = ppeArgs.Graphics;

            g.DrawString(ticketTitle, FontNormal, Brushes.Black, 30, firstLineTopPosition, new StringFormat());
            g.DrawString(string.Format("{0} {1} {2}", DateTime.Now.ToShortTimeString(),
                DateTime.Now.ToShortDateString(), "      Mekong Sandwiches"), FontSmall, Brushes.Black, xpos, firstLineTopPosition+30, new StringFormat());
            g.DrawString("========================================", FontSmall, Brushes.Black, xpos, firstLineTopPosition+40, new StringFormat());
            g.DrawString(contactInfo.Substring(0, contactInfo.Length > maxStringLength ? maxStringLength : contactInfo.Length - 1), FontNormal, 
                Brushes.Black, xpos, firstLineTopPosition+50, new StringFormat());
            g.DrawString("========================================", FontSmall, Brushes.Black, xpos, firstLineTopPosition+70, new StringFormat());

            foreach (string line in printLines)
            {
                g.DrawString(line + Environment.NewLine, FontNormal, Brushes.Black, xpos+5, ypos, new StringFormat());
                ypos += lineSpacing;
            }
        }
    }
}

