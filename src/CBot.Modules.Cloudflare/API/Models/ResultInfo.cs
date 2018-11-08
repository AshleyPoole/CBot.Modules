using Newtonsoft.Json;

namespace CBot.Modules.Cloudflare.API.Models
{
	public partial class ResultInfo
	{
		[JsonProperty("page")]
		public long Page { get; set; }

		[JsonProperty("per_page")]
		public long PerPage { get; set; }

		[JsonProperty("count")]
		public long Count { get; set; }

		[JsonProperty("total_count")]
		public long TotalCount { get; set; }
	}
}