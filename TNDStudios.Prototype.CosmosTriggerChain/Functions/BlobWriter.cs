using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace TNDStudios.Prototype.CosmosTriggerChain.Functions
{
    public static class BlobWriter
    {
        [FunctionName("BlobWriter")]
        public static async Task Run(
            [CosmosDBTrigger(
                databaseName: "TimeStreamProcessing",
                collectionName: "Processed",
                ConnectionStringSetting = "CosmosDBConnection",
                LeaseCollectionName = "leases",
                CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            Binder binder, // Manual binding so X blobs etc. can be bound when the binding outcome is not known
            [ServiceBus(
                queueOrTopicName: "timesheets", 
                Connection = "ServiceBusConnection")]IAsyncCollector<Message> serviceBus,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                foreach (Document doc in input)
                {
                    ProcessedObject processedObject = JsonConvert.DeserializeObject<ProcessedObject>(doc.ToString());
                    if (processedObject != null)
                    {
                        // Copy blob
                        string path = $"timesheets/{processedObject.Id}.txt";

                        var attributes = new Attribute[]
                        {
                            new BlobAttribute(path),
                            new StorageAccountAttribute("StorageConnection")
                        };

                        using (var writer = await binder.BindAsync<TextWriter>(attributes))
                        {
                            writer.Write(JsonConvert.SerializeObject(processedObject, Formatting.Indented));
                        }

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
}
