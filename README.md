# Data Factory Elastic Search Copy Pipeline

This is a data factory pipeline to move data from an Elasticsearch index to an 
Azure Data Lake in a JSON format and then to further conform that data to parquet. 

## How to Deploy

You will need to have an Azure Data Factory, Azure Function App, Azure Data Lake Gen 2 (Storage) Account, and an Azure Key Vault
deployed to properly use this Template. 

[First, deploy the Function App via visual studio.](https://learn.microsoft.com/en-us/azure/azure-functions/functions-develop-vs?tabs=in-process#publish-to-azure)

[Add your Secrets to your Key Vault and give permission to the Managed Identity of the Data Factory.](https://learn.microsoft.com/en-us/azure/data-factory/how-to-use-azure-key-vault-secrets-pipeline-activities)
	
- Call your elasticsearch API Key Secret `ElasticSearchAPI`, make sure to include ApiKey in the string
- Call your Azure Function Function Key Secret `AzureFunctionKey`

Import the Pipelines into Azure Data Factory by going to the plus, hover Pipeline, Import from pipeline template and importing the .zip files

Fill in the paramaters to test the job

## Methodology 

This is built to work around the 10,000 item limit in an elasticsearch result. 

This is built to go extremely parallel based on running many pipelines at once while maintaining a consistent
structure of the data because of the Point In Time identifier. 

All secrets are stored in Key Vault. 

The Azure function is used to get the document data from our JSON result output 
as the ADF Lookup activity is limited to 4 MB.