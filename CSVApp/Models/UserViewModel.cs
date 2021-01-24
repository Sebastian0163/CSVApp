using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSVApp.Models
{
    public class UserViewModel
    {
        public string Name { get; set; }
        public string Birthday { get; set; }
        public bool Married { get; set; }
        public string Phone { get; set; }
        public string Csv_file { get; set; }
        public string Salary { get; set; }
    }
}
