# DEPLOYMENT GUIDE - OrderItemsReserver Integration

## What Was Built

✅ **OrderItemsReserver Azure Function** - Uploads order JSON to Blob Storage
✅ **Integration with eShopOnWeb** - Calls function after order creation
✅ **Complete end-to-end flow** - Order → Reservation → Blob Storage

---

## QUICK START

### 1. Open Solution
```
File: C:\Users\RohithG\source\repos\Azure\AppService\eShopOnWeb.sln
Contains: OrderItemsReserver project in src folder
```

### 2. Local Testing (Optional)
```bash
# Start Azure Storage Emulator (Azurite)
azurite

# Start Azure Function locally
cd src/OrderItemsReserver
func start

# Run Web App
# In Visual Studio: Set Web as startup project → F5
```

### 3. Deploy to Azure

---

## AZURE DEPLOYMENT STEPS

### Step 1: Create Azure Resources

```bash
# Login
az login
az account set --subscription "YOUR_SUBSCRIPTION_ID"

# Create Resource Group
az group create --name eShopOnWeb-rg --location eastus

# Create Storage Account
az storage account create \
  --name eshoponwebstorage \
  --resource-group eShopOnWeb-rg \
  --location eastus \
  --sku Standard_LRS \
  --kind StorageV2

# Create Blob Container
az storage container create \
  --name order-requests \
  --account-name eshoponwebstorage

# Create Function App Storage
az storage account create \
  --name eshopadminfunction \
  --resource-group eShopOnWeb-rg \
  --location eastus \
  --sku Standard_LRS \
  --kind StorageV2

# Create Function App
az functionapp create \
  --resource-group eShopOnWeb-rg \
  --consumption-plan-location eastus \
  --name OrderItemsReserver \
  --storage-account eshopadminfunction \
  --runtime dotnet-isolated \
  --runtime-version 10.0 \
  --functions-version 4
```

### Step 2: Configure Function App

```bash
# Get storage connection string
STORAGE_CONN=$(az storage account show-connection-string \
  --name eshoponwebstorage \
  --resource-group eShopOnWeb-rg \
  --query connectionString -o tsv)

# Add app setting
az functionapp config appsettings set \
  --name OrderItemsReserver \
  --resource-group eShopOnWeb-rg \
  --settings BlobStorageConnectionString="$STORAGE_CONN"
```

### Step 3: Publish Azure Function

**In Visual Studio:**
1. Right-click `OrderItemsReserver` project
2. Select "Publish"
3. Choose "Azure" → "Azure Function App"
4. Select "OrderItemsReserver" function app
5. Click "Publish"

**Or using Azure CLI:**
```bash
cd src/OrderItemsReserver
func azure functionapp publish OrderItemsReserver
```

### Step 4: Get Function URL

```bash
# Get function URL with code key
az functionapp function show \
  --resource-group eShopOnWeb-rg \
  --name OrderItemsReserver \
  --function-name ReserveOrderItems \
  --query "invokeUrlTemplate"
```

Copy the URL. It will look like:
```
https://orderitemsreserver.azurewebsites.net/api/reserve-order?code=YOUR_CODE
```

### Step 5: Configure Web App

**In Azure Portal:**
1. Go to App Service → Create new app service
2. Or use existing app service

**Add Application Settings:**

In Azure Portal → App Service → Configuration → Application settings, add:

```
Key: OrderReserverFunctionUrl
Value: https://orderitemsreserver.azurewebsites.net/api/reserve-order?code=YOUR_CODE
```

### Step 6: Deploy Web App

**In Visual Studio:**
1. Right-click `Web` project
2. Select "Publish"
3. Choose "Azure" → "Azure App Service"
4. Select or create app service
5. Configure settings (add OrderReserverFunctionUrl)
6. Click "Publish"

---

## VERIFY DEPLOYMENT

### Test Order Creation

1. Access Web App: `https://your-app.azurewebsites.net`
2. Login/Register
3. Add items to basket
4. Proceed to checkout
5. Create order

### Check Blob Storage

```bash
# List files in blob container
az storage blob list \
  --container-name order-requests \
  --account-name eshoponwebstorage \
  --query "[].name" \
  -o table

# Download order file to verify
az storage blob download \
  --name "order-1-TIMESTAMP.json" \
  --container-name order-requests \
  --account-name eshoponwebstorage \
  --file downloaded-order.json
```

### Check Function Logs

1. Azure Portal → Function App "OrderItemsReserver"
2. Functions → ReserveOrderItems
3. Click "Monitor" to view logs

---

## EXPECTED JSON OUTPUT

File stored in Blob Storage: `order-1-20250320143000.json`

```json
{
  "orderId": 1,
  "buyerId": "user@example.com",
  "items": [
    {
      "itemId": 1,
      "itemName": "Product Name",
      "quantity": 2
    }
  ],
  "createdAt": "2025-03-20T14:30:00Z"
}
```

---

## TROUBLESHOOTING

### Order not creating JSON file

**Check:**
1. OrderReserverFunctionUrl configured in app settings
2. Function URL is correct with code key
3. Storage connection string correct
4. Check function logs in Azure Portal

**Fix:**
```bash
# Verify function URL is working
curl -X POST https://orderitemsreserver.azurewebsites.net/api/reserve-order?code=YOUR_CODE \
  -H "Content-Type: application/json" \
  -d '{"orderId":1,"buyerId":"test","items":[{"itemId":1,"itemName":"Test","quantity":1}],"createdAt":"2025-03-20T00:00:00Z"}'
```

### Function returns 400 Bad Request

Check JSON format:
- All required fields present
- Correct data types
- Items array not empty

### Storage connection issues

Verify connection string:
```bash
az storage account show-connection-string \
  --name eshoponwebstorage \
  --resource-group eShopOnWeb-rg
```

---

## PROJECT STRUCTURE

```
src/OrderItemsReserver/
├── OrderItemsReserver.csproj
├── Program.cs
├── host.json
├── local.settings.json
├── Models/
│   └── OrderRequest.cs
└── Functions/
    └── ReserveOrderItemsFunction.cs

Integration files:
├── src/ApplicationCore/Interfaces/IOrderReservationService.cs
├── src/Infrastructure/Services/OrderReservationService.cs
├── src/ApplicationCore/Services/OrderService.cs
├── src/Web/Configuration/ConfigureCoreServices.cs
├── src/PublicApi/Extensions/ServiceCollectionExtensions.cs
```

---

## CLEANUP - DELETE RESOURCES

When done, remove all Azure resources:

```bash
# Delete entire resource group (all resources)
az group delete --name eShopOnWeb-rg --yes --no-wait

# Or delete individually
az functionapp delete --resource-group eShopOnWeb-rg --name OrderItemsReserver --yes
az webapp delete --resource-group eShopOnWeb-rg --name your-app-name --yes
az storage account delete --resource-group eShopOnWeb-rg --name eshoponwebstorage --yes
az group delete --name eShopOnWeb-rg --yes
```

---

## KEY FILES TO REMEMBER

| File | Purpose |
|------|---------|
| `eShopOnWeb.sln` | Main solution (includes OrderItemsReserver) |
| `src/OrderItemsReserver/` | Azure Function project |
| `src/Infrastructure/Services/OrderReservationService.cs` | HTTP client for function |
| `Directory.Packages.props` | NuGet packages configuration |

---

## COMPLETION CHECKLIST

- [ ] Open eShopOnWeb.sln in Visual Studio
- [ ] See OrderItemsReserver project in Solution Explorer
- [ ] Build solution successfully
- [ ] Create Azure resources (Resource Group, Storage, Function App)
- [ ] Deploy OrderItemsReserver function
- [ ] Get function URL
- [ ] Configure Web App with OrderReserverFunctionUrl
- [ ] Deploy Web App
- [ ] Test order creation
- [ ] Verify JSON file appears in Blob Storage
- [ ] Check function logs
- [ ] Clean up Azure resources

---

## DONE ✅

Everything is ready for deployment. Follow the steps above and your OrderItemsReserver integration will be live!

