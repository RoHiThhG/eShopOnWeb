# Complete Step-by-Step Fix for HTTP 500 Error

## What Changed in Your Code

The code has been improved to handle missing configuration gracefully:

**Changes made:**
1. ✅ Better error logging for database seeding failures
2. ✅ Validation of required Azure configuration settings
3. ✅ Clear error messages for missing connection strings
4. ✅ Global exception logging middleware

---

## Required Azure Configuration

Your deployed Web App needs these to work:

### 1. Azure Key Vault
- **Resource Type:** Key Vault
- **Contains:** SQL connection strings as secrets
- **What to create:**
  - Secret: `AZURE-SQL-CATALOG-CONNECTION-STRING`
  - Secret: `AZURE-SQL-IDENTITY-CONNECTION-STRING`

### 2. Azure SQL Database (Catalog)
- **Resource Type:** SQL Server + Database
- **Database Name:** catalog

### 3. Azure SQL Database (Identity)
- **Resource Type:** SQL Server + Database  
- **Database Name:** identity

### 4. Azure App Service
- **Resource Type:** App Service
- **Must have these Application Settings:**
  - `AZURE_KEY_VAULT_ENDPOINT` = `https://your-keyvault.vault.azure.net/`
  - `AZURE_SQL_CATALOG_CONNECTION_STRING_KEY` = `AZURE-SQL-CATALOG-CONNECTION-STRING`
  - `AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY` = `AZURE-SQL-IDENTITY-CONNECTION-STRING`
- **Must have Managed Identity assigned**
- **Must have Key Vault access permissions**

---

## Why You're Getting 500 Error

The app starts successfully but fails when:

1. ❌ Configuration settings are missing from App Service
2. ❌ Cannot connect to Key Vault
3. ❌ Cannot read secrets from Key Vault
4. ❌ Cannot connect to SQL databases
5. ❌ Database connection strings are invalid

---

## Solution: Add Missing Configuration

### IN AZURE PORTAL:

#### Step 1: Go to Your App Service

1. Open [Azure Portal](https://portal.azure.com)
2. Click **App Services** (or search for it)
3. Click on your deployed app (e.g., `eshopweb-rg-ejc5d2bycpf6erdt`)

#### Step 2: Navigate to Configuration

In the left sidebar:
- Click **Settings**
- Click **Configuration**
- You'll see "Application settings" tab at the top

#### Step 3: Add Application Settings

For each setting below, click **+ New application setting** and enter:

**Setting 1:**
```
Name: AZURE_KEY_VAULT_ENDPOINT
Value: https://kv-XXXXX.vault.azure.net/
```

**Setting 2:**
```
Name: AZURE_SQL_CATALOG_CONNECTION_STRING_KEY
Value: AZURE-SQL-CATALOG-CONNECTION-STRING
```

**Setting 3:**
```
Name: AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY
Value: AZURE-SQL-IDENTITY-CONNECTION-STRING
```

Then click **Save** button → **Continue** when prompted

#### Step 4: Find Key Vault Endpoint

To get your Key Vault endpoint:

1. In Azure Portal, search for **Key vaults**
2. Click on your key vault (e.g., `kv-XXXXX`)
3. Click **Overview** tab
4. Copy the **Vault URI** (looks like `https://kv-XXXXX.vault.azure.net/`)
5. This is your `AZURE_KEY_VAULT_ENDPOINT` value

#### Step 5: Create Secrets in Key Vault

1. Still in your Key Vault, click **Secrets** in left sidebar
2. Click **+ Generate/Import**

**Create Secret 1:**
- Name: `AZURE-SQL-CATALOG-CONNECTION-STRING`
- Value: (Get from SQL Server → Connection Strings → ADO.NET, replace `{your_username}` and `{your_password}`)
- Click **Create**

**Create Secret 2:**
- Name: `AZURE-SQL-IDENTITY-CONNECTION-STRING`
- Value: (Get from SQL Server → Connection Strings → ADO.NET, replace `{your_username}` and `{your_password}`)
- Click **Create**

#### Step 6: Grant App Service Access to Key Vault

1. In your Key Vault, click **Access policies** (left sidebar)
2. Click **+ Create** button at top
3. Under "Secret permissions", select: **Get** and **List**
4. Click **Next**
5. Search for your App Service name and select it
6. Click **Next**
7. Click **Review + create** → **Create**

#### Step 7: Restart App Service

1. Go back to your App Service
2. Click **Restart** button at the top
3. Wait 30-60 seconds for it to restart
4. Try accessing your app again

---

## How to Get SQL Connection Strings

### For Catalog Database:

1. In Azure Portal, search for **SQL servers**
2. Find and click the `sqlcatalog-XXXXX` server
3. Click on the **catalog** database (listed below)
4. Click **Connection strings** tab
5. Copy the **ADO.NET** string
6. Replace `{your_username}` with `sqladmin`
7. Replace `{your_password}` with your SQL admin password
8. Store this in Key Vault as `AZURE-SQL-CATALOG-CONNECTION-STRING`

### For Identity Database:

Same process, but for the `sqlidentity-XXXXX` server and **identity** database

---

## Verify It Works

After completing all steps:

1. Open your app: `https://eshopweb-rg-*.azurewebsites.net/`
2. You should see the home page ✅

If still getting 500 error:
- Check **Log Stream** in App Service (left sidebar)
- Look for specific error messages
- Share the error message for further help

---

## Code Changes Made

### File: `src/Web/Program.cs`

**What changed:**
- Added try-catch around database seeding with better error logging
- Added global exception logging middleware
- Errors during startup are now logged instead of silently failing

### File: `src/Web/Extensions/ServiceCollectionExtensions.cs`

**What changed:**
- Added validation for required configuration settings
- Clear error messages if settings are missing
- Better error handling for Key Vault connection

**These changes help identify configuration issues faster instead of generic 500 errors.**

---

## Next: Deploy Updated Code

Now that the configuration fix is in place, you should redeploy the Web project:

### Using Visual Studio:

1. Right-click **Web** project
2. Click **Publish**
3. Select your Azure App Service
4. Click **Publish**

### Using Azure CLI:

```bash
cd src/Web
dotnet publish -c Release -o ./publish
az webapp deployment source config-zip --resource-group <RG-Name> --name <AppServiceName> --src-path ./publish.zip
```

---

## Deployment Checklist

- [ ] Configuration settings added to App Service
- [ ] Secrets created in Key Vault
- [ ] App Service has access to Key Vault
- [ ] SQL connection strings are correct
- [ ] App Service restarted
- [ ] Updated code deployed (with better error handling)
- [ ] App loads without 500 error

---

## Common Issues

| Problem | Solution |
|---------|----------|
| 500 error persists | Check Log Stream for specific error message |
| "Key Vault not found" | Verify AZURE_KEY_VAULT_ENDPOINT is correct |
| "Secret not found" | Verify secret names match exactly in Key Vault |
| "Cannot connect to database" | Verify SQL connection string and credentials |
| "Access denied" | Re-do Step 6 (grant Key Vault access) |

---

## Questions?

Share the error message from **Log Stream** (App Service → Log Stream) and I can help identify the exact issue!
