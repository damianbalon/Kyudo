using SQLite;
using System;

namespace KyudoApp.MAIU.Model
{
    [SQLite.Table("notatki")]
    public class Notatki
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_treningu")]
        public int? IdTreningu { get; set; }

        [Column("tresc")]
        public string Tresc {  get; set; }

        [Column("tytul")]
        public string Tytul { get; set; }

    }
}
