using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DataLayer
{
    public class Database : DbContext
    {
        public Database() : base()
        {
            Database.Migrate();
        }

        public DbSet<List> Lists { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<Row> Rows { get; set; }
        public DbSet<Cell> Cells { get; set; }
        public DbSet<ListTemplate> ListTemplates { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(MsLConstant.connectionString);
            }
        }

        public async Task SaveList(List list)
        {
            var newList = new List
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                Color = list.Color,
                Icon = list.Icon,
                IsFavorited = list.IsFavorited,
                PageSize = list.PageSize,
                CurrentPage = list.CurrentPage,
            };

            // Add the columns associated with the template
            foreach (var column in list.Columns)
            {
                var newCol = new Column
                {
                    Id = column.Id,
                    Name = column.Name,
                    TypeId = column.TypeId,
                    Description = column.Description,
                    IsHidden = column.IsHidden,
                    Width = column.Width,
                    Choices = column.Choices,
                    ListID = newList.Id,
                    DefaultValue = column.DefaultValue,
                    AtoZFilter = column.AtoZFilter,
                    ZtoAFilter = column.ZtoAFilter
                };

                Columns.Add(newCol);
            }

            foreach (var row in list.Rows)
            {
                var newRow = new Row
                {
                    Id = row.Id,
                    ListId = newList.Id,
                };

                foreach (var cell in row.Cells)
                {
                    _ = new Cell
                    {
                        Value = cell.Value,
                        RowID = row.Id,
                    };
                }

                Rows.Add(newRow);
            }

            Lists.Add(newList);

            await SaveChangesAsync();
        }

        public async Task<IEnumerable<List>> GetLists()
        {
            return await Lists.ToListAsync();
        }

        public async Task<IEnumerable<ListTemplate>> GetTemplates()
        {
            return await ListTemplates.ToListAsync();
        }

        public async Task<List> GetList(Guid id)
        {
            var list = await Lists.FindAsync(id);

            ArgumentNullException.ThrowIfNull(list);
            return list;
        }

        public async Task<ListTemplate> GetTemplate(Guid id)
        {
            var template = await ListTemplates.FindAsync(id);

            ArgumentNullException.ThrowIfNull(template);
            return template;
        }

        public async Task SaveListTemplate(ListTemplate template)
        {
            var newTemplate = new ListTemplate
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                Color = template.Color,
                Icon = template.Icon,
                PageSize = template.PageSize,
                CurrentPage = template.CurrentPage, 
            };

            foreach (var column in template.Columns)
            {
                var newCol = new Column
                {
                    Id = column.Id,
                    Name = column.Name,
                    TypeId = column.TypeId,
                    Description = column.Description,
                    IsHidden = column.IsHidden,
                    Width = column.Width,
                    Choices = column.Choices,
                    DefaultValue = column.DefaultValue,
                    AtoZFilter = column.AtoZFilter,
                    ZtoAFilter = column.ZtoAFilter
                    
                };
                newTemplate.Columns.Add(newCol);
            }

            ListTemplates.Add(newTemplate);
            await SaveChangesAsync();
        }

        public async Task<IEnumerable<ListTemplate>> GetListTemplates()
        {
            return await ListTemplates.Include(t => t.Columns).ToListAsync();
        }

        public async Task<ListTemplate> GetListTemplate(Guid id)
        {
            var template = await ListTemplates.Include(t => t.Columns).FirstOrDefaultAsync(t => t.Id == id);
            ArgumentNullException.ThrowIfNull(template);
            return template;
        }
    }

    public class ListTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;   
        public string Icon { get; set; } = string.Empty;
        public int PageSize { get; set; } = 2;
        public int CurrentPage { get; set; } = 1;
        public List<Column> Columns { get; set; } = new List<Column>();
    }
}
