using IoTGreenHouseML.Model;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: FunctionsStartup(typeof(IoTGreenHouse.Startup))]
namespace IoTGreenHouse
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var storageAccount = CloudStorageAccount.Parse(Function1.CONNECTION_STRING);

            var client = storageAccount.CreateCloudBlobClient();

            var container = client.GetContainerReference("model");

            var model = container.GetBlockBlobReference("MLModel.zip");

            var uri = model.Uri.AbsoluteUri;

            builder.Services.AddPredictionEnginePool<ModelInput, ModelOutput>()
                .FromUri(uri);
        }
    }
}
