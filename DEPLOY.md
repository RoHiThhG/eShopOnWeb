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

#### 1.1 Create Resource Group

1. Go to [Azure Portal](https://portal.azure.com)
2. Click **Resource groups** in left sidebar
3. Click **Create**
4. Fill in:
   - **Subscription:** Select your subscription
   - **Resource group name:** `eShopOnWeb-rg`
   - **Region:** East US
5. Click **Review + create** → **Create**

#### 1.2 Create Storage Account for Blob Storage

1. In Azure Portal, search for **Storage accounts**
2. Click **Create**
3. Fill in:
   - **Subscription:** Your subscription
   - **Resource group:** `eShopOnWeb-rg`
   - **Storage account name:** `eshoponwebstorage` (must be unique)
   - **Region:** East US
   - **Performance:** Standard
   - **Redundancy:** Locally-redundant storage (LRS)
4. Click **Review + create** → **Create**

#### 1.3 Create Blob Container

1. Open the storage account `eshoponwebstorage`
2. In left sidebar, go to **Containers** (under Data storage)
3. Click **+ Container**
4. **Name:** `order-requests`
5. **Public access level:** Private
6. Click **Create**

#### 1.4 Create Storage Account for Function App

1. Search for **Storage accounts** in Azure Portal
2. Click **Create**
3. Fill in:
   - **Subscription:** Your subscription
   - **Resource group:** `eShopOnWeb-rg`
   - **Storage account name:** `eshopadminfunction` (must be unique)
   - **Region:** East US
   - **Performance:** Standard
   - **Redundancy:** Locally-redundant storage (LRS)
4. Click **Review + create** → **Create**

#### 1.5 Create Function App

1. Search for **Function App** in Azure Portal
2. Click **Create**
3. Fill in:
   - **Subscription:** Your subscription
   - **Resource group:** `eShopOnWeb-rg`
   - **Function App name:** `OrderItemsReserver`
   - **Publish:** Code
   - **Runtime stack:** .NET
   - **Version:** 8 Isolated
   - **Region:** East US
   - **Storage account:** Select `eshopadminfunction`
4. Click **Review + create** → **Create**

### Step 2: Configure Function App

1. Open the **OrderItemsReserver** function app in Azure Portal
2. Go to **Configuration** (in left sidebar under Settings)
3. Click **+ New application setting**
4. Add:
   - **Name:** `BlobStorageConnectionString`
   - **Value:** Get connection string from `eshoponwebstorage` storage account:
     - Go to Storage account `eshoponwebstorage` 
     - Click **Access keys** in left sidebar
     - Copy the **Connection string** from key1
   - Paste it as the value
5. Click **OK**
6. Click **Save** at the top

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

1. In Azure Portal, open **OrderItemsReserver** function app
2. Go to **Functions** (in left sidebar)
3. Click **ReserveOrderItems** function
4. Click **Get Function URL** at the top right
5. Select **default (Function key)**
6. Click **Copy**

The URL will look like:
```
https://orderitemsreserver.azurewebsites.net/api/reserve-order?code=YOUR_CODE
```

Keep this URL for Step 5.

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

1. Go to Storage account **eshoponwebstorage** in Azure Portal
2. Click **Containers** in left sidebar
3. Click on **order-requests** container
4. You should see files named like `order-1-TIMESTAMP.json`
5. Click a file to view/download it

### Check Function Logs

1. In Azure Portal, open **OrderItemsReserver** function app
2. Go to **Functions** → **ReserveOrderItems**
3. Click **Monitor** tab
4. View logs and traces for recent executions

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

**Check in Azure Portal:**

1. **OrderReserverFunctionUrl configured?**
   - App Service → Configuration → Check `OrderReserverFunctionUrl` exists

2. **Function URL is correct with code key?**
   - Function App → Functions → ReserveOrderItems → Get Function URL

3. **Storage connection string correct?**
   - Function App → Configuration → Check `BlobStorageConnectionString` value
   - Compare with Storage account → Access keys → Connection string

4. **Check function logs:**
   - Function App → Functions → ReserveOrderItems → Monitor tab
   - Look for error messages

### Function returns 400 Bad Request

Check JSON format in function code:
- All required fields present (orderId, buyerId, items, createdAt)
- Correct data types
- Items array not empty

### Storage connection issues

1. Go to Storage account **eshoponwebstorage**
2. Click **Access keys** in left sidebar
3. Copy the **Connection string** 
4. Verify it matches the value in Function App → Configuration → `BlobStorageConnectionString`

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

When done, remove all Azure resources from Azure Portal:

### Delete Everything at Once (Recommended)

1. Go to **Resource groups** in Azure Portal
2. Search for **eShopOnWeb-rg**
3. Click on it
4. Click **Delete resource group** at the top
5. Type the resource group name to confirm: `eShopOnWeb-rg`
6. Click **Delete**

This will delete:
- ✓ Function App (OrderItemsReserver)
- ✓ Storage Accounts (eshoponwebstorage, eshopadminfunction)
- ✓ All other resources in the group

### Delete Individual Resources

If you want to delete resources one by one:

1. **Delete Function App:**
   - Search "Function App" in Portal
   - Click **OrderItemsReserver**
   - Click **Delete** at top → Confirm

2. **Delete Storage Accounts:**
   - Search "Storage accounts" in Portal
   - Click **eshoponwebstorage** → Delete → Confirm
   - Click **eshopadminfunction** → Delete → Confirm

3. **Delete Resource Group:**
   - Go to **Resource groups**
   - Click **eShopOnWeb-rg**
   - Click **Delete resource group** → Confirm

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

