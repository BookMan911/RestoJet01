

public class LanguageService
{
    
   private  Dictionary<string, Hashtable> _languages =  new Dictionary<string, Hashtable>();

   private Hashtable _currentLanguage = new Hashtable();

   
   public void For(string lang)
   {
      if(_languages.Contains(lang)){
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
          _languages.Add(filename, JsonConvert.DeserializeObject(json));// here should be hashtable every word is key and value is the translation

      }catch(Exception e){
          return e;
      }
   }

  
}