# azure-kafka-cassandra-http

A C#/.NET project to work as a POC (Proof of Concept) to understand how build Azure Functions to Consume/Publish messages from/to Kafka and persist into Cassandra

# Pre-requisites

## For Development

* [Microsoft Azure account](https://portal.azure.com/#home)
* [Gatling IO](https://gatling.io/) for load test
* [Gatling Kafka](https://github.com/mnogu/gatling-kafka) for load test with kafka
* [JetBrains Rider](https://www.jetbrains.com/rider/) or [Visual Studio](https://visualstudio.microsoft.com/pt-br/) for development and publish to Azure
* If use Rider you should use [Azure Toolkit](https://plugins.jetbrains.com/plugin/11220-azure-toolkit-for-rider)
* [Azure Core Tools](https://github.com/Azure/azure-functions-core-tools) to help to test locally

## Dependencies

* Cassandra running and accessible
* Kafka running and accessible

Search for tutorials to install or run the Kafka and Cassandra.

**_Suggestion_**:
* Use the Azure Virtual Machines
* Set a public IP with DNS
* Open the used ports
* **DON'T DO IT** in a production environment

Kafka can work with authentication SSL Plaintext or without authentication. We have Two functions listen to Kafka, one to work with authorization an another one without it.
The HTTP function will publish to Kafka broker with authentication if it is enable.

You can see the available configurations in the file `local.settings.json`

### Cassandra Model

```
CREATE KEYSPACE IF NOT EXISTS app 
WITH durable_writes = true 
AND replication = {'class':'SimpleStrategy', 'replication_factor' : 1};


CREATE TABLE IF NOT EXISTS app.person (
	id int,
	email varchar,
	first_name varchar,
	last_name varchar,
	last_update timestamp,
	PRIMARY KEY ((id), email)
) WITH COMPACTION = {
	'class': 'SizeTieredCompactionStrategy',
	'tombstone_threshold': '0.2',
	'unchecked_tombstone_compaction': 'true'
}
AND gc_grace_seconds = 86400;
```

### Kafka Topic

```
.\kafka-topics.bat --create --zookeeper HOST:2181 --topic person --partitions 10 --replication-factor 1
```

# Load Test Tool

How to prepare the load test tool

1. Download [Gatling IO](https://gatling.io/)
2. Unzip the Binary
3. Git clone the [Gatling Kafka](https://github.com/mnogu/gatling-kafka) 
4. Build gatling-kafka following the README instructions in the GitHub
5. Copy the JAR file generated to the `$GATLING_HOME/lib` folder
6. Copy the `performance-tests/user-files` to `$GATLING_HOME/user-files`
7. Replace the variables `URL_TO_YOUR_FUNCTION` and `HOST`
8. Adjust other parameters as you wish

## Pre-requisites

* Java [JRE 1.8](https://www.oracle.com/java/technologies/javase-jre8-downloads.html) or superior

# Call HTTP Function

```
curl -v -X POST URL_TO_YOUR_FUNCTION/HttpTriggerCheck -d '{"Id" : 1, "Email" : "john.doe@company.com", "FirstName": "John", "LastName" : "Doe"}'
```

Monitoring you can check:
* if the HTTP Function was triggered
* if the Kafka Function was triggered
* if the cassandra receive the expected data

# Run Load Tests

1. Go to the folder `$GATLING_HOME`
2. Execute: `./bin/gatling.bat` or `./bin/gatling.sh`
3. Select the desired test:
   * `HttpTriggerToKafka`: POST several JSONs to HTTP Function and this function will publish the message to Kafka Topic
   * `PublishToKafka`: This test, publish several messages direct into Kafka Topic

# Build / Publish to Azure Command-Line

## Build Local

The script below build and create a file `publish.zip` with the bundle to delivery to Portal Azure.

```
./build.ps1
```

## Run Azure CLI Docker

Before make the changes of the variables in the script deploy-azure.sh
```
# Azure Settings
RESOURCE_GROUP="funcResourceGroup01"
LOCATION="westeurope"
APP_INSIGHTS_NAME="funcAppInsights01"
STORAGE_ACCOUNT="funcStorage"
FUNCTION_APP_NAME="funcsDemo01"

# Function Settings
TIMEOUT_SECONDS="10"
CASSANDRA_HOST="host"
CASSANDRA_PORT="9042"
CASSANDRA_USER="cassandra"
CASSANDRA_PASSWD="password"
CASSANDRA_KEYSPACE="app"
KAFKA_BROKER="host:9092"
KAFKA_BROKER_AUTH="host_auth:9092"
KAFKA_TOPIC="person"
KAFKA_CONSUMER_GROUP="consumer01"
KAFKA_SSL_ENABLED="true"
KAFKA_USER="user"
KAFKA_PASSWORD="passwd"
```

Execute `pwd` to get `FULL_PATH`

```
docker run -v FULL_PATH:/function --name azure-cli -it mcr.microsoft.com/azure-cli
```

Inside the container
```
cd /function
az login
./deploy-azure.sh
```
