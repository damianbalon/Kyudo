using SQLite;
using System;

namespace KyudoApp.MAIU.Model
{
    [Table("wynik")]
    public class Wynik
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_treningu")]
        public int? IdTreningu { get; set; }  

        [Column("data")]
        public DateTime Data { get; set; } 

        [Column("x")]
        public float X { get; set; }  

        [Column("y")]
        public float Y { get; set; }

        [Ignore]
        public bool CzyTrafiony
        {
            get
            {
                // Definicja prostokąta, który reprezentuje mato
                float centerX = 150;
                float centerY = 225;

                // Promień koła, na podstawie średnicy 125 / 1.5
                float radius = 130f / 1.5f;

                // Obliczenie odległości od punktu (X, Y) do środka koła (centerX, centerY)
                float distance = (float)Math.Sqrt(Math.Pow(X - centerX, 2) + Math.Pow(Y - centerY, 2));

                // Jeśli odległość jest mniejsza lub równa promieniowi, punkt jest w kole
                return distance <= radius;
            }
        }

    }
}
