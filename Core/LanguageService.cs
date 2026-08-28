using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RestoJett.Core
{
    public class LanguageService
    {
        // this is language service shluld be init and created at program , in every page we will select language from For method from the url of the page , and every word is taken from translate function if no word the word itself will returned
        
        
       private Dictionary<string, Hashtable> _languages =  new Dictionary<string, Hashtable>();

       private Hashtable _currentLanguage = new Hashtable();

       
       public void For(string lang)
       {
          if(_languages.ContainsKey(lang)){
                _currentLanguage = _languages[lang];
          }
       }
       public string Transilate(string word){

         if(_currentLanguage.ContainsKey(word)){
             return _currentLanguage[word].ToString();

           }else{

           
        
               return word;
           }
       }
        

       public Exception loadFromJson(string filepath)
       {
          try{

              var json = File.ReadAllText(filepath);
              var filename = Path.GetFileNameWithoutExtension(filepath);
              var hashtable = JsonConvert.DeserializeObject<Hashtable>(json);
              if(hashtable != null)
              {
                  _languages[filename] = hashtable;
              }

          }catch(Exception e){
              return e;
          }
          return null;
       }

      
    }
}