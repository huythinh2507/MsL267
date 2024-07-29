using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class Cell
    {
        public object Value { get; set; } = new object();
        public ColumnType ColumnType { get; set; }
        

    }
}
