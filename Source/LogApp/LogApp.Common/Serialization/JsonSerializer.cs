using LogApp.Common.Model;
using Newtonsoft.Json;


namespace LogApp.Common.Serialization
{
    public static class JsonSerializer
    {
        #region Members
        public static string ConvertToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static ApplicationInfoCollection ConvertToObject(string json)
        {
            return JsonConvert.DeserializeObject<ApplicationInfoCollection>(json);
        } 
        #endregion
    }
}
