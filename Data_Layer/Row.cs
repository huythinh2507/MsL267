namespace DataLayer
{
    public class Row
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public List<Cell> Cells { get; set; } = new List<Cell>();
        public Guid ListId { get; internal set; }

        public void UpdateCells(List<string> newValues)
        {
            if (newValues.Count != Cells.Count)
            {
                throw new ArgumentException("Number of values does not match the number of cells.");
            }

            for (int i = 0; i < Cells.Count; i++)
            {
                Cells[i].Value = newValues[i];
            }
        }
    }
}
