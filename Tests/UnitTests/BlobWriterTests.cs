using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TNDStudios.Prototype.CosmosTriggerChain;
using TNDStudios.Prototype.CosmosTriggerChain.Functions;
using Xunit;

namespace Tests
{
    public class BlobWriterTests
    {
        // How many payloads to send in the full integration test
        private const Int32 integration_test_payloads_to_send_count = 300;

        [Theory(DisplayName = "Full Integration Test")]
        [InlineData(null, integration_test_payloads_to_send_count)]
        public void integration_test(
            List<ProcessedObject> payloads = null, 
            Int32 payloadCount = integration_test_payloads_to_send_count)
        {
            // Arrange (if no theory data was supplied
            // so it acts as if it was a test not a theory)
            if (payloads == null || payloads.Count == 0)
                payloads = GeneratePayloads(payloadCount);

            // Act
            MockWriter writer = Execute(payloads);

            // Assert
            Assert.Equal(writer.WrittenItems.Count, payloads.Count);
        }

        /// <summary>
        /// Generate a set of test data
        /// </summary>
        /// <param name="payloadCount">How much test data</param>
        /// <returns>A list of test data</returns>
        public List<ProcessedObject> GeneratePayloads(Int32 payloadCount)
        {
            List<ProcessedObject> result = new List<ProcessedObject>();

            Int32 docId = 0;
            while (docId < payloadCount)
            {
                result.Add(new ProcessedObject()
                {
                    User = $"Joe {docId.ToString()}",
                    Id = Guid.NewGuid().ToString()
                }); // Create a new payload object
                docId++; // Next!
            }

            return result;
        }

        /// <summary>
        /// Execute based on a set of payloads rather than cosmos documents
        /// which are harder to set up for the testers
        /// </summary>
        /// <param name="payloads">The payloads to process</param>
        /// <returns>A mocked writer (for the blobs) to test</returns>
        public MockWriter Execute(List<ProcessedObject> payloads)
        {
            // Loop the test payloads to create a set of incoming 
            // Cosmos documents for the function
            List<Document> documents = new List<Document>();
            foreach (ProcessedObject payload in payloads)
            {
                Document document = new Document(); // New Empty document
                String json = JsonConvert.SerializeObject(payload, Formatting.Indented); // Cast the payload to a string to load
                document.LoadFrom(new JsonTextReader(new StringReader(json))); // Load the payload to the document
                documents.Add(document); // Load the document to the list for the function to recieve
            }

            // Execute the azure function based on the documents just created
            return Execute(documents);
        }

        /// <summary>
        /// Execute the azure function based on a set of incoming 
        /// Cosmos Documents as the actual input expects to be the 
        /// trigger
        /// </summary>
        /// <param name="documents">The list of changed documents</param>
        /// <returns>A mocked writer (for the blobs) to test</returns>
        public MockWriter Execute(List<Document> documents)
        {
            Binder binder = Substitute.For<Binder>(); // Soft mock the binder object by simply instantiating it
            MockWriter writer = new MockWriter(); // Create a hard mocked class so we can collect the results
            binder.BindAsync<TextWriter>(new Attribute[] { }).ReturnsForAnyArgs(writer); // Make sure the soft mocked binder returns the mocked writer

            ILogger logger = Substitute.For<ILogger>(); // Mock the logger to allow the logging to pass through

            // Act on the arrangement of test data
            BlobWriter.Run(documents, binder, logger);

            // Return the writer to assert the results
            return writer;
        }
    }

    /// <summary>
    /// Mocked up writer type to collect what was created by the function
    /// </summary>
    public class MockWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8; // Hardcode the encoding type (needed by the base)

        // Items that are written to by the mocked process and can be checked later
        // not done with NSubstitute for now as want to add more complex state based checks
        private List<String> writtenItems = new List<string>();
        public IReadOnlyList<String> WrittenItems { get => writtenItems; }

        // Override the collection of the writing of the documents
        public override void Write(string value) => writtenItems.Add(value);
    }
}
