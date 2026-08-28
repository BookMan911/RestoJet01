using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Collections;

namespace RestoJett.Core
{
    public class RestaurantService
    {
        public JMenu MainMenu { get; set; }

        //rewrite the service to be more like asp.net core service style and use dependency injection and make it more modular and easy to test and maintain
        // enable /add /edit /remove for everything with follwing rules
        // here is the service its asp.net style service to manage ( meals , users, customers,orders)
        // the method style is result error style like
      //  public Tuple<Exception,List<JUser>> getUsers(){} all operations like that to ease of trace errors
      // all operation needs logged user , so the normal user cant do all operations 
      // public Tuple<Exception,List<JUser>> getUsers(Juser loggeduser){} like this 
      // every action for each user is logged and saved so admin can know who did what
      // ceate cshtml page (RPAGE) that controls the service , and make that page modren UI
      // the RPAGE uses the Languageservice in top most we do ( langservice.For("lang")) and for each word in the page we use ( langservice.Transilate("word"))

        
    }
}