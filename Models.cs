using System;

namespace ProcessMSWebOrders
{
    class Models
    {
        public class Order
        {
            public int id { get; set; }
            public string name { get; set; }
            public string phone { get; set; }
            public DateTime pickUpTime { get; set; }
            public DateTime pickUpDate { get; set; }
            public OrderItem[] orderItems { get; set; }
        }

        public class OrderItem
        {
            public int id { get; set; }
            public string name { get; set; }
            public int qty { get; set; }
            public decimal price { get; set; }
            public ItemOption[] instructions { get; set; }
        }

        public class ItemOption
        {
            public string option { get; set; }
            public string item { get; set; }
            public decimal price { get; set; }
        }
    }
}
