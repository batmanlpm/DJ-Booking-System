const { app } = require('@azure/functions');
const { getContainer } = require('../shared/cosmosClient');

// GET all bookings
app.http('getBookings', {
    methods: ['GET'],
    authLevel: 'anonymous',
    route: 'bookings',
    handler: async (request, context) => {
        try {
            const container = getContainer(process.env.COSMOS_CONTAINER_BOOKINGS);
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

// POST create booking
app.http('createBooking', {
    methods: ['POST'],
    authLevel: 'anonymous',
    route: 'bookings',
    handler: async (request, context) => {
        try {
            const booking = await request.json();
            booking.id = Date.now().toString();
            booking.createdAt = new Date().toISOString();
            
            const container = getContainer(process.env.COSMOS_CONTAINER_BOOKINGS);
            const { resource } = await container.items.create(booking);
            
            return {
                status: 201,
                jsonBody: resource,
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

// DELETE booking
app.http('deleteBooking', {
    methods: ['DELETE'],
    authLevel: 'anonymous',
    route: 'bookings/{id}',
    handler: async (request, context) => {
        try {
            const id = request.params.id;
            const container = getContainer(process.env.COSMOS_CONTAINER_BOOKINGS);
            
            await container.item(id, id).delete();
            
            return {
                status: 204
            };
        } catch (error) {
            return {
                status: 500,
                jsonBody: { error: error.message }
            };
        }
    }
});
