# ?? N8N COMPLETE AUTOMATION - DATABASE + AGENT

**Date:** 2025-01-23  
**Status:** ? **READY TO IMPLEMENT**

---

## ?? **WHAT THIS DOES:**

A complete **N8N workflow** that automates:
1. ? Reading venues/bookings from Cosmos DB
2. ? Updating ElevenLabs agent with real-time data
3. ? Creating bookings via voice commands
4. ? Syncing database with agent knowledge
5. ? Scheduled updates (hourly/daily)

---

## ?? **WORKFLOW 1: SYNC DATABASE TO AGENT (Scheduled)**

### **Purpose:**
Keep agent knowledge synchronized with database contents

### **Trigger:**
- **Type:** Schedule
- **Interval:** Every 1 hour (or on-demand via webhook)

### **Flow:**

```
[Schedule Trigger: Every 1 hour]
    ?
[HTTP Request: Get Venues from Cosmos DB API]
    ?
[Function: Format Venue List]
    ?
[Set Variable: venue_list = "test venue, Club XYZ, ..."]
    ?
[HTTP Request: Get Bookings Count]
    ?
[Function: Build Updated System Prompt]
    ?
[HTTP Request: Update ElevenLabs Agent]
    ?
[IF: Success?]
    ?? TRUE ? [Send Notification: "Agent synced with DB"]
    ?? FALSE ? [Send Alert: "Sync failed"]
```

---

## ?? **WORKFLOW 2: CREATE BOOKING VIA VOICE**

### **Purpose:**
Handle booking creation from voice commands

### **Trigger:**
- **Type:** Webhook
- **Method:** POST
- **Path:** `/create-booking`

### **Flow:**

```
[Webhook Trigger: POST /create-booking]
    ?
    Receives: {
        "venue": "test venue",
        "date": "2025-01-27",
        "time": "22:00",
        "dj": "Shane",
        "duration": 4
    }
    ?
[Validate Input]
    ?
[HTTP Request: Check Venue Exists in Cosmos DB]
    ?
[IF: Venue exists?]
    ?? TRUE ? Continue
    ?? FALSE ? Return error
    ?
[HTTP Request: Check Slot Available]
    ?
[IF: Slot free?]
    ?? TRUE ? Continue
    ?? FALSE ? Return "Slot taken"
    ?
[HTTP Request: Create Booking in Cosmos DB]
    ?
[IF: Booking created?]
    ?? TRUE ? [Update Agent Knowledge]
    ?? FALSE ? Return error
    ?
[Respond to Webhook: Success]
    {
        "success": true,
        "bookingId": "abc123",
        "message": "Booking created!"
    }
```

---

## ?? **WORKFLOW 3: QUERY DATABASE VIA AGENT**

### **Purpose:**
Agent can query real-time database data

### **Trigger:**
- **Type:** Webhook  
- **Method:** POST
- **Path:** `/query-venues` or `/query-bookings`

### **Flow:**

```
[Webhook Trigger: POST /query-venues]
    ?
[HTTP Request: Get All Venues from Cosmos DB]
    ?
[Function: Filter Active Venues]
    ?
[Function: Format Response]
    {
        "venues": [
            {"name": "test venue", "active": true},
            {"name": "Club XYZ", "active": true}
        ],
        "count": 2
    }
    ?
[Respond to Webhook]
```

---

## ?? **N8N NODE CONFIGURATIONS:**

### **Node 1: Get Cosmos DB Venues**

**Type:** HTTP Request  
**Method:** GET  
**URL:** 
```
https://{{cosmosEndpoint}}.documents.azure.com/dbs/{{dbName}}/colls/{{container}}/docs
```

**Headers:**
```json
{
  "Authorization": "{{cosmosKey}}",
  "x-ms-version": "2018-12-31",
  "x-ms-documentdb-isquery": "true",
  "Content-Type": "application/query+json"
}
```

**Body (Query):**
```json
{
  "query": "SELECT * FROM c WHERE c.type = 'venue' AND c.isActive = true",
  "parameters": []
}
```

---

### **Node 2: Format Venue List**

**Type:** Function  
**Code:**
```javascript
const venues = items[0].json.Documents || [];

const venueList = venues
  .map(v => v.name)
  .join(', ');

const venueCount = venues.length;

return {
  json: {
    venueList: venueList,
    venueCount: venueCount,
    venues: venues
  }
};
```

---

### **Node 3: Build Updated System Prompt**

**Type:** Function  
**Code:**
```javascript
// Base system prompt
const basePrompt = `=== CRITICAL: BOOKING AUTOMATION ===
When users say "make a booking":
- Say ONLY: "Creating your booking now!"
- STOP TALKING immediately

=== DATABASE KNOWLEDGE (UPDATED: ${new Date().toISOString()}) ===
AVAILABLE VENUES (${items[0].json.venueCount}):
${items[0].json.venueList}

You have FULL ACCESS to this live data.
When asked "What venues are available?", you can say:
"We currently have ${items[0].json.venueCount} active venues: ${items[0].json.venueList}"

=== YOUR CORE PERSONALITY ===
You are Candy-Bot, the AI assistant for The Fallen Collective DJ Booking System.
Keep responses SHORT (under 30 words).
Be friendly and helpful.

**You are always connected to the live database!** ??`;

return {
  json: {
    prompt: basePrompt
  }
};
```

---

### **Node 4: Update ElevenLabs Agent**

**Type:** HTTP Request  
**Method:** PATCH  
**URL:**
```
https://api.elevenlabs.io/v1/convai/agents/agent_2201kacf3j0nfjj9w151tbr3n5e0
```

**Headers:**
```json
{
  "xi-api-key": "sk_b04ee67b81c033d87b4ec481ccb20a5d63fa3b81c7d62a60",
  "Content-Type": "application/json"
}
```

**Body:**
```json
{
  "conversation_config": {
    "agent": {
      "prompt": {
        "prompt": "{{$json.prompt}}"
      }
    }
  }
}
```

---

### **Node 5: Create Booking in Cosmos DB**

**Type:** HTTP Request  
**Method:** POST  
**URL:**
```
https://{{cosmosEndpoint}}.documents.azure.com/dbs/{{dbName}}/colls/{{container}}/docs
```

**Headers:**
```json
{
  "Authorization": "{{cosmosKey}}",
  "x-ms-version": "2018-12-31",
  "x-ms-documentdb-partitionkey": "[\"{{$json.partitionKey}}\"]",
  "Content-Type": "application/json"
}
```

**Body:**
```json
{
  "id": "{{$json.bookingId}}",
  "type": "booking",
  "venueName": "{{$json.venue}}",
  "djName": "{{$json.dj}}",
  "date": "{{$json.date}}",
  "time": "{{$json.time}}",
  "duration": {{$json.duration}},
  "status": "confirmed",
  "createdAt": "{{$now}}",
  "createdBy": "voice-command"
}
```

---

## ?? **UPDATE C# TO USE N8N:**

### **Option 1: Use N8N for Everything**

Replace `CosmosDbService` calls with N8N webhooks:

```csharp
// Instead of:
var venues = await CosmosDb.GetAllVenuesAsync();

// Do this:
var n8nClient = new N8NWorkflowClient("http://localhost:5678/webhook");
var venues = await n8nClient.GetVenuesAsync();
```

### **Option 2: Hybrid Approach (Recommended)**

- Keep direct Cosmos DB access for fast reads
- Use N8N for writes and agent updates
- N8N handles sync in background

```csharp
// Direct read (fast)
var venues = await CosmosDb.GetAllVenuesAsync();

// Write via N8N (handles agent sync)
await n8nClient.CreateBookingAsync(bookingData);
```

---

## ?? **COSMOS DB CREDENTIALS FOR N8N:**

### **What you need:**

**1. Cosmos DB Endpoint:**
```
https://[your-account].documents.azure.com
```

**2. Cosmos DB Key:**
```
(Primary or Secondary key from Azure Portal)
```

**3. Database Name:**
```
fallen-collective-dj-booking
```

**4. Container Names:**
```
- venues
- bookings
- users
```

### **Where to find in code:**

Check `Services/CosmosDbService.cs` for:
```csharp
private const string EndpointUri = "https://...";
private const string PrimaryKey = "...";
```

Or check `App.config` / `appsettings.json`

---

## ?? **SETUP STEPS:**

### **Step 1: Install N8N**

**Docker (Easiest):**
```powershell
docker run -it --rm `
  --name n8n `
  -p 5678:5678 `
  -v n8n_data:/home/node/.n8n `
  n8nio/n8n
```

**Desktop App:**
```
Download: https://n8n.io/download
```

---

### **Step 2: Import Workflow JSON**

I'll create the complete workflow JSON you can import:

```json
{
  "name": "DJ Booking System - Database + Agent Sync",
  "nodes": [
    {
      "name": "Schedule: Sync Every Hour",
      "type": "n8n-nodes-base.scheduleTrigger",
      "position": [250, 300],
      "parameters": {
        "rule": {
          "interval": [
            {
              "field": "hours",
              "hoursInterval": 1
            }
          ]
        }
      }
    },
    {
      "name": "Get Cosmos DB Venues",
      "type": "n8n-nodes-base.httpRequest",
      "position": [450, 300],
      "parameters": {
        "url": "={{$env.COSMOS_ENDPOINT}}/dbs/{{$env.COSMOS_DB}}/colls/venues/docs",
        "method": "POST",
        "authentication": "genericCredentialType",
        "headers": {
          "parameters": [
            {
              "name": "Authorization",
              "value": "={{$env.COSMOS_KEY}}"
            },
            {
              "name": "x-ms-documentdb-isquery",
              "value": "true"
            }
          ]
        },
        "body": {
          "query": "SELECT * FROM c WHERE c.isActive = true"
        }
      }
    }
  ]
}
```

---

## ?? **ENVIRONMENT VARIABLES:**

Set these in N8N settings:

```env
# Cosmos DB
COSMOS_ENDPOINT=https://your-account.documents.azure.com
COSMOS_KEY=your_primary_key_here
COSMOS_DB=fallen-collective-dj-booking
COSMOS_VENUES_CONTAINER=venues
COSMOS_BOOKINGS_CONTAINER=bookings

# ElevenLabs
ELEVENLABS_API_KEY=sk_b04ee67b81c033d87b4ec481ccb20a5d63fa3b81c7d62a60
ELEVENLABS_AGENT_ID=agent_2201kacf3j0nfjj9w151tbr3n5e0
```

---

## ? **BENEFITS OF N8N AUTOMATION:**

### **1. Real-Time Agent Knowledge**
```
Agent always knows:
- Current venues
- Available slots
- Recent bookings
- System status
```

### **2. No Code Debugging**
```
- Visual workflow builder
- Test each node individually
- See data flow in real-time
- Easy to modify
```

### **3. Centralized Logic**
```
All automation in one place:
- Database operations
- Agent updates
- Error handling
- Notifications
```

### **4. Scalability**
```
Easy to add:
- Email notifications
- Discord alerts
- Slack integration
- SMS confirmations
- Analytics tracking
```

---

## ?? **NEXT STEPS:**

### **Quick Start:**

1. **Install N8N** (Docker recommended)
2. **Import workflow** (I'll provide JSON)
3. **Set environment variables** (Cosmos + ElevenLabs)
4. **Test sync workflow** (manual trigger first)
5. **Enable schedule** (hourly auto-sync)
6. **Update C# app** (use N8N webhooks)

---

## ?? **ADVANCED FEATURES:**

### **What else N8N can do:**

**1. Multi-Database Sync**
```
N8N can sync between:
- Cosmos DB (primary)
- SQL backup
- Redis cache
- ElasticSearch for analytics
```

**2. AI Agent Orchestration**
```
N8N ? ElevenLabs (voice)
     ? OpenAI (text generation)
     ? DALL-E (images)
     ? Claude (reasoning)
```

**3. Monitoring & Alerts**
```
N8N ? Discord webhook (booking created)
     ? Email (daily summary)
     ? SMS (urgent issues)
     ? Slack (team notifications)
```

**4. Analytics Pipeline**
```
N8N ? Read bookings
     ? Calculate stats
     ? Send to PowerBI
     ? Update dashboard
```

---

## ?? **COMPLETE WORKFLOW EXAMPLES:**

I'll create 3 ready-to-import workflows:

1. **`workflow-1-sync-db-to-agent.json`** - Scheduled sync
2. **`workflow-2-create-booking.json`** - Voice booking handler
3. **`workflow-3-query-handler.json`** - Database query endpoint

---

## ? **SUMMARY:**

**N8N can automate:**
- ? Database reads/writes
- ? Agent knowledge updates
- ? Booking creation
- ? Venue queries
- ? Scheduled syncs
- ? Error handling
- ? Notifications
- ? Analytics

**Advantages:**
- ?? Visual workflow builder
- ?? Auto-retry on failures
- ?? Built-in logging
- ?? Secure credential storage
- ?? Easy to scale
- ?? Free and open-source

---

**?? YES, N8N CAN ABSOLUTELY AUTOMATE DATABASE + AGENT TOGETHER!** ?

**Want me to create the complete workflow JSON files?** ??
