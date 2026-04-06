using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;

namespace OrderItemsReserver.Functions;

public class ReserveOrderItemsFunction
{
    private readonly BlobServiceClient _blobServiceClient;

    public ReserveOrderItemsFunction(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    [Function("ReserveOrderItems")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "reserve-order")] HttpRequestData req)
    {
        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var orderRequest = JsonSerializer.Deserialize<OrderRequest>(requestBody);

            if (orderRequest == null || orderRequest.Items.Count == 0)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Invalid order request" });
                return badResponse;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient("order-requests");
            await containerClient.CreateIfNotExistsAsync();

            string fileName = $"order-{orderRequest.OrderId}-{DateTime.UtcNow:yyyyMMddHHmmss}.json";
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var memoryStream = new MemoryStream())
            {
                var json = JsonSerializer.Serialize(orderRequest, new JsonSerializerOptions { WriteIndented = true });
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Position = 0;
                await blobClient.UploadAsync(memoryStream, overwrite: true);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Order reserved successfully", fileName = fileName });
            return response;
        }
        catch (Exception ex)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "Internal server error: " + ex.Message });
            return errorResponse;
        }
    }
}
