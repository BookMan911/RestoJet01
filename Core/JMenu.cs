using System.Collections.Specialized;

namespace RestoJett.Core
{
    public class JMenu
    {
        public string Guid { get; set; }
        public string Name { get; set; }
        public OrderedDictionary Items = new OrderedDictionary(); // MealGuid-> JMeal
    }
}