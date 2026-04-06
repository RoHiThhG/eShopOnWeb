using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        var blobConnectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString") 
            ?? "UseDevelopmentStorage=true";
        services.AddSingleton(new BlobServiceClient(blobConnectionString));
    })
    .Build();

host.Run();

