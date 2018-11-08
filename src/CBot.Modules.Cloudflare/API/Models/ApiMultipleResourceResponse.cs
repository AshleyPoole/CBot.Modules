using Newtonsoft.Json;

namespace CBot.Modules.Cloudflare.API.Models
{
	public partial class ApiMultipleResourceResponse
	{
		[JsonProperty("success")]
		public bool Success { get; set; }

		[JsonProperty("errors")]
		public object[] Errors { get; set; }

		[JsonProperty("messages")]
		public object[] Messages { get; set; }

		[JsonProperty("result")]
		// TODO: CHECK THIS WORKS
		public Zone[] Zone { get; set; }

		[JsonProperty("result_info")]
		public ResultInfo ResultInfo { get; set; }
	}
}
