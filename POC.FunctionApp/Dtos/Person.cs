namespace FunctionApp.Dtos
{
    public class Person
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Email: {Email}";
        }
    }
}