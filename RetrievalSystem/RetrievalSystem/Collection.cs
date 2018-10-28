using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetrievalSystem
{
    class Collection
    {
        public string DocID { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Bibliographic { get; set; }

        public string Words { get; set; }

        public float Score { get; set; }
    }
}
