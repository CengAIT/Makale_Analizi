using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Makale_Analizi
{
    public class ArticleDocument
    {
        public string? id { get; set; }
        public string? title { get; set; }
        public int year { get; set; }
        public List<string>? authors { get; set; }
        public List<string>? referenced_works { get; set; }
    }
}