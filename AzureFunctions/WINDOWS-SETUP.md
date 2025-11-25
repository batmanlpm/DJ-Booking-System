# Windows Setup Guide - The Fallen Collective

## Prerequisites

1. **Node.js** - Download from https://nodejs.org (LTS version)
2. **Azure Functions Core Tools** - Install with:
   ```
   npm install -g azure-functions-core-tools@4 --unsafe-perm true
   ```

## Quick Start

### Step 1: Install Dependencies
Double-click: **setup.bat**

### Step 2: Start API Server
Double-click: **start-api.bat**

The API will start at http://localhost:7071/api

### Step 3: Open Website
Open **index.html** in your browser (in the WebPage folder)

## Manual Commands (if .bat files don't work)

Open Command Prompt in the AzureFunctions folder:

```cmd
npm install
func start
```

## Testing

Once the API is running:
1. Open the website (index.html)
2. Try creating a booking
3. Check if bookings appear in the table
4. All features should work!

## Troubleshooting

**"func is not recognized"**
- Install Azure Functions Core Tools (see Prerequisites)

**"npm is not recognized"**
- Install Node.js (see Prerequisites)
- Restart Command Prompt after installation

**Port 7071 already in use**
- Close other instances of func.exe
- Or change port in host.json

## Deploy to Production

When ready to go live:
1. Deploy Azure Functions to Azure Portal
2. Update API URL in index.html (line 526)
3. Upload index.html to Neocities or your hosting

## Support

Questions? The Candy-Bot in the website can help! 🍬
