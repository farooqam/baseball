SET resourceGroupName="gamelogs"
SET name="baseballfm"
SET databaseName="baseball"
SET collectionName="gamelog"
SET key="uO4qhxp68Igt5kA13lNpMYsUIU3yjQBaqEIG94bDoaUhoTTvaFu0BSKdw9idLEIhwGVQRJvWkDdjn5n9Q4WLNg=="
SET url="https://baseballfm.documents.azure.com:443/"

CALL az cosmosdb database delete --db-name %databaseName% --key %key% --name %name% --resource-group-name %resourceGroupName% --url-connection %url%
CALL az cosmosdb database create --name %name% --db-name %databaseName% --resource-group %resourceGroupName%
CALL az cosmosdb collection create --collection-name %collectionName% --name %name% --db-name %databaseName% --resource-group %resourceGroupName%

	