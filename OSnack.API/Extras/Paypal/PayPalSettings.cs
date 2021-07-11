using Newtonsoft.Json;

namespace OSnack.API.Extras.Paypal
{
   public class PayPalSettings
   {

      [JsonProperty(PropertyName = "ClientId")]
      public string ClientId { get; set; }
      [JsonProperty(PropertyName = "ClientSecret")]
      public string ClientSecret { get; set; }
      [JsonProperty(PropertyName = "IsProduction")]
      public bool IsProduction { get; set; }
      [JsonProperty(PropertyName = "CurrencyCode")]
      public string CurrencyCode { get; set; }
   }
}
