#!/bin/sh

RESOURCE_GROUP="funcResourceGroup01"
LOCATION="westeurope"
APP_INSIGHTS_NAME="funcAppInsights01"
STORAGE_ACCOUNT="funcStorage"
FUNCTION_APP_NAME="funcsDemo01"

resourceGroupExists=`az group exists -n $RESOURCE_GROUP`
echo "resource group [$RESOURCE_GROUP] already exists? [$resourceGroupExists]"
if [ ! $resourceGroupExists ]
then
    echo "Creating resource group:[$RESOURCE_GROUP]"
    az group create -n $RESOURCE_GROUP -l $LOCATION
fi 


appInsightsShow=`az resource show -g $RESOURCE_GROUP -n $APP_INSIGHTS_NAME --resource-type "Microsoft.Insights/components"`
appInsightsExists=$?
if [ $appInsightsExists -eq 0 ]
then
    echo "app insights [$APP_INSIGHTS_NAME] already exists? [true]"
else
    echo "Creating app insights:[$APP_INSIGHTS_NAME]"
    az resource create -g $RESOURCE_GROUP -n $APP_INSIGHTS_NAME --resource-type "Microsoft.Insights/components" --properties "{\"Application_Type\":\"web\"}"
fi


storageAccountShow=`az storage account show --name $STORAGE_ACCOUNT --resource-group $RESOURCE_GROUP`
storageAccountExists=$?
if [ $storageAccountExists -eq 0 ]
then
    echo "storage account [$STORAGE_ACCOUNT] already exists? [true]"
else 
    echo "Creating storage account:[$STORAGE_ACCOUNT]"
    az storage account create -n $STORAGE_ACCOUNT -l $LOCATION -g $RESOURCE_GROUP --sku Standard_LRS
fi

functionAppShow=`az functionapp show --name $FUNCTION_APP_NAME --resource-group $RESOURCE_GROUP`
functionAppExists=$?
if [ $functionAppExists -eq 0 ]
then
    echo "function app [$FUNCTION_APP_NAME] already exists? [true]"
else 
    echo "Creating Function App:[$FUNCTION_APP_NAME]"
    az functionapp create -n $FUNCTION_APP_NAME --storage-account $STORAGE_ACCOUNT --consumption-plan-location $LOCATION --app-insights $APP_INSIGHTS_NAME --runtime dotnet -g $RESOURCE_GROUP --functions-version 2
fi

echo "Deploying Function App with package"
az functionapp deployment source config-zip -g $RESOURCE_GROUP -n $FUNCTION_APP_NAME --src /function/publish.zip

echo "Configuring appsettings"
az functionapp config appsettings set -n $FUNCTION_APP_NAME -g $RESOURCE_GROUP --settings "CASSANDRA_HOST=CASSANDRAHOST.northeurope.cloudapp.azure.com" "CASSANDRA_PORT=9042" "CASSANDRA_USER=cassandra" "CASSANDRA_PASSWD=password" "CASSANDRA_KEYSPACE=app" "KAFKA_BROKER=KAFKAHOST.northeurope.cloudapp.azure.com:9092" "KAFKA_TOPIC=person" "KAFKA_CONSUMER_GROUP=functionGroup01"