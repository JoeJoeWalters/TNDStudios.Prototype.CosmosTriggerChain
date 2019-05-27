Feature: BlobWriterSpecs
	In order for a payload to be passed to the blob writer
	As an azure function
	I want to see the data from Cosmos being written to blob storage

@counttest
Scenario: When the documents are passed in, a given set of blob's are written
	Given A set of 300 processed documents exists
	When the documents are passed to the azure function
	Then 300 blob items should be written
