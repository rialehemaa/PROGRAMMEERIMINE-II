using System;
using System.Collections.Generic;
using System.Text;

namespace Tooded_DB
{

    public class Kategooria
    {
        public int Id { get; set; }
        public string Kategooria_nimetus { get; set; }
        public string Kirjeldus { get; set; }

        public ICollection<Toode> Tooded { get; set; }
    }
}
