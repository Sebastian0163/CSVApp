namespace CSVApp.Dal
{
    public class User
    {
        public int CsvId { get; set; }
        public string Name { get; set; }
        public string Birthday { get; set; }
        public bool Married { get; set; }
        public string Phone { get; set; }
        public string Csv_file { get; set; }
        public string Salary { get; set; }
        public bool IsActive { get; set; }
    }
}
