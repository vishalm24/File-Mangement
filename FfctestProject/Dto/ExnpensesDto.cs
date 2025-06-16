namespace ConsoleApp1.Dto
{
    class ExnpensesDto
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public int? ReportId { get; set; }
        public string ExistingPath { get; set; }
        public string FullPath { get; set; } = null!;
        public string NewPath { get; set; } = null!;
        public string NewRelativePath { get; set; } = null!;
        public DateTime CreateDate { get; set; }
    }
}