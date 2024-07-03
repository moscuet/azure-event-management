using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventManagementApi.Entity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace EventManagementApi.Services
{
    public class CosmosDbService
    {
        private readonly Container _eventMetadataContainer;
        private readonly Container _userInteractionsContainer;
        private readonly Container _eventsContainer;

        public CosmosDbService(CosmosClient cosmosClient, IConfiguration configuration)
        {
            var databaseName = configuration["CosmosDb:DatabaseName"];
            _eventMetadataContainer = cosmosClient.GetContainer(databaseName, "EventMetadata");
            _userInteractionsContainer = cosmosClient.GetContainer(databaseName, "UserInteractions");
            _eventsContainer = cosmosClient.GetContainer(databaseName, "Events");
        }

        public async Task<IEnumerable<EventMetadata>> SearchEventsByMetadataAsync(string[] tags, string type, string category)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE ARRAY_CONTAINS(@tags, c.tags) AND c.type = @type AND c.category = @category")
                .WithParameter("@tags", tags)
                .WithParameter("@type", type)
                .WithParameter("@category", category);

            var iterator = _eventMetadataContainer.GetItemQueryIterator<EventMetadata>(query);
            var results = new List<EventMetadata>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<IEnumerable<Event>> GetMostViewedEventsAsync()
        {
            var query = new QueryDefinition("SELECT c.eventId, COUNT(c.id) as views FROM c WHERE c.interactionType = 'view' GROUP BY c.eventId ORDER BY views DESC");

            var iterator = _userInteractionsContainer.GetItemQueryIterator<UserInteraction>(query);
            var results = new List<UserInteraction>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            var eventIds = results.Select(r => r.EventId).Distinct();
            var events = new List<Event>();

            foreach (var eventId in eventIds)
            {
                var eventResponse = await _eventsContainer.ReadItemAsync<Event>(eventId, new PartitionKey(eventId));
                events.Add(eventResponse.Resource);
            }

            return events;
        }
    }

    public class EventMetadata
    {
        public string Id { get; set; }
        public string[] Tags { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
    }

    public class UserInteraction
    {
        public string Id { get; set; }
        public string EventId { get; set; }
        public string InteractionType { get; set; }
    }
}
