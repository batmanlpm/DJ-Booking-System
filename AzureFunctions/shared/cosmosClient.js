const { CosmosClient } = require('@azure/cosmos');

const endpoint = process.env.COSMOS_ENDPOINT;
const key = process.env.COSMOS_KEY;
const databaseId = process.env.COSMOS_DATABASE;

const client = new CosmosClient({ endpoint, key });
const database = client.database(databaseId);

module.exports = {
    client,
    database,
    getContainer: (containerName) => database.container(containerName)
};
