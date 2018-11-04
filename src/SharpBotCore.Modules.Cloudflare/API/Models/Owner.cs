using Newtonsoft.Json;

namespace SharpBotCore.Modules.Cloudflare.API.Models
{
	public partial class Owner
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("email")]
		public string Email { get; set; }

		[JsonProperty("owner_type")]
		public string OwnerType { get; set; }
	}
}