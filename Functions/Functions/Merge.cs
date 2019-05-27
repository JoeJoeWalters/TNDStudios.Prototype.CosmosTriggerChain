using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TNDStudios.Prototype.CosmosTriggerChain
{
    public static class Merge
    {
        [FunctionName("Merge")]
        public static async Task Run(
            [CosmosDBTrigger(
                databaseName: "TimeStreamProcessing",
                collectionName: "RawLines",
                ConnectionStringSetting = "CosmosDBConnection",
                LeaseCollectionName = "RawLines_Merge_Leases",
                CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            [CosmosDB(
                databaseName: "TimeStreamProcessing",
                collectionName: "Processed",
                ConnectionStringSetting = "CosmosDBConnection",
                CreateIfNotExists = true)]
                IAsyncCollector<ProcessedObject> documentOutput, // Document writer binding for writing only
            [CosmosDB(databaseName: "TimeStreamProcessing",
                collectionName: "Processed",
                ConnectionStringSetting = "CosmosDBConnection")]
                DocumentClient client, // Document client binding for all document tasks
            ILogger log)/*,
            ExecutionContext context)*/
        {
            log.LogInformation("Documents modified " + input.Count);

            foreach (Document doc in input ?? new List<Document>())
            {
                RawLine rawLine = JsonConvert.DeserializeObject<RawLine>(doc.ToString());
                if (rawLine != null)
                {
                    IQueryable<RawLine> queryDocuments = client
                                .CreateDocumentQuery<RawLine>(UriFactory.CreateDocumentCollectionUri("TimeStreamProcessing", "RawLines"))
                                .Where(so => so.TimesheetId == rawLine.TimesheetId);

                    List<RawLine> foundLines = queryDocuments.ToList<RawLine>();
                    if (foundLines.Count > 0 && foundLines.Count == foundLines[0].TotalItems)
                    {
                        ProcessedObject processedObject = new ProcessedObject()
                        {
                            Id = foundLines[0].TimesheetId,
                            User = foundLines[0].User,
                            Lines = foundLines.Select(line => new ProcessedObjectLine() { Day = line.Day, RateCode = line.RateCode, Units = line.Units }).ToList()
                        };

                        await documentOutput.AddAsync(processedObject);
                    }
                }
            }
        }
    }
}
