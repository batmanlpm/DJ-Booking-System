const { app } = require('@azure/functions');
const { getContainer } = require('../shared/cosmosClient');

// GET all venues
app.http('getVenues', {
    methods: ['GET'],
    authLevel: 'anonymous',
    route: 'venues',
    handler: async (request, context) => {
        try {
            const container = getContainer(process.env.COSMOS_CONTAINER_VENUES);
            const { resources } = await container.items.readAll().fetchAll();
            
            return {
                status: 200,
                jsonBody: resources,
                headers: { 'Content-Type': 'application/json' }
            };
        } catch (error) {
            return {
                status: 500,
                jsonBody: { error: error.message }
            };
        }
    }
});

// POST create venue
app.http('createVenue', {
    methods: ['POST'],
    authLevel: 'anonymous',
    route: 'venues',
    handler: async (request, context) => {
        try {
            const venue = await request.json();
            venue.id = Date.now().toString();
            venue.createdAt = new Date().toISOString();
            venue.isOpen = true;
            
            const container = getContainer(process.env.COSMOS_CONTAINER_VENUES);
            const { resource } = await container.items.create(venue);
            
            return {
                status: 201,
                jsonBody: resource
            };
        } catch (error) {
            return {
                status: 500,
                jsonBody: { error: error.message }
            };
        }
    }
});
