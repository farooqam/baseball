SET resourceGroupName="gamelogs"
SET name="baseballfm"
SET databaseName="baseball"
SET collectionName="gamelog"
SET key="key"
SET url="url"

CALL az cosmosdb database delete --db-name %databaseName% --key %key% --name %name% --resource-group-name %resourceGroupName% --url-connection %url%
CALL az cosmosdb database create --name %name% --db-name %databaseName% --resource-group %resourceGroupName%
CALL az cosmosdb collection create --collection-name %collectionName% --name %name% --db-name %databaseName% --resource-group %resourceGroupName%

	