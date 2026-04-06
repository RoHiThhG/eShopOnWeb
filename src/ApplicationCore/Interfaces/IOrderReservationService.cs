using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

public interface IOrderReservationService
{
    Task SendOrderReservationAsync(int orderId, string buyerId, List<(int itemId, string itemName, int quantity)> items);
}
