using System;
using TechTalk.SpecFlow;

namespace Tests.Specflow.BlobWriter
{
    [Binding]
    public class BlobWriterSpecsSteps
    {
        [BeforeScenario("Blob Writer Integration Test")]
        public void Startup()
        {

        }

        [AfterScenario("Blob Writer Integration Test")]
        public void TearDown()
        {

        }

        [Given(@"A set of (.*) processed documents exists")]
        public void GivenASetOfProcessedDocumentsExists(int payloadCount)
        {
        }
        
        [When(@"the documents are passed to the azure function")]
        public void WhenTheDocumentsArePassedToTheAzureFunction()
        {
        }
        
        [Then(@"(.*) blob items should be written")]
        public void ThenBlobItemsShouldBeWritten(int payloadCount)
        {
        }
    }
}
