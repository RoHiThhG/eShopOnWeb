# 🔧 FIX: HTTP 500 Error on Deployed Web App

## Your Current Status
✅ Web App deployed to Azure  
✅ Function App deployed to Azure  
❌ Web App returning 500 error  

**Root Cause:** Missing Azure configuration in App Service settings

---

## 🚀 QUICK FIX (5 minutes)

### Step 1: Identify Your Resource Group

Your app is deployed at: `https://eshopweb-rg-ejc5d2bycpf6erdt.azurewebsites.net/`

1. Go to [Azure Portal](https://portal.azure.com)
2. Search for **Resource groups**
3. Find and click the resource group containing your deployed app
4. Note the name (e.g., `eshopweb-rg-***`)

---

### Step 2: Find Your SQL Servers and Get Connection Strings

Your app needs 2 SQL databases. Let's find them:

1. In the **Resource group**, look for resources of type "SQL server"
2. You should see 2 servers (typically named something like `sqlcatalog-***` and `sqlidentity-***`)

#### For the CATALOG database:

1. Click on the **first SQL server** (catalog)
2. In the left sidebar, click **Databases**
3. Click on the **catalog** database
4. Click **Connection strings** tab
5. Copy the **ADO.NET** connection string
6. Replace `{your_username}` with `sqladmin` and `{your_password}` with your SQL admin password
7. **Save this string** - you'll need it in Step 4

#### For the IDENTITY database:

1. Click on the **second SQL server** (identity)
2. In the left sidebar, click **Databases**
3. Click on the **identity** database
4. Click **Connection strings** tab
5. Copy the **ADO.NET** connection string
6. Replace `{your_username}` with `sqladmin` and `{your_password}` with your SQL admin password
7. **Save this string** - you'll need it in Step 4

---

### Step 3: Store Secrets in Key Vault

1. In your **Resource group**, find the **Key Vault** resource (typically named `kv-***`)
2. Click on it
3. In the left sidebar, click **Secrets** (under Objects)
4. Click **+ Generate/Import** button

#### Create Secret #1:

- **Name:** `AZURE-SQL-CATALOG-CONNECTION-STRING`
- **Value:** (Paste the CATALOG connection string from Step 2)
- **Activation date:** Leave blank
- **Expiration date:** Leave blank
- Click **Create**

#### Create Secret #2:

- **Name:** `AZURE-SQL-IDENTITY-CONNECTION-STRING`
- **Value:** (Paste the IDENTITY connection string from Step 2)
- **Activation date:** Leave blank
- **Expiration date:** Leave blank
- Click **Create**

---

### Step 4: Get Key Vault Endpoint

Still in your **Key Vault**:

1. Click **Overview** tab (top left)
2. Copy the **Vault URI** (looks like `https://kv-abc123xyz.vault.azure.net/`)
3. **Save this value** - you'll need it in Step 5

---

### Step 5: Configure App Service Settings

1. Go back to your **Resource group**
2. Find your **App Service** (the one deployed, looks like `eshopweb-rg-*` with type "App Service")
3. Click on it
4. In the left sidebar, click **Configuration** (under Settings)
5. Click the **Application settings** tab (should be active by default)

#### Add these settings:

Click **+ New application setting** for each:

| Name | Value | Notes |
|------|-------|-------|
| `AZURE_KEY_VAULT_ENDPOINT` | `https://kv-abc123xyz.vault.azure.net/` | Get this from Step 4 |
| `AZURE_SQL_CATALOG_CONNECTION_STRING_KEY` | `AZURE-SQL-CATALOG-CONNECTION-STRING` | Exact name from Step 3 |
| `AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY` | `AZURE-SQL-IDENTITY-CONNECTION-STRING` | Exact name from Step 3 |

**How to add each setting:**
1. Click **+ New application setting**
2. Enter the **Name** (from left column)
3. Enter the **Value** (from middle column)
4. Click **OK** button
5. Repeat for all 3 settings

6. After adding all 3, click the **Save** button at the top
7. A dialog will appear asking to confirm - click **Continue**
8. The app will restart automatically

---

### Step 6: Grant App Service Access to Key Vault

Your App Service needs permission to read secrets from Key Vault.

#### Using Azure Portal (Easiest):

1. Go back to your **Key Vault**
2. In the left sidebar, click **Access policies**
3. Click **+ Create** button at the top
4. **Select Permissions:**
   - Under "Secret permissions", check: ✓ Get, ✓ List
   - Click **Next**
5. **Select Principal:**
   - In the search box, type your **App Service name** (e.g., `eshopweb-rg-*`)
   - Click on your App Service in the results
   - Click **Next**
6. Click **Review + create** at the bottom
7. Click **Create**

#### Using Azure CLI (If you have it installed):

```bash
# Get your App Service's principal ID
$appId = az app show --resource-group <ResourceGroupName> --name <AppServiceName> --query identity.principalId -o tsv

# Grant access
az keyvault set-policy --name <KeyVaultName> --object-id $appId --secret-permissions get list
```

---

### Step 7: Verify Everything Works

1. Go to your app: `https://eshopweb-rg-*.azurewebsites.net/`
2. If you see the home page ✅ **IT WORKS!**
3. If you still get 500 error ❌ **See troubleshooting below**

---

## 🔍 Troubleshooting: Still Getting 500 Error?

### Check the Logs

#### Method 1: Real-time Log Stream

1. Go to your **App Service** in Azure Portal
2. In left sidebar, click **Log stream** (under Monitoring)
3. Keep this open and refresh your app in the browser
4. You'll see error messages in real-time

#### Method 2: Check Application Insights

1. Go to your **App Service**
2. In left sidebar, click **Application Insights** (under Monitoring)
3. Click on the Application Insights resource name (looks like `appi-*`)
4. Click **Failures** to see error details
5. Look for exceptions - they'll show what's wrong

### Common Error Messages & Fixes

#### Error: "AZURE_KEY_VAULT_ENDPOINT is not configured"

**Fix:** You didn't add this setting in Step 5. Go back and add it.

#### Error: "Could not connect to Key Vault"

**Fix:** Your App Service doesn't have permission. Complete Step 6.

#### Error: "Secret 'AZURE-SQL-CATALOG-CONNECTION-STRING' not found"

**Fix:** The secret name is wrong or doesn't exist in Key Vault. Check Step 3.

#### Error: "Cannot open database 'catalog'"

**Fix:** The SQL connection string is wrong. Check:
- SQL username is correct (usually `sqladmin`)
- SQL password is correct
- Database name is correct (`catalog` or `identity`)

---

## 📱 Using PowerShell (Advanced)

Save this as `fix-app-service.ps1`:

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$AppServiceName,
    
    [Parameter(Mandatory=$true)]
    [string]$KeyVaultName,
    
    [Parameter(Mandatory=$true)]
    [string]$CatalogConnectionString,
    
    [Parameter(Mandatory=$true)]
    [string]$IdentityConnectionString
)

Write-Host "Setting up App Service configuration..." -ForegroundColor Green

# Get Key Vault endpoint
$keyVaultEndpoint = az keyvault show --name $KeyVaultName --query properties.vaultUri -o tsv
Write-Host "Key Vault Endpoint: $keyVaultEndpoint" -ForegroundColor Cyan

# Create secrets
Write-Host "Creating secrets in Key Vault..." -ForegroundColor Green
az keyvault secret set --vault-name $KeyVaultName --name "AZURE-SQL-CATALOG-CONNECTION-STRING" --value $CatalogConnectionString --only-show-errors
az keyvault secret set --vault-name $KeyVaultName --name "AZURE-SQL-IDENTITY-CONNECTION-STRING" --value $IdentityConnectionString --only-show-errors

# Get App Service principal
Write-Host "Getting App Service principal ID..." -ForegroundColor Green
$principalId = az app show --resource-group $ResourceGroupName --name $AppServiceName --query identity.principalId -o tsv

if (-not $principalId) {
    Write-Host "Enabling Managed Identity..." -ForegroundColor Yellow
    az app identity assign --resource-group $ResourceGroupName --name $AppServiceName
    $principalId = az app show --resource-group $ResourceGroupName --name $AppServiceName --query identity.principalId -o tsv
}

Write-Host "Principal ID: $principalId" -ForegroundColor Cyan

# Grant access to Key Vault
Write-Host "Granting Key Vault access..." -ForegroundColor Green
az keyvault set-policy --name $KeyVaultName --object-id $principalId --secret-permissions get list

# Update App Service settings
Write-Host "Updating App Service settings..." -ForegroundColor Green
az webapp config appsettings set `
    --resource-group $ResourceGroupName `
    --name $AppServiceName `
    --settings `
        AZURE_KEY_VAULT_ENDPOINT=$keyVaultEndpoint `
        AZURE_SQL_CATALOG_CONNECTION_STRING_KEY="AZURE-SQL-CATALOG-CONNECTION-STRING" `
        AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY="AZURE-SQL-IDENTITY-CONNECTION-STRING"

Write-Host "Restarting App Service..." -ForegroundColor Green
az webapp restart --resource-group $ResourceGroupName --name $AppServiceName

Write-Host "✅ Configuration complete!" -ForegroundColor Green
Write-Host "Check your app at: https://$AppServiceName.azurewebsites.net" -ForegroundColor Cyan
```

**Run it with:**
```bash
./fix-app-service.ps1 -ResourceGroupName "eshopweb-rg-***" `
    -AppServiceName "eshopweb-rg-***" `
    -KeyVaultName "kv-***" `
    -CatalogConnectionString "Server=tcp:...connection string..." `
    -IdentityConnectionString "Server=tcp:...connection string..."
```

---

## ✅ Checklist

Before trying again, confirm:

- [ ] Added `AZURE_KEY_VAULT_ENDPOINT` to App Service settings
- [ ] Added `AZURE_SQL_CATALOG_CONNECTION_STRING_KEY` to App Service settings
- [ ] Added `AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY` to App Service settings
- [ ] Created `AZURE-SQL-CATALOG-CONNECTION-STRING` secret in Key Vault
- [ ] Created `AZURE-SQL-IDENTITY-CONNECTION-STRING` secret in Key Vault
- [ ] Granted App Service access to Key Vault
- [ ] App Service restarted after adding settings
- [ ] SQL connection strings include username and password

---

## 🎯 Next Steps After Fix

Once the app loads successfully:

1. **Test the app:** Create an account, add items to cart, checkout
2. **Verify Function Integration:** Orders should trigger the OrderItemsReserver function
3. **Check Blob Storage:** Orders should appear as JSON files in the container
4. **Monitor:** Set up Application Insights alerts for errors

---

## 💡 Tips

- If you see generic error page, enable detailed errors in Log Stream
- Connection strings are sensitive - don't share them or commit to Git
- Key Vault secrets are the most secure way to store connection strings
- App Service logs are your best friend - check them often!

---

**Still stuck?** Share the error message from Log Stream and I can help further!
