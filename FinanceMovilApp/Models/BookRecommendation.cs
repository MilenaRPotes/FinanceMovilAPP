using SQLite;

namespace FinanceMovilApp.Models
{
    [Table("BookRecommendation")]
    public class BookRecommendation
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Description { get; set; }

        public string Category { get; set; } // "Finanzas" o "Mentalidad" / "Finances" or "Mindset"

        // Propiedades para el degradado / Gradient properties
        // 1. ALMACENAMIENTO: Guardamos los códigos como TEXTO (Esto evita el error) / STORAGE: We store the codes as TEXT (This avoids errors)
        public string HexStart { get; set; }
        public string HexEnd { get; set; }

        // 2. VISUALIZACIÓN: Propiedades "mágicas" que convierten el texto a Color para la UI / DISPLAY: "Magic" properties that convert text to Color for the UI
        public Color ColorStart => Color.FromArgb(HexStart);
        public Color ColorEnd => Color.FromArgb(HexEnd);
        public string Url { get; set; } // Link 
    }
}
