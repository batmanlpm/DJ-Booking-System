# FIX-DUPLICATE-USERS-CONTAINER.md

## ?? PROBLEM IDENTIFIED

You have **TWO** Users containers in Azure Cosmos DB:
1. `users` (lowercase) ? **App uses this one**
2. `Users` (capitalized) ? **Old/duplicate container**

## ? SOLUTION

### **Option 1: Delete the Capitalized Container** (RECOMMENDED)

Since the app uses lowercase `users`, delete the capitalized `Users` container:

1. In Azure Portal ? Cosmos DB ? Data Explorer
2. Right-click the **`Users`** container (capitalized)
3. Select "Delete Container"
4. Confirm deletion

**Why this works:**
- Removes confusion
- App only uses lowercase `users`
- All data is in the correct container

---

### **Option 2: Verify Which Container Has Data**

Before deleting, check which container has your users:

**In Azure Portal:**
1. Click `users` (lowercase) ? Items
2. Do you see users listed? (sysadmin, me, etc.)
3. Click `Users` (capitalized) ? Items  
4. Is this one empty?

**Expected:**
- `users` (lowercase) = Has data ?
- `Users` (capitalized) = Empty ?

---

## ?? HOW THIS HAPPENED

The app was changed from `"Users"` to `"users"` to fix partition key issues.

Old code created: `Users` (capitalized)  
New code creates: `users` (lowercase)

Both containers exist now, causing confusion.

---

## ?? QUICK FIX STEPS

1. **In Azure Portal** ? Open Data Explorer
2. **Expand** both containers to see their Items
3. **Verify** `users` (lowercase) has your data
4. **Verify** `Users` (capitalized) is empty
5. **Delete** the empty `Users` (capitalized) container
6. **Refresh** app - should now show users correctly

---

## ?? VERIFY IN APP

After deleting the duplicate container:

1. **Restart app**
2. **Login as sysadmin**
3. **Go to Users menu**
4. **You should see:**
   - sysadmin
   - me
   - Any other users you created

---

## ?? IF BOTH CONTAINERS HAVE DATA

If BOTH containers have users:

1. **Don't delete anything yet**
2. **Export data from capitalized `Users`**
3. **Migrate to lowercase `users`** (if needed)
4. **Then delete capitalized `Users`**

Let me know if both have data and I'll create a migration script!

---

## ?? VERIFICATION QUERY

To check which container has data, in Azure Portal Query:

**For lowercase `users`:**
```sql
SELECT * FROM c
```

**For capitalized `Users`:**
```sql  
SELECT * FROM c
```

Compare the results.

---

**Recommendation:** Delete the empty capitalized `Users` container and keep using lowercase `users`.
