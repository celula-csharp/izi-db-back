namespace IziDbBack.Domain.Entities
{
    public class DatabaseInstance
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public string? ConnectionString { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<UserInstance> UserInstances { get; set; } = new List<UserInstance>();
    }
}