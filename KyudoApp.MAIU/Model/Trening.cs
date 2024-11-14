using SQLite;
using System;

namespace KyudoApp.MAIU.Model
{
    [SQLite.Table("trening")]
    public class Trening
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("data")]
        public DateTime Data { get; set; }

        [Column("miejsce")]
        public string Miejsce { get; set; }

        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan EndTime { get; set; }
    }
}
