using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;

namespace ElasticSearchDataFactory
{
    public static class ElasticSearchResultProcessing
    {
        [FunctionName("ElasticSearchResultProcessing")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            try
            {
                string url = data.datasets[0].properties.linkedServiceName.properties.typeProperties.url;
                Uri FileSystemUrl = new Uri(url + data.datasets[0].properties.typeProperties.location.fileSystem);
                // Get the account name via the url
                string AccountName = url.Split("//")[1].Split('.')[0];
                string AccountKey = data.datasets[0].properties.linkedServiceName.properties.typeProperties.accountKey;
                Azure.Storage.StorageSharedKeyCredential SharedKey = new(AccountName, AccountKey);


                var DataLakeClient = DataLakeUtils.GetDataLakeFileSystemClient(FileSystemUrl, SharedKey);

                string filePath = data.datasets[0].properties.typeProperties.location.folderPath + "/" + data.datasets[0].properties.typeProperties.location.fileName;

                log.LogInformation("Current file: " + filePath);

                DataLakeFileClient fc = DataLakeUtils.CreateFileClient(DataLakeClient, filePath);

                dynamic result = await ProcessElasticOutputFile(fc);

                string log_result = JsonConvert.SerializeObject(result);
                log.LogInformation(log_result);

                return new OkObjectResult(result);
            } catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            
        }

        public static async Task<dynamic> ProcessElasticOutputFile(DataLakeFileClient fc)
        {
            using (StreamReader stream = new StreamReader(await fc.OpenReadAsync()))
            {
                dynamic ElasticResult = JsonConvert.DeserializeObject(await stream.ReadToEndAsync());

                string pit_id = ElasticResult.pit_id;
                JArray hits = ElasticResult.hits.hits;
                int number_of_hits = hits.Count;
                JArray sort = (JArray)hits[number_of_hits - 1]["sort"];

                return new
                {
                    pit_id,
                    number_of_hits,
                    sort
                };
            }
        }
    }
}
