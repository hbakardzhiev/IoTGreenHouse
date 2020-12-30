using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using Azure.Storage.Blobs;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using System.Reflection;
using IoTGreenHouseML.Model;
using IoTGreenHouse;
using Microsoft.Extensions.ML;
using System.Text;

namespace IoTGreenHouse
{
    public class Function1
    {
        public static readonly String CONNECTION_STRING = "DefaultEndpointsProtocol=https;AccountName=iotgreenhousedatastorage;AccountKey=b+C0eOJZ6+aKBj9kr8Dkxji7NZiE/Bn8ujS+w0wB/T5M22OCvK4OtAPvOrD3UyudMK4KSv/ZdQD+n0/ao4XVpw==;EndpointSuffix=core.windows.net";
        private static readonly String BLOB_PICTURES = "pictures";

        private readonly PredictionEnginePool<ModelInput, ModelOutput> _predictionEngine;

        public Function1(PredictionEnginePool<ModelInput, ModelOutput> predictionEngine)
        {
            this._predictionEngine = predictionEngine;
        }

        [FunctionName("DownloadPicture")]
        public static async Task<IActionResult> DownloadPicture(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var id = req.Query["id"];
            BlobClient blobClient = GetBlobTable(id);

            MemoryStream stream = await GetStream(blobClient);
            var type = await blobClient.DownloadAsync();

            return new FileContentResult(stream.ToArray(), type.Value.ContentType);
        }


        [FunctionName("UploadSensor")]
        public static async Task<IActionResult> SensorData(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var formData = req.Form;
            var id = formData["id"];
            var name = formData["name"];
            var temp = formData["temperature"];

            CloudTable table = GetSQLTable("plantData");

            await InsertTableEntity(table, id, name, temp);
            return new OkObjectResult("Successful save of enitity");
        }


        [FunctionName("UploadPicture")]
        public async Task<IActionResult> Picture(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var picture = req.Form.Files["picture"];
            var id = req.Form["id"];

            //BlobClient blob = GetPictureBlobById(id);

            //await blob.UploadAsync(picture.OpenReadStream(), true);

            var tempPath = Path.Combine(context.FunctionAppDirectory, picture.FileName);

            using (Stream filestream = new FileStream(tempPath, FileMode.Create))
            {
                picture.CopyTo(filestream);
            }

            var input = new ModelInput();
            input.ImageSource = tempPath;
            var output = _predictionEngine.Predict(input);

            return new OkObjectResult("Successful upload of image with prediction " + output.Prediction);
        }


        [FunctionName("DownloadBest")]
        public async static Task<IActionResult> DownloadBest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            CloudStorageAccount accountClient = CloudStorageAccount.Parse(CONNECTION_STRING);
            CloudTableClient tableClient = accountClient.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("plantScore");

            TableQuery query1 = new TableQuery().OrderByDesc("score").Take(10);

            var result = table.ExecuteQuery(query1);

            List<ImageModel> models = new List<ImageModel>();

            foreach(ITableEntity el in result)
            {
                BlobClient blob = GetPictureBlobById(el.RowKey);
                MemoryStream stream = new MemoryStream();
                await blob.DownloadToAsync(stream);
                
                models.Add(new ImageModel()
                {
                    Name = el.PartitionKey,
                    Data = Convert.ToBase64String(stream.ToArray())
                });
            }

            return new OkObjectResult(models); ;
        }

        public static async Task<string> InsertTableEntity(CloudTable p_tbl, string id, string name, string temp)
        {
            PlantData plant = new PlantData(id, name, temp);
            TableOperation insertOperation = TableOperation.Insert(plant);
            TableResult result = await p_tbl.ExecuteAsync(insertOperation);
            return "Employee added";
        }
        private static BlobClient GetPictureBlobById(StringValues id)
        {
            BlobServiceClient client = new BlobServiceClient(CONNECTION_STRING);
            BlobContainerClient containerClient = client.GetBlobContainerClient(BLOB_PICTURES);
            BlobClient blob = containerClient.GetBlobClient(id);
            return blob;
        }
        public static CloudTable GetSQLTable(string tableReference)
        {
            CloudStorageAccount storageAcc = CloudStorageAccount.Parse(CONNECTION_STRING);
            CloudTableClient tblclient = storageAcc.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tblclient.GetTableReference(tableReference);
            return table;
        }
        public static async Task<MemoryStream> GetStream(BlobClient blobClient)
        {
            var stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream);
            return stream;
        }
        public static Stream GetStreamNonAsync(BlobClient blobClient)
        {
            var response = blobClient.Download();
            return response.Value.Content;
        }

        public static BlobClient GetBlobTable(StringValues id, string blobContainerName = "pictures")
        {
            BlobServiceClient client = new BlobServiceClient(CONNECTION_STRING);
            BlobContainerClient containerClient = client.GetBlobContainerClient(blobContainerName);
            BlobClient blobClient = containerClient.GetBlobClient(id);
            return blobClient;
        }
    }
}
