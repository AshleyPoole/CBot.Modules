using System;
using System.Collections.Generic;

using SharpBotCore.Messaging.Domain;
using SharpBotCore.Middleware.Domain;
using SharpBotCore.Middleware.Domain.Handlers;

namespace SharpBotCore.Modules.Cloudflare
{
	public class CloudflareMiddleware : MiddlewareBase
	{
		private readonly IManageCloudflare cloudflareManager;

		public CloudflareMiddleware(IManageCloudflare cloudflareManager)
		{
			this.cloudflareManager = cloudflareManager;

			this.HandlerMappings = new[]
									{
										new HandlerMapping
										{
											Handlers = StartsWithHandler.For($"{Parameters.Purge} {Parameters.Cloudflare} zone"),
											EvaluatorFunc = this.PurgeZone,
											Description = $"Purges Cloudflare cache for specified zone. { GetPurgeZoneHelpText()}",
											VisibleInHelp = true
										},
										new HandlerMapping
										{
											Handlers = StartsWithHandler.For($"{Parameters.Purge} {Parameters.Cloudflare} tag"),
											EvaluatorFunc = this.PurgeCacheTag,
											Description = $"Purges Cloudflare cache for specified cache tag. {GetPurgeCacheTagHelpText()}",
											VisibleInHelp = true
										}
									};
		}

		private IEnumerable<ResponseMessage> PurgeZone(IncomingMessage incomingMessage, IHandler matchedHandle)
		{
			yield return incomingMessage.IndicateTypingOnChannel();

			if (!CommandWellFormatted(incomingMessage.TargetedText, requiredCommandLength: 4))
			{
				yield return incomingMessage.ReplyToChannel($"Sorry, you must provide a zone name in order to purge the cache. I.e {GetPurgeZoneHelpText()}.");
				yield break;
			}

			var zoneName = GetCleanZoneName(GetPositionalElementFromTargetText(incomingMessage.TargetedText, position: 3));
			var result = this.cloudflareManager.PurgeZone(zoneName, incomingMessage.Username).GetAwaiter().GetResult();

			switch (result.OperationStatus)
			{
				case OperationStatus.Success:
					yield return incomingMessage.ReplyToChannel($"Successfully purged cache for {result.ZoneName}.");
					yield break;
				case OperationStatus.ZoneNotFound:
					yield return incomingMessage.ReplyToChannel($"Sorry, no zone could be found for {result.ZoneName}.");
					yield break;
				default:
					yield return incomingMessage.ReplyToChannel($"Sorry, something went wrong when purging {result.ZoneName}.");
					yield break;
			}
		}

		private IEnumerable<ResponseMessage> PurgeCacheTag(IncomingMessage incomingMessage, IHandler matchedHandle)
		{
			yield return incomingMessage.IndicateTypingOnChannel();

			if (!CommandWellFormatted(incomingMessage.TargetedText, requiredCommandLength: 6))
			{
				yield return incomingMessage.ReplyToChannel($"Sorry, you must provide a zone and cache tag in order to purge the cache. I.e {GetPurgeCacheTagHelpText()}.");
				yield break;
			}

			var cacheTag = GetPositionalElementFromTargetText(incomingMessage.TargetedText, position: 3);
			var zoneName = GetCleanZoneName(GetPositionalElementFromTargetText(incomingMessage.TargetedText, position: 5));

			var result = this.cloudflareManager.PurgeZoneCacheTag(zoneName, cacheTag, incomingMessage.Username).GetAwaiter().GetResult();

			switch (result.OperationStatus)
			{
				case OperationStatus.Success:
					yield return incomingMessage.ReplyToChannel($"Successfully purged cache for {result.ZoneName} of {cacheTag} cache tag.");
					yield break;
				case OperationStatus.ZoneNotFound:
					yield return incomingMessage.ReplyToChannel($"Sorry, no zone could be found for {result.ZoneName}.");
					yield break;
				default:
					yield return incomingMessage.ReplyToChannel($"Sorry, something went wrong when purging {result.ZoneName} of {cacheTag} cache tag.");
					yield break;
			}
		}

		private static bool CommandWellFormatted(string message, int requiredCommandLength)
		{
			var commandWordLength = message.Split(" ", StringSplitOptions.RemoveEmptyEntries).Length;
			return commandWordLength == requiredCommandLength;
		}

		private static string GetPurgeCacheTagHelpText()
		{
			return $"`{Parameters.Purge} {Parameters.Cloudflare} tag PROD-MyApp zone ashleypoole.co.uk `";
		}

		private static string GetPurgeZoneHelpText()
		{
			return $"`{Parameters.Purge} {Parameters.Cloudflare} zone ashleypoole.co.uk`";
		}

		private static string GetPositionalElementFromTargetText(string messageText, int position)
		{
			return messageText.Split(" ", StringSplitOptions.RemoveEmptyEntries)[position];
		}

		private static string GetCleanZoneName(string zoneName)
		{
			return zoneName.Contains("|") ? zoneName.Substring(zoneName.IndexOf("|", StringComparison.Ordinal) + 1).Replace(">", string.Empty) : zoneName;
		}
	}
}
