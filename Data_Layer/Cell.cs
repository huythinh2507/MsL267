namespace DataLayer
{
    public class Cell
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Value { get; set; } = string.Empty;
        public ColumnType ColumnType { get; set; }
        public Guid RowID { get; set; }
    }
}
