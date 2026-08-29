using System.Collections.Generic;

namespace RestoJett.Core
{
    public class JOrder
    {
        public string Name { get; set; }
        public string Guid { get; set; }
        public Dictionary<string, JOrderItem> Items { get; set; } = new Dictionary<string, JOrderItem>();
        public string CustomerGuid { get; set; }
        public string Date { get; set; }
        public JDeliveryStatus DeliveryStatus { get; set; }
        public JOrderStatus OrderStatus { get; set; }
        public JPaymentType PaymentType { get; set; }
    }
}