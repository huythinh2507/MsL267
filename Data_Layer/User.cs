namespace DataLayer
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public bool IsOwner { get; internal set; } = false;

    }
}