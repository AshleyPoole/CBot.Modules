using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using CBot.Modules.IncidentManagement.Models;

namespace CBot.Modules.IncidentManagement
{
	internal class StorageClient : IIncidentStorage
	{
		private CloudTable incidentTable;

		public StorageClient(ModuleConfiguration configuration)
		{
			var storageAccount = CloudStorageAccount.Parse(configuration.AzureConnectionString);
			var tableClient = storageAccount.CreateCloudTableClient();

			this.incidentTable = tableClient.GetTableReference("incidents");
		}

		public async Task<int> GetNextRowKey(string incidentDateTime)
		{
			await this.EnsureIncidentsTableExists();

			var query = new TableQuery<Incident>().Where(
				TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, incidentDateTime));

			TableContinuationToken token = null;
			do
			{
				var incidentsQuery = await this.incidentTable.ExecuteQuerySegmentedAsync(query, token);
				token = incidentsQuery.ContinuationToken;

				if (incidentsQuery.Results.Any())
				{
					return int.Parse(incidentsQuery.Results.OrderByDescending(x => x.DeclaredDateTimeUtc).First().RowKey) + 1;

				}

				return 1;

			} while (token != null);
		}

		public async Task<Incident> PersistNewIncident(Incident incident)
		{
			await this.EnsureIncidentsTableExists();

			var insertOperation = TableOperation.Insert(incident);
			var insertResult = await this.incidentTable.ExecuteAsync(insertOperation);

			return (Incident)insertResult.Result;
		}

		public async Task<Incident> GetIncidentByChannelName(string channelName)
		{
			await this.EnsureIncidentsTableExists();

			var query = new TableQuery<Incident>().Where(
				TableQuery.GenerateFilterCondition(nameof(Incident.ChannelName), QueryComparisons.Equal, channelName));

			TableContinuationToken token = null;
			do
			{
				var incidentsQuery = await this.incidentTable.ExecuteQuerySegmentedAsync(query, token);
				token = incidentsQuery.ContinuationToken;

				return !incidentsQuery.Results.Any()
							? null
							: incidentsQuery.Results.OrderByDescending(x => x.DeclaredDateTimeUtc).First();

			} while (token != null);
		}

		public async Task<Incident> GetIncidentById(Guid id)
		{
			await this.EnsureIncidentsTableExists();

			var query = new TableQuery<Incident>().Where(TableQuery.GenerateFilterConditionForGuid(
				nameof(Incident.Id),
				QueryComparisons.Equal,
				id));

			TableContinuationToken token = null;
			do
			{
				var incidentsQuery = await this.incidentTable.ExecuteQuerySegmentedAsync(query, token);
				token = incidentsQuery.ContinuationToken;

				return !incidentsQuery.Results.Any()
							? null
							: incidentsQuery.Results.First();

			} while (token != null);
		}

		public async Task<Incident> GetIncidentByChannelId(string channelId)
		{
			await this.EnsureIncidentsTableExists();

			var query = new TableQuery<Incident>().Where(
				TableQuery.GenerateFilterCondition(nameof(Incident.ChannelId), QueryComparisons.Equal, channelId));

			TableContinuationToken token = null;
			do
			{
				var incidentsQuery = await this.incidentTable.ExecuteQuerySegmentedAsync(query, token);
				token = incidentsQuery.ContinuationToken;

				return !incidentsQuery.Results.Any()
							? null
							: incidentsQuery.Results.OrderByDescending(x => x.DeclaredDateTimeUtc).First();

			} while (token != null);
		}

		public async Task<Incident> UpdateIncident(Incident incident)
		{
			await this.EnsureIncidentsTableExists();

			var updateOperation = TableOperation.Replace(incident);
			var updateResult = await this.incidentTable.ExecuteAsync(updateOperation);

			return (Incident)updateResult.Result;
		}

		public async Task<List<Incident>> GetActiveIncidents()
		{
			await this.EnsureIncidentsTableExists();

			var query = new TableQuery<Incident>().Where(
				TableQuery.GenerateFilterConditionForBool(nameof(Incident.Closed), QueryComparisons.Equal, false));

			TableContinuationToken token = null;
			do
			{
				var incidentsQuery = await this.incidentTable.ExecuteQuerySegmentedAsync(query, token);
				token = incidentsQuery.ContinuationToken;

				return incidentsQuery.Results;

			} while (token != null);
		}

		public async Task<List<Incident>> GetRecentIncidents(int days)
		{
			await this.EnsureIncidentsTableExists();

			var query = new TableQuery<Incident>().Where(
				TableQuery.GenerateFilterCondition(
					nameof(Incident.PartitionKey),
					QueryComparisons.GreaterThanOrEqual,
					DateTime.UtcNow.AddDays(-days).ToString("yyyy-MM-dd")));

			TableContinuationToken token = null;
			do
			{
				var incidentsQuery = await this.incidentTable.ExecuteQuerySegmentedAsync(query, token);
				token = incidentsQuery.ContinuationToken;

				// TODO: MOVE THIS INTO A FILTER
				var results = incidentsQuery.Results;
				results.RemoveAll(x => x.Deleted);

				return results;

			} while (token != null);
		}

		private async Task EnsureIncidentsTableExists()
		{
			await this.incidentTable.CreateIfNotExistsAsync();
		}
	}
}
