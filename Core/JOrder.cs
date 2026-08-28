using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;

namespace RestoJett.Core
{
    public class JOrder
    {
        public string Name { get; set; }
        public string Guid { get; set; }
        public OrderedDictionary Items = new OrderedDictionary();
        public string CustomerGuid { get; set; }
        public string Date { get; set; }
        public JDeliveryStatus DeliveryStatus { get; set; }
        public JOrderStatus OrderStatus { get; set; }
    }
}