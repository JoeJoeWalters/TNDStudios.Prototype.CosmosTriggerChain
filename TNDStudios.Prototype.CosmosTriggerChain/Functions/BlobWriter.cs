using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
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
            Binder binder,
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
                    }
                }

            }
        }
    }
}
