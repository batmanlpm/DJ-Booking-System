# ?? COSMOS DB PARTITION KEY ERROR - DIAGNOSIS & FIX

**Error:** `PartitionKey extracted from document doesn't match the one specified in the header`

---

## ?? **ROOT CAUSE:**

Your Azure Cosmos DB `Users` container has a **partition key path** that doesn't match what the code is sending.

---

## ? **SOLUTION: CHECK & FIX AZURE COSMOS DB**

### **Step 1: Check Container Partition Key in Azure Portal**

1. Go to: https://portal.azure.com
2. Navigate to: **Azure Cosmos DB** ? `fallen-collective`
3. Click: **Data Explorer**
4. Expand: `DJBookingDB`
5. Click on: **Users** container
6. Click: **Scale & Settings**
7. Look for: **Partition key**

**It should show:** `/username` (lowercase)

---

## ?? **IF IT SHOWS SOMETHING ELSE:**

The container was created with the wrong partition key. You have **2 options**:

### **Option A: Recreate Container** (RECOMMENDED if no data yet)

1. **Delete Users container** (loses all data!)
2. **Create new Users container** with:
   - Container ID: `Users`
   - Partition key: `/username` (lowercase!)
   - Throughput: 400 RU/s (or Manual)

### **Option B: Change Code to Match** (if container can't be changed)

If your container partition key is something else (like `/id` or `/Username`), we need to update the code.

**Tell me what the partition key path shows in Azure Portal.**

---

## ?? **CHECKING CODE vs AZURE:**

### **What Code Expects:**

```csharp
// In CosmosDbService.cs:
await _usersContainer.CreateItemAsync(user, new PartitionKey(user.Username));
```

Code sends: `user.Username` value as partition key

### **What JSON Sends:**

```json
{
  "id": "guid-here",
  "username": "testuser",  // ? Partition key value
  ...
}
```

JSON property: `"username"` (lowercase, thanks to `[JsonProperty("username")]`)

### **What Azure Expects:**

Container partition key path must be: `/username`

---

## ? **QUICK FIX STEPS:**

### **1. Verify Azure Container Settings:**

Run this to check your current container:

```bash
# If you have Azure CLI installed:
az cosmosdb sql container show \
  --account-name fallen-collective \
  --database-name DJBookingDB \
  --name Users \
  --resource-group <your-resource-group> \
  --query "resource.partitionKey"
```

### **2. If Partition Key is Wrong:**

**Delete and recreate container:**

```bash
# Delete old container (WARNING: Deletes all users!)
az cosmosdb sql container delete \
  --account-name fallen-collective \
  --database-name DJBookingDB \
  --name Users \
  --resource-group <your-resource-group> \
  --yes

# Create new container with correct partition key
az cosmosdb sql container create \
  --account-name fallen-collective \
  --database-name DJBookingDB \
  --name Users \
  --partition-key-path "/username" \
  --throughput 400 \
  --resource-group <your-resource-group>
```

---

## ?? **MANUAL FIX (Azure Portal):**

### **Step 1: Delete Old Container**
1. Azure Portal ? Cosmos DB ? `fallen-collective`
2. Data Explorer ? `DJBookingDB` ? **Users**
3. Right-click ? **Delete Container**
4. Type container name to confirm
5. Click **Delete**

### **Step 2: Create New Container**
1. Right-click `DJBookingDB` ? **New Container**
2. Fill in:
   - **Container ID:** `Users`
   - **Partition key:** `/username` (lowercase, with the slash!)
   - **Throughput:** `400` RU/s (Manual)
3. Click **OK**

---

## ?? **TEST AFTER FIX:**

1. Try registering a new user
2. Should work without partition key error
3. User document will be stored with `username` as partition key

---

## ?? **IMPORTANT:**

- Partition key path is **case-sensitive**: `/username` ? `/Username`
- Partition key path must include the slash: `/username`
- Once created, partition key **cannot be changed**
- Only way to change is to delete and recreate container

---

## ?? **WHAT TO DO NOW:**

1. **Check Azure Portal** - What is the current partition key?
2. **Tell me the value** - I'll confirm if it matches
3. **If wrong** - Delete and recreate container
4. **Test registration** - Should work after fix

---

**Let me know what the partition key shows in Azure Portal and I'll help fix it!** ??
