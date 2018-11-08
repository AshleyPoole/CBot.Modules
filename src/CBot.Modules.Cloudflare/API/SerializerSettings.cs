using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CBot.Modules.Cloudflare.API
{
	internal static class SerializerSettings
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
