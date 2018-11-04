using Newtonsoft.Json;

namespace SharpBotCore.Modules.Cloudflare.API.Models
{
	public partial class Plan
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("price")]
		public long Price { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }

		[JsonProperty("frequency")]
		public string Frequency { get; set; }

		[JsonProperty("legacy_id")]
		public string LegacyId { get; set; }

		[JsonProperty("is_subscribed")]
		public bool IsSubscribed { get; set; }

		[JsonProperty("can_subscribe")]
		public bool CanSubscribe { get; set; }
	}
}