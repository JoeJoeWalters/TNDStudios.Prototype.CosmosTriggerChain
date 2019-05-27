using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
                LeaseCollectionName = "Processed_BlobWriter_Leases",
                CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            Binder binder, // https://github.com/Azure/Azure-Functions/issues/162 : Even though docs sometimes say you can use an IAsyncCollector you can't with CloudBlobs
            ILogger log)
        {
            foreach (Document doc in input ?? new List<Document>())
            {
                ProcessedObject processedObject = JsonConvert.DeserializeObject<ProcessedObject>(doc.ToString());
                if (processedObject != null)
                {
                    // Copy blob
                    string path = $"timesheets/{processedObject.Id}.txt";

                    Attribute[] attributes = new Attribute[]
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
