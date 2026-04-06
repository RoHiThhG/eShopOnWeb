# Azure App Service Configuration Guide - Fix 500 Error

## 🔴 Problem Diagnosis

Your Web App is getting **500 Internal Server Error** because the following are missing in Azure App Service:

1. ❌ Azure Key Vault connection
2. ❌ SQL Database connection strings
3. ❌ Proper Managed Identity permissions
4. ❌ Application settings

---

## ✅ STEP-BY-STEP FIX

### STEP 1: Verify your Web App in Azure Portal

1. Go to [Azure Portal](https://portal.azure.com)
2. Search for **App Services**
3. Click on your **eshopweb-rg-*** app service
4. Note the **Resource Group** name (e.g., `eshopweb-rg-***`)

---

### STEP 2: Check if Azure Resources Exist

1. **SQL Server** - Search "SQL servers" → You should see 2 servers:
   - One for Catalog DB
   - One for Identity DB
   
2. **Key Vault** - Search "Key vaults" → You should see 1 vault

3. **Storage Accounts** - Search "Storage accounts" → Check if blob storage exists

If these **don't exist**, you need to deploy infrastructure first using Bicep.

---

### STEP 3: Get Database Connection Strings

#### For Catalog Database:

1. Search "SQL servers" in Azure Portal
2. Click on **catalog-*** server
3. Click on **catalog** database in the left sidebar
4. Click **Connection strings** tab
5. Copy the **ADO.NET (SQL authentication)** string
6. Replace `{your_username}` and `{your_password}` with:
   - **Username:** `sqladmin` (or your configured username)
   - **Password:** (the password you set during creation)

**Example:**
```
Server=tcp:sqlcatalog-abc123.database.windows.net,1433;Initial Catalog=catalog;Persist Security Info=False;User ID=sqladmin;Password=YourPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

#### For Identity Database:

Repeat the same for **identity-*** server and **identity** database.

---

### STEP 4: Store Secrets in Key Vault

1. Search "Key vaults" in Azure Portal
2. Click on your key vault (e.g., `kv-***`)
3. Click **Secrets** in left sidebar
4. Click **+ Generate/Import**

Create 2 secrets:

**Secret 1: AZURE-SQL-CATALOG-CONNECTION-STRING**
- **Name:** `AZURE-SQL-CATALOG-CONNECTION-STRING`
- **Value:** (Paste the Catalog connection string from Step 3)
- Click **Create**

**Secret 2: AZURE-SQL-IDENTITY-CONNECTION-STRING**
- **Name:** `AZURE-SQL-IDENTITY-CONNECTION-STRING`
- **Value:** (Paste the Identity connection string from Step 3)
- Click **Create**

---

### STEP 5: Configure App Service Application Settings

1. Go to your App Service (e.g., `eshopweb-rg-ejc5d2bycpf6erdt`)
2. Click **Configuration** in left sidebar (under Settings)
3. You should see **Application settings** tab

#### Add/Update these settings:

| Name | Value |
|------|-------|
| `AZURE_KEY_VAULT_ENDPOINT` | `https://kv-***.vault.azure.net/` |
| `AZURE_SQL_CATALOG_CONNECTION_STRING_KEY` | `AZURE-SQL-CATALOG-CONNECTION-STRING` |
| `AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY` | `AZURE-SQL-IDENTITY-CONNECTION-STRING` |

**To find the Key Vault endpoint:**
1. Go to Key Vault
2. Click **Overview** tab
3. Copy the **Vault URI** (looks like `https://kv-abc123.vault.azure.net/`)

**Steps to add settings:**
1. Click **+ New application setting**
2. Enter **Name** and **Value**
3. Click **OK**
4. Click **Save** at the top
5. When prompted, click **Continue** to restart the app

---

### STEP 6: Configure Managed Identity Access

Your App Service needs permission to read Key Vault secrets.

#### Option A: Using Azure CLI (Recommended)

```bash
# Get App Service Principal ID
$appServicePrincipalId = az app show --resource-group <YourResourceGroup> --name <YourAppServiceName> --query identity.principalId -o tsv

# Grant Key Vault access
az keyvault set-policy --name <YourKeyVaultName> --object-id $appServicePrincipalId --secret-permissions get list

# Example:
az keyvault set-policy --name kv-abc123 --object-id 12345678-1234-1234-1234-123456789012 --secret-permissions get list
```

#### Option B: Using Azure Portal

1. Go to your **Key Vault**
2. Click **Access policies** in left sidebar
3. Click **+ Create** at the top
4. **Permissions:**
   - Secret permissions: Select **Get**, **List**
   - Click **Next**
5. **Principal:**
   - Search for your App Service name (e.g., `eshopweb-rg-***`)
   - Click your App Service
   - Click **Next**
6. Click **Review + create** → **Create**

---

### STEP 7: Restart App Service

1. Go to your **App Service**
2. Click **Restart** button at the top
3. Wait 30-60 seconds for the app to restart

---

### STEP 8: Test the App

1. Go to your app URL: `https://eshopweb-rg-*.azurewebsites.net/`
2. If you see the home page ✅ - **Configuration is fixed!**
3. If you still get 500 error ❌ - Check logs (see below)

---

## 🔍 TROUBLESHOOTING - Check Logs

### Enable Application Insights Logging

1. Go to your **App Service**
2. Click **Application Insights** in left sidebar
3. Click **Turn on Application Insights** (if not already on)
4. Select **Create new resource**
5. Click **Apply**

### View Error Logs

#### Method 1: Stream Logs (Real-time)

1. Go to **App Service**
2. Click **Log stream** in left sidebar
3. Refresh your app in browser
4. Watch the logs for error messages

#### Method 2: Check Application Insights

1. Go to **App Service** → **Application Insights**
2. Click the Application Insights resource name
3. Click **Failures** to see errors
4. Click **Performance** to see slow requests

#### Method 3: Check Platform Logs

1. Go to **App Service**
2. Click **Diagnostic settings** in left sidebar
3. Click **Add diagnostic setting**
4. Enable:
   - ✅ AppServiceHTTPLogs
   - ✅ AppServicePlatformLogs
5. Send to: **Log Analytics Workspace** or **Storage Account**
6. Click **Save**

---

## 🛠️ Common Issues & Fixes

### Issue 1: "Could not connect to Key Vault"

**Cause:** Managed Identity doesn't have permission

**Fix:**
```bash
# Re-apply access policy
az keyvault set-policy --name <KeyVaultName> \
  --object-id <AppServicePrincipalId> \
  --secret-permissions get list
```

### Issue 2: "Connection string is empty"

**Cause:** Secret not found in Key Vault

**Fix:**
1. Go to Key Vault → Secrets
2. Verify secret names match exactly:
   - `AZURE-SQL-CATALOG-CONNECTION-STRING`
   - `AZURE-SQL-IDENTITY-CONNECTION-STRING`
3. Verify values are not empty

### Issue 3: "Cannot connect to database"

**Cause:** Connection string is wrong or firewall blocked

**Fix:**
1. Go to **SQL Server** → **Networking** in left sidebar
2. Click **+ Add your client IPv4 address** (if needed)
3. Or add App Service by clicking **Allow Azure services and resources**
4. Click **Save**

### Issue 4: "Authorization failed for principal"

**Cause:** App Service Managed Identity not configured

**Fix:**
```bash
# Update App Service to use System Assigned Identity
az app identity assign --resource-group <RG> --name <AppName>
```

---

## 📋 CHECKLIST

Before accessing the app, verify:

- [ ] SQL Catalog database exists and is accessible
- [ ] SQL Identity database exists and is accessible
- [ ] Key Vault secrets created (`AZURE-SQL-CATALOG-CONNECTION-STRING`, `AZURE-SQL-IDENTITY-CONNECTION-STRING`)
- [ ] App Service application settings configured:
  - [ ] `AZURE_KEY_VAULT_ENDPOINT`
  - [ ] `AZURE_SQL_CATALOG_CONNECTION_STRING_KEY`
  - [ ] `AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY`
- [ ] App Service Managed Identity assigned
- [ ] Key Vault access policy allows App Service to read secrets
- [ ] SQL Server firewall allows Azure services
- [ ] App Service restarted

---

## 🚀 Quick PowerShell Script

Save this as `setup-app-service.ps1` and run:

```powershell
# Configuration
$resourceGroup = "eshopweb-rg-***"  # Change this
$appServiceName = "eshopweb-rg-***"  # Change this
$keyVaultName = "kv-***"  # Change this

# Get App Service Principal ID
Write-Host "Getting App Service Principal ID..." -ForegroundColor Green
$principalId = az app show --resource-group $resourceGroup --name $appServiceName --query identity.principalId -o tsv

if (-not $principalId) {
    Write-Host "Enabling Managed Identity..." -ForegroundColor Yellow
    az app identity assign --resource-group $resourceGroup --name $appServiceName
    $principalId = az app show --resource-group $resourceGroup --name $appServiceName --query identity.principalId -o tsv
}

Write-Host "Principal ID: $principalId" -ForegroundColor Green

# Grant Key Vault access
Write-Host "Granting Key Vault access..." -ForegroundColor Green
az keyvault set-policy --name $keyVaultName --object-id $principalId --secret-permissions get list

# Restart App Service
Write-Host "Restarting App Service..." -ForegroundColor Green
az webapp restart --resource-group $resourceGroup --name $appServiceName

Write-Host "Done! Check your app at https://$appServiceName.azurewebsites.net" -ForegroundColor Cyan
```

---

## ✅ SUCCESS

Once all steps are completed:

1. 🟢 App loads without 500 error
2. 🟢 Database connection works
3. 🟢 Authentication works
4. 🟢 Orders can be created
5. 🟢 Function integration works

**If still having issues, check logs using the "Troubleshooting" section above.**
