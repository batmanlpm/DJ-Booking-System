# ?? FIX: Cannot Create Test Accounts - Partition Key Error

## ? ERROR
```
PartitionKey extracted from document doesn't match the one specified in the header
```

## ?? ROOT CAUSE
Your Cosmos DB `users` container has the **WRONG partition key**.

## ? SOLUTION

### Option 1: Recreate Container (RECOMMENDED)

1. **Delete** the existing `users` container in Azure Portal
2. **Create new** `users` container with:
   - **Partition key:** `/username` (lowercase, with forward slash)
   - **Throughput:** 400 RU/s

### Option 2: Use Migration Tool

Run the migration tool we created earlier:
```powershell
cd Tools
dotnet run --project MigrationTool.csproj
```

## ?? VERIFY PARTITION KEY

In Azure Portal:
1. Go to your Cosmos DB account
2. Open **Data Explorer**
3. Expand **DJBookingDB**
4. Click on **users** container
5. Click **Settings**
6. Check **Partition key** = `/username` (lowercase!)

If it shows `/Username` (capital U) ? **DELETE and recreate the container**

## ?? IMPORTANT
- Partition key is **case-sensitive**
- Must be `/username` not `/Username`
- Cannot be changed after container creation
- Must recreate container if wrong

## ?? AFTER FIX
Once fixed, you can create the 5 test accounts:
1. Log in as SysAdmin
2. Go to Users tab
3. Create accounts 1-5 with password `asdfgh`
4. Tutorial videos will play for those accounts

---

**Next Step:** Check your partition key in Azure Portal NOW!
