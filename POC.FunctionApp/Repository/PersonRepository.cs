namespace FunctionApp.Repository
{
    using System;
    using System.Threading.Tasks;
    using Cassandra;
    using FunctionApp.Dtos;

    public class PersonRepository
    {
        private readonly ISession session;
        private PreparedStatement insertStatement;

        public PersonRepository(ISession session)
        {
            this.session = session;
            CreatePreparedStatements();
        }
        
        private void CreatePreparedStatements()
        {
            string cql = "INSERT INTO person (id, email, first_name, last_name, last_update) VALUES (?,?,?,?,?);";
            this.insertStatement = session.Prepare(cql);
        }

        public Task Save(Person person)
        {
            var statement = this.insertStatement.Bind(person.Id, person.Email, person.FirstName, person.LastName, DateTimeOffset.Now);
            return this.session.ExecuteAsync(statement);
        }
    }
}