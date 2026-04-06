using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.Infrastructure.Services;

public class OrderReservationService : IOrderReservationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderReservationService> _logger;

    public OrderReservationService(HttpClient httpClient, IConfiguration configuration, ILogger<OrderReservationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOrderReservationAsync(int orderId, string buyerId, List<(int itemId, string itemName, int quantity)> items)
    {
        try
        {
            var orderReserverUrl = _configuration["OrderReserverFunctionUrl"];

            if (string.IsNullOrEmpty(orderReserverUrl))
            {
                _logger.LogWarning("OrderReserverFunctionUrl is not configured. Skipping order reservation.");
                return;
            }

            var orderRequest = new
            {
                OrderId = orderId,
                BuyerId = buyerId,
                Items = items.Select(i => new
                {
                    ItemId = i.itemId,
                    ItemName = i.itemName,
                    Quantity = i.quantity
                }).ToList(),
                CreatedAt = DateTime.UtcNow
            };

            var content = new StringContent(
                JsonSerializer.Serialize(orderRequest),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(orderReserverUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Order {orderId} successfully sent to reservation service.");
            }
            else
            {
                _logger.LogError($"Failed to send order {orderId} to reservation service. Status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending order reservation: {ex.Message}");
            // Don't throw - order creation should not fail if reservation fails
        }
    }
}

internal class OrderReservationRequest
{
    public int OrderId { get; set; }
    public string BuyerId { get; set; } = string.Empty;
    public List<OrderReservationItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

internal class OrderReservationItem
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
