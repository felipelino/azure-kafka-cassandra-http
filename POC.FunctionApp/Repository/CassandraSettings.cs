﻿namespace FunctionApp.Repository
{
    using System;
    using Microsoft.Extensions.Configuration;

    public class CassandraSettings
    {
        public string ContactPoints { get; private set; }

        public int Port { get; private set; }

        public string UserName { get; private set; }

        public string Password { get; private set; }

        public string KeySpace { get; private set; }

        public CassandraSettings(IConfiguration configuration)
        {
            ContactPoints = configuration["CASSANDRA_HOST"];
            Port = Int32.Parse(configuration["CASSANDRA_PORT"]);
            UserName = configuration["CASSANDRA_USER"];
            Password = configuration["CASSANDRA_PASSWD"];
            KeySpace = configuration["CASSANDRA_KEYSPACE"];
        }
    }
}
