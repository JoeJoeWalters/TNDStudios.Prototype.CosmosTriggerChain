using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using TNDStudios.Prototype.CosmosTriggerChain;
using Xunit;

namespace Tests.Specflow.BlobWriter
{
    [Binding]
    [Scope(Tag = "datatest")]
    public class WrittenDataSpecSteps
    {
        BlobWriterTests blobWriterTests;
        ProcessedObject payload;
        String json;
        MockWriter writer;

        [BeforeScenario("datatest")]
        public void WrittenDataStartup()
        {
            blobWriterTests = new BlobWriterTests();
            payload = null;
            writer = null;
            json = null;
        }

        [AfterScenario("datatest")]
        public void WrittenDataTearDown()
        {
            blobWriterTests = null;
            payload = null;
            writer = null;
            json = null;
        }

        [Given(@"A document is passed to the azure function")]
        public void ADocumentIsPassedToTheAzureFunction()
        {
            payload = blobWriterTests.GeneratePayloads(1)[0];
            json = JsonConvert.SerializeObject(payload);
            writer = blobWriterTests.Execute(new List<ProcessedObject>() { payload });
        }

        [Then(@"A blob with the same serialised content is written")]
        public void ABlobWithTheSameSerialisedContentIsWritten()
        {
            Assert.Equal(1, writer.WrittenItems.Count);
            Assert.Equal(json, writer.WrittenItems[0]);
        }
    }
}
