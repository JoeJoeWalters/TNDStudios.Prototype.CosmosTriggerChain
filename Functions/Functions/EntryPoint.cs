using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TNDStudios.Prototype.CosmosTriggerChain
{
    public static class EntryPoint
    {
        [FunctionName("EntryPoint")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "entrypoint")] HttpRequest req,
            [CosmosDB(
                databaseName: "TimeStreamProcessing",
                collectionName: "RawLines",
                ConnectionStringSetting = "CosmosDBConnection")]
                IAsyncCollector<RawLine> documentOutput, // Document writer binding for writing only
            [CosmosDB(
                databaseName: "TimeStreamProcessing",
                collectionName: "RawLines",
                ConnectionStringSetting = "CosmosDBConnection")]
                DocumentClient client, // Document client binding for all document tasks
            ILogger log)
        {
            RawLine data = null;

            log.LogInformation("Recieved request for a timesheet line to be processed.");

            try
            {
                 data = JsonConvert.DeserializeObject<RawLine>(
                     await new StreamReader(req.Body).ReadToEndAsync()
                     );
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            if (data != null)
            {
                if ((data.Id ?? String.Empty) != String.Empty &&
                    (data.TimesheetId ?? String.Empty) != String.Empty)
                {
                    IQueryable<RawLine> queryDocuments = client
                                    .CreateDocumentQuery<RawLine>(
                                        UriFactory.CreateDocumentCollectionUri("TimeStreamProcessing", "RawLines"), 
                                        new FeedOptions() { EnableCrossPartitionQuery = true })
                                    .Where(so => (so.Id == data.Id) || (so.TimesheetId == data.TimesheetId && so.SequenceNumber == data.SequenceNumber));

                    if (queryDocuments.ToList().Count == 0)
                    {
                        await documentOutput.AddAsync(data);
                        return new OkResult();
                    }
                    else
                        return new UnprocessableEntityObjectResult("Item has already been recieved.");
                }
                else
                    return new BadRequestObjectResult("No Id provided to identify the line uniquely.");
            }
            else
                return new BadRequestObjectResult("Error in format of request payload. Could not deserialise.");
        }
    }
}
