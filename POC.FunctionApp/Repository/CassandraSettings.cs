﻿namespace FunctionApp.Repository
{
    public class CassandraSettings
    {
        public string ContactPoints { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string KeySpace { get; set; }
    }
}
