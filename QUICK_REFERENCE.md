# 🚀 QUICK VISUAL REFERENCE - Fix HTTP 500 Error

## The Problem
```
You access: https://eshopweb-rg-ejc5d2bycpf6erdt.azurewebsites.net/
You get:    HTTP 500 Internal Server Error
Reason:     Missing Azure configuration
```

## The Solution Flow

```
┌─────────────────────────────────────────────────────────────┐
│  AZURE APP SERVICE (Your Deployed Web App)                  │
│  eshopweb-rg-ejc5d2bycpf6erdt                               │
│                                                              │
│  Needs 3 Application Settings:                              │
│  ✓ AZURE_KEY_VAULT_ENDPOINT                                 │
│  ✓ AZURE_SQL_CATALOG_CONNECTION_STRING_KEY                  │
│  ✓ AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY                 │
└────────────────┬─────────────────────────────────────────────┘
                 │
                 │ (Connects to)
                 ▼
┌─────────────────────────────────────────────────────────────┐
│  AZURE KEY VAULT (kv-XXXXX)                                 │
│  Stores secrets securely                                    │
│                                                              │
│  Must contain 2 secrets:                                    │
│  ✓ AZURE-SQL-CATALOG-CONNECTION-STRING                      │
│  ✓ AZURE-SQL-IDENTITY-CONNECTION-STRING                     │
└────┬──────────────────────────────────────────────────────┬─┘
     │                                                      │
     │ (Provides)                                           │
     ▼                                                      ▼
┌──────────────────────────┐              ┌──────────────────────────┐
│ SQL Server (Catalog DB)  │              │ SQL Server (Identity DB) │
│ sqlcatalog-XXXXX         │              │ sqlidentity-XXXXX        │
│ Database: catalog        │              │ Database: identity       │
│ Tables: Products, etc.   │              │ Tables: Users, Roles     │
└──────────────────────────┘              └──────────────────────────┘
```

## Configuration Checklist (Azure Portal)

### 1️⃣ Find Your Key Vault Endpoint

```
Azure Portal
  ↓
Search "Key vaults"
  ↓
Click your key vault (kv-XXXXX)
  ↓
Click Overview
  ↓
Copy "Vault URI" → This is AZURE_KEY_VAULT_ENDPOINT
Example: https://kv-abc123xyz.vault.azure.net/
```

### 2️⃣ Create Secrets in Key Vault

```
Azure Portal → Your Key Vault
  ↓
Left sidebar → "Secrets"
  ↓
Click "+ Generate/Import"
  ↓
Name: AZURE-SQL-CATALOG-CONNECTION-STRING
Value: <Connection string from SQL Server>
  ↓
Create another for AZURE-SQL-IDENTITY-CONNECTION-STRING
```

### 3️⃣ Add Settings to App Service

```
Azure Portal → Your App Service
  ↓
Left sidebar → Settings → Configuration
  ↓
Click "+ New application setting"
  ↓
Add 3 settings:
  
  1. AZURE_KEY_VAULT_ENDPOINT = https://kv-XXXXX.vault.azure.net/
  2. AZURE_SQL_CATALOG_CONNECTION_STRING_KEY = AZURE-SQL-CATALOG-CONNECTION-STRING
  3. AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY = AZURE-SQL-IDENTITY-CONNECTION-STRING
  ↓
Click Save
```

### 4️⃣ Grant Key Vault Access

```
Azure Portal → Your Key Vault
  ↓
Left sidebar → "Access policies"
  ↓
Click "+ Create"
  ↓
Select Permissions: Get, List (for Secrets)
  ↓
Select Principal: Your App Service name
  ↓
Review + Create
```

### 5️⃣ Restart and Test

```
Azure Portal → Your App Service
  ↓
Click "Restart" button
  ↓
Wait 30-60 seconds
  ↓
Go to your app URL
  ↓
Should see home page ✅
```

---

## How to Get SQL Connection Strings

### For Catalog Database

```
Azure Portal
  ↓
Search "SQL servers"
  ↓
Click "sqlcatalog-XXXXX"
  ↓
Left sidebar → Databases
  ↓
Click "catalog" database
  ↓
Click "Connection strings" tab
  ↓
Copy ADO.NET string
  ↓
Replace {your_username} with: sqladmin
Replace {your_password} with: <your-sql-password>
  ↓
This is your AZURE-SQL-CATALOG-CONNECTION-STRING
```

### For Identity Database

Same process for "sqlidentity-XXXXX" server

---

## Error Diagnosis Flow

```
                    ┌─ Are you getting 500 error? ─┐
                    │                                 │
                  YES                               NO
                    │                                 │
                    ▼                                 ▼
           ┌─────────────────────┐         ✅ App is working!
           │ Check Log Stream    │         (Skip to testing)
           │ (App Service →      │
           │  Log stream)        │
           └────────┬────────────┘
                    │
         ┌──────────┴──────────┐
         │                     │
         ▼                     ▼
    "Key Vault not found"  "Connection string error"
         │                     │
         ▼                     ▼
    1. Check AZURE_KEY_   1. Verify connection
       VAULT_ENDPOINT        string in Key Vault
       setting exists     2. Check credentials
    2. Verify URL is         (username/password)
       correct           3. Test SQL connectivity

```

---

## Key Files in Your Repo

```
eShopOnWeb/
├── src/
│   ├── Web/
│   │   ├── Program.cs ✅ UPDATED (better error handling)
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs ✅ UPDATED (validation)
│   ├── OrderItemsReserver/
│   └── ... (other projects)
├── infra/
│   └── main.bicep (Optional: can auto-deploy everything)
├── FIX_DEPLOYED_APP_500_ERROR.md ← START HERE
├── CONFIGURATION_SETUP_GUIDE.md
├── AZURE_APP_SERVICE_CONFIGURATION.md
├── SETUP_SUMMARY.md
└── This file

```

---

## One-Line Status

| Component | Status | Action |
|-----------|--------|--------|
| Code | ✅ Fixed | Redeploy to Azure |
| Configuration | ❌ Required | Follow configuration guide |
| Database | ❓ Unknown | Verify via connection string |
| Function App | ✅ Deployed | Should work once app is up |

---

## Action Items (In Order)

- [ ] 1. Open `FIX_DEPLOYED_APP_500_ERROR.md` and follow Steps 1-7
- [ ] 2. Verify all settings added to App Service
- [ ] 3. Verify all secrets in Key Vault
- [ ] 4. Verify Key Vault access granted to App Service
- [ ] 5. Restart App Service
- [ ] 6. Test: Go to your app URL
- [ ] 7. If working: Continue with feature testing
- [ ] 8. If not working: Check Log Stream for specific error

---

## Testing Flow (After Fix)

```
App loads?
    ├─ NO  → Check Log Stream, share error
    └─ YES ↓
        
        Can you register?
            ├─ NO  → Database connection issue
            └─ YES ↓
            
                Can you login?
                    ├─ NO  → Authentication issue  
                    └─ YES ↓
                    
                        Can you create orders?
                            ├─ NO  → Function or database issue
                            └─ YES ↓
                            
                                Check Blob Storage for order files
                                ✅ SUCCESS! App is fully functional
```

---

## Estimated Time

- Configuration: **15-20 minutes**
- Redeploy code: **5 minutes**
- Testing: **10 minutes**
- **Total: ~30-35 minutes**

---

## Emergency Quick Fix

If you're in a hurry and want to get it working immediately:

```bash
# Using Azure CLI (requires installation)

# Set variables
$rg = "eshopweb-rg-***"
$app = "eshopweb-rg-***"
$kv = "kv-***"
$endpoint = "https://kv-*****.vault.azure.net/"

# Add settings to App Service
az webapp config appsettings set \
  -g $rg -n $app \
  --settings \
    AZURE_KEY_VAULT_ENDPOINT=$endpoint \
    AZURE_SQL_CATALOG_CONNECTION_STRING_KEY="AZURE-SQL-CATALOG-CONNECTION-STRING" \
    AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY="AZURE-SQL-IDENTITY-CONNECTION-STRING"

# Grant access
$principalId = az app show -g $rg -n $app --query identity.principalId -o tsv
az keyvault set-policy -n $kv --object-id $principalId --secret-permissions get list

# Restart
az webapp restart -g $rg -n $app

echo "✅ Configuration complete!"
```

---

## Still Having Issues?

```
Step 1: Get the exact error from Log Stream
Step 2: Share the error message (with stack trace if available)
Step 3: Include your configuration settings (don't share passwords!)
Step 4: I can identify the exact issue
```

**Common errors:**
- "AZURE_KEY_VAULT_ENDPOINT is not configured" → Add the setting
- "Secret not found" → Create the secret in Key Vault
- "Connection refused" → Check SQL firewall rules
- "Access denied" → Grant Key Vault permissions

---

## ✨ Final Checklist Before Testing

```
✓ AZURE_KEY_VAULT_ENDPOINT is set to correct URL
✓ AZURE_SQL_CATALOG_CONNECTION_STRING_KEY is set
✓ AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY is set
✓ AZURE-SQL-CATALOG-CONNECTION-STRING secret exists in Key Vault
✓ AZURE-SQL-IDENTITY-CONNECTION-STRING secret exists in Key Vault
✓ App Service has managed identity enabled
✓ App Service has Key Vault access permissions
✓ SQL connection strings have correct credentials
✓ App Service has been restarted
✓ Updated code has been deployed (optional but recommended)
```

All checked? → Go to your app URL → Should work! ✅

**Stuck on any step? Check the detailed guide: `FIX_DEPLOYED_APP_500_ERROR.md`**
