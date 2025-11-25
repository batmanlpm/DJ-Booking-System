# ?? Test Accounts & Tutorial Configuration

## ? COMPLETED CHANGES:

### 1. Tutorial Logic Updated
**File:** `MainWindow.xaml.cs`

Tutorial now **ONLY shows for usernames 1-5**:
```csharp
bool isTutorialTestAccount = _currentUser.Username == "1" || 
                             _currentUser.Username == "2" || 
                             _currentUser.Username == "3" || 
                             _currentUser.Username == "4" || 
                             _currentUser.Username == "5";
```

- ? Disabled for original sysadmin
- ? Disabled for all other existing users
- ? Enabled ONLY for test accounts 1-5

### 2. Test Accounts to Create

Use the Users panel in the app to create these accounts:

| Username | Password | Role | Is DJ | Is Venue Owner |
|----------|----------|------|-------|----------------|
| 1 | asdfgh | User | No | No |
| 2 | asdfgh | DJ | Yes | No |
| 3 | asdfgh | VenueOwner | No | Yes |
| 4 | asdfgh | Manager | No | No |
| 5 | asdfgh | SysAdmin | No | No |

**Full Name suggestions:**
- 1: Test User
- 2: Test DJ
- 3: Test VenueOwner  
- 4: Test Manager
- 5: Test SysAdmin

**Email suggestions:**
- user1@test.com
- dj2@test.com
- venue3@test.com
- manager4@test.com
- sysadmin5@test.com

### 3. Chat UI Changes

**ISSUE:** Cannot find "Integrated Web Discord" text in ChatView.xaml

The current ChatView only shows a single Discord WebView2 panel. There are no tabs visible for:
- "Integrated Web Discord"
- Webhook textbox

**ACTION NEEDED:** 
- Please point to the exact location/screenshot where "Integrated Web Discord" appears
- OR describe where the webhook textbox should be added in the current ChatView

---

## ?? NEXT STEPS:

1. **Create the 5 test accounts** manually using the Users panel
2. **Locate the Chat UI element** that needs renaming
3. **Add webhook textbox** once location is confirmed
4. **Test tutorial** with accounts 1-5

**Status:** Tutorial logic complete ? | Accounts pending manual creation | Chat UI location needed
