using System;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CBot.Modules.NewRelic.API.Models
{
	public partial class ApiResponse
	{
		[JsonProperty("applications")]
		public Application[] Applications { get; set; }
	}

	public partial class Application
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("language")]
		public string Language { get; set; }

		[JsonProperty("health_status")]
		public string HealthStatus { get; set; }

		[JsonProperty("reporting")]
		public bool Reporting { get; set; }

		[JsonProperty("last_reported_at")]
		public DateTimeOffset LastReportedAt { get; set; }

		[JsonProperty("application_summary")]
		public Summary ApplicationSummary { get; set; }

		[JsonProperty("end_user_summary")]
		public Summary EndUserSummary { get; set; }

		[JsonProperty("settings")]
		public Settings Settings { get; set; }

		[JsonProperty("links")]
		public Links Links { get; set; }
	}

	public partial class Summary
	{
		[JsonProperty("response_time")]
		public double ResponseTime { get; set; }

		[JsonProperty("throughput")]
		public double Throughput { get; set; }

		[JsonProperty("error_rate", NullValueHandling = NullValueHandling.Ignore)]
		public double? ErrorRate { get; set; }

		[JsonProperty("apdex_target")]
		public long ApdexTarget { get; set; }

		[JsonProperty("apdex_score")]
		public double ApdexScore { get; set; }
	}

	public partial class Links
	{
		[JsonProperty("application_instances")]
		public long[] ApplicationInstances { get; set; }

		[JsonProperty("alert_policy")]
		public long AlertPolicy { get; set; }

		[JsonProperty("application_hosts")]
		public long[] ApplicationHosts { get; set; }
	}

	public partial class Settings
	{
		[JsonProperty("app_apdex_threshold")]
		public double AppApdexThreshold { get; set; }

		[JsonProperty("end_user_apdex_threshold")]
		public long EndUserApdexThreshold { get; set; }

		[JsonProperty("enable_real_user_monitoring")]
		public bool EnableRealUserMonitoring { get; set; }

		[JsonProperty("use_server_side_config")]
		public bool UseServerSideConfig { get; set; }
	}

	public partial class ApiResponse
	{
		public static ApiResponse FromJson(string json) =>
			JsonConvert.DeserializeObject<ApiResponse>(json, Converter.Settings);
	}

	internal static class Converter
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
																{
																	MetadataPropertyHandling =
																		MetadataPropertyHandling.Ignore,
																	DateParseHandling = DateParseHandling.None,
																	Converters =
																	{
																		new IsoDateTimeConverter
																		{
																			DateTimeStyles =
																				DateTimeStyles.AssumeUniversal
																		}
																	},
																};
	}
}
