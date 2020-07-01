﻿namespace FunctionApp.Repository
{
    using System;
    using Cassandra;
    using Microsoft.Extensions.Logging;

    public class CassandraConnectionFactory
    {
        private readonly CassandraSettings settings;
        private readonly ILogger logger;

        public ISession Session { get; }
        
        public CassandraConnectionFactory(ILogger logger, CassandraSettings settings)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Session =
                Cluster.Builder()
                    .AddContactPoints(settings.ContactPoints)
                    .WithPort(settings.Port)
                    .WithCredentials(settings.UserName, settings.Password)
                    .WithDefaultKeyspace(settings.KeySpace)
                    .Build()
                    .Connect();
        }
    }
}
