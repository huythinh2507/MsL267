namespace DataLayer
{
    public class Row
    {
        public List<Cell> Cells { get; set; } = new List<Cell>();
        
        public readonly Guid Id = Guid.NewGuid();
        public void UpdateCells(List<object> newValues)
        {
            if (newValues.Count != Cells.Count)
            {
                throw new ArgumentException("Number of values does not match the number of cells.");
            }

            for (int i = 0; i < Cells.Count; i++)
            {
                Cells[i].Value = (List<object>)newValues[i];
            }
        }
    }
}
