namespace DataLayer
{
    public class View
    {
        public ViewType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}