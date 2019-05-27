using Microsoft.Azure.Documents;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TNDStudios.Prototype.CosmosTriggerChain.Functions
{
    public static class TopicWriter
    {
        [FunctionName("TopicWriter")]
        public static async Task Run(
            [CosmosDBTrigger(
                databaseName: "TimeStreamProcessing",
                collectionName: "Processed",
                ConnectionStringSetting = "CosmosDBConnection",
                LeaseCollectionName = "Processed_TopicWriter_Leases",
                CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            [ServiceBus(
                queueOrTopicName: "timesheets",
                Connection = "ServiceBusConnection")]IAsyncCollector<Message> serviceBus,
            ILogger log)
        {
            foreach (Document doc in input ?? new List<Document>())
            {
                ProcessedObject processedObject = JsonConvert.DeserializeObject<ProcessedObject>(doc.ToString());
                if (processedObject != null)
                {
                    Message message = new Message(
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(processedObject, Formatting.Indented))
                        );
                    message.UserProperties["CustomProperty"] = "example property";

                    await serviceBus.AddAsync(message);
                }
            }
        }
    }
}
