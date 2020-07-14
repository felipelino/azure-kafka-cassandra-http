#!/bin/sh
# Azure Settings
RESOURCE_GROUP="funcResourceGroup01"
LOCATION="westeurope"
APP_INSIGHTS_NAME="funcAppInsights01"
STORAGE_ACCOUNT="funcStorage"
FUNCTION_APP_NAME="funcsDemo01"

# Function Settings
CASSANDRA_HOST="host"
CASSANDRA_PORT="9042"
CASSANDRA_USER="cassandra"
CASSANDRA_PASSWD="password"
CASSANDRA_KEYSPACE="app"
KAFKA_BROKER="host:9092"
KAFKA_TOPIC="person"
KAFKA_CONSUMER_GROUP="consumer01"

resourceGroupExists=`az group exists -n $RESOURCE_GROUP`
echo "resource group [$RESOURCE_GROUP] already exists? [$resourceGroupExists]"
if [ $resourceGroupExists == "false" ]
then
    echo "Creating resource group:[$RESOURCE_GROUP]"
    az group create -n $RESOURCE_GROUP -l $LOCATION
    if [ $? -ne 0 ] 
    then
        return 1
    fi
fi 

appInsightsShow=`az resource show -g $RESOURCE_GROUP -n $APP_INSIGHTS_NAME --resource-type "Microsoft.Insights/components"`
appInsightsExists=$?
if [ $appInsightsExists -eq 0 ]
then
    echo "app insights [$APP_INSIGHTS_NAME] already exists? [true]"
else
    echo "Creating app insights:[$APP_INSIGHTS_NAME]"
    az resource create -g $RESOURCE_GROUP -n $APP_INSIGHTS_NAME --resource-type "Microsoft.Insights/components" --properties "{\"Application_Type\":\"web\"}"
    if [ $? -ne 0 ] 
    then
        return 1
    fi
fi


storageAccountShow=`az storage account show --name $STORAGE_ACCOUNT --resource-group $RESOURCE_GROUP`
storageAccountExists=$?
if [ $storageAccountExists -eq 0 ]
then
    echo "storage account [$STORAGE_ACCOUNT] already exists? [true]"
else 
    echo "Creating storage account:[$STORAGE_ACCOUNT]"
    az storage account create -n $STORAGE_ACCOUNT -l $LOCATION -g $RESOURCE_GROUP --sku Standard_LRS
    if [ $? -ne 0 ] 
    then
        return 1
    fi
fi

functionAppShow=`az functionapp show --name $FUNCTION_APP_NAME --resource-group $RESOURCE_GROUP`
functionAppExists=$?
if [ $functionAppExists -eq 0 ]
then
    echo "function app [$FUNCTION_APP_NAME] already exists? [true]"
else 
    echo "Creating Function App:[$FUNCTION_APP_NAME]"
    az functionapp create -n $FUNCTION_APP_NAME --storage-account $STORAGE_ACCOUNT --consumption-plan-location $LOCATION --app-insights $APP_INSIGHTS_NAME --runtime dotnet -g $RESOURCE_GROUP --functions-version 2
    if [ $? -ne 0 ] 
    then
        return 1
    fi
fi

echo "Deploying Function App with package"
az functionapp deployment source config-zip -g $RESOURCE_GROUP -n $FUNCTION_APP_NAME --src /function/publish.zip
if [ $? -ne 0 ] 
then
    return 1
fi

echo "Configuring appsettings"
az functionapp config appsettings set -n $FUNCTION_APP_NAME -g $RESOURCE_GROUP --settings "CASSANDRA_HOST=$CASSANDRA_HOST" "CASSANDRA_PORT=$CASSANDRA_PORT" "CASSANDRA_USER=$CASSANDRA_USER" "CASSANDRA_PASSWD=$CASSANDRA_PASSWD" "CASSANDRA_KEYSPACE=$CASSANDRA_KEYSPACE" "KAFKA_BROKER=$KAFKA_BROKER" "KAFKA_TOPIC=$KAFKA_TOPIC" "KAFKA_CONSUMER_GROUP=$KAFKA_CONSUMER_GROUP"
if [ $? -ne 0 ] 
then
    return 1
fi
