using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using TNDStudios.Prototype.CosmosTriggerChain;
using Xunit;

namespace Tests.Specflow.BlobWriter
{
    [Binding]
    public class BlobWriterSpecsSteps
    {
        BlobWriterTests blobWriterTests;
        List<ProcessedObject> payloads;
        MockWriter writer;

        [BeforeScenario("counttest")]
        public void CountTestStartup()
        {
            blobWriterTests = new BlobWriterTests();
            payloads = new List<ProcessedObject>();
            writer = null;
        }

        [AfterScenario("counttest")]
        public void CountTestTearDown()
        {
            blobWriterTests = null;
            payloads = null;
            writer = null;
        }

        [Given(@"A set of (.*) processed documents exists")]
        public void GivenASetOfProcessedDocumentsExists(int payloadCount)
        {
            payloads = blobWriterTests.GeneratePayloads(payloadCount);
        }
        
        [When(@"the documents are passed to the azure function")]
        public void WhenTheDocumentsArePassedToTheAzureFunction()
        {
            writer = blobWriterTests.Execute(payloads);
        }
        
        [Then(@"(.*) blob items should be written")]
        public void ThenBlobItemsShouldBeWritten(int payloadCount)
        {
            Assert.Equal(payloadCount, writer.WrittenItems.Count);
        }
    }
}
