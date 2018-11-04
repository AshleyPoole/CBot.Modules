using Newtonsoft.Json;

namespace SharpBotCore.Modules.Cloudflare.API.Models
{
	public partial class ApiSingleResourceResponse
	{
		[JsonProperty("result")]
		public Zone Zone { get; set; }

		[JsonProperty("success")]
		public bool Success { get; set; }

		[JsonProperty("errors")]
		public object[] Errors { get; set; }

		[JsonProperty("messages")]
		public object[] Messages { get; set; }
	}
}