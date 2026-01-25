using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace FinanceMovilApp.Models
{
    [Preserve(AllMembers = true)]
    [Table("Mindset")]
    public class MindsetItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Content { get; set; }
        public string Icon { get; set; }
        public bool IsRead { get; set; }
        public bool IsLocked { get; set; }
    }
}
