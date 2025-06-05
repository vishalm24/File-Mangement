using ConsoleApp1.Dto;
using FfctestProject.Models;
using System.Text;
using System.Text.RegularExpressions;
class Program
{
    static async Task Main(string[] args)
    {
        using var context = new FfctestContext();

        string path = AppContext.BaseDirectory;
        string basePath = path.Substring(0, path.Length - 1);

        var existData = new List<ExnpensesDto>();
        var notExistData = new List<ExpenseReportImage>();

        var expenseEntities = (from eri in context.ExpenseReportImages
                               join eah in context.ExpenseApprovalHistories
                                   on eri.ExpenseReportId equals eah.ExpenseReportId
                               join eda in context.ExpenseDefaultFinanceApprovers
                                   on eah.UserId equals eda.UserId
                               select new ExnpensesDto
                               {
                                   Id = eri.Id,
                                   CompanyId = eda.CompanyId,
                                   ReportId = eri.ExpenseReportId,
                                   ExistingPath = eri.ImagePath ?? "",
                                   FullPath = string.Empty,
                                   CreateDate = eri.CreateDate
                               }).ToList();

        foreach (var expense in expenseEntities)
        {
            var imagePath = expense.ExistingPath;
            var imageFilename = Path.GetFileName(imagePath);

            bool hasValidPath = !string.IsNullOrWhiteSpace(imagePath);


            string TrimedImagePath = Regex.Replace(imagePath, @"\d.*", "");
            string TrimedImagedate = Regex.Replace(imagePath, @"\D.*", "");

            string date = $"{expense.CreateDate.Year}-{expense.CreateDate.Month}-{expense.CreateDate.Day}";


            var dto = new ExnpensesDto
            {
                Id = expense.Id,
                CompanyId = expense.CompanyId,
                ExistingPath = imagePath ?? string.Empty,
                ReportId = expense.ReportId,
                FullPath = hasValidPath
                    ? Path.GetFullPath(Path.Combine(basePath, imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)))
                    : string.Empty,
                CreateDate = expense.CreateDate,
                NewPath = ($"{basePath}{TrimedImagePath}{expense.CompanyId}\\{expense.ReportId}\\{date}\\{imageFilename}").Replace('/', '\\'),
                NewRelativePath = ($"{TrimedImagePath}{expense.CompanyId}\\{expense.ReportId}\\{date}\\{imageFilename}").Replace('\\', '/')
            };

            if (hasValidPath && expense.CompanyId != null)
                existData.Add(dto);

        }

        Console.WriteLine("Entries with valid image paths:\n");
        foreach (var data in existData)
        {
            Console.WriteLine($"Id: {data.Id}");
            Console.WriteLine($"CompanyId: {data.CompanyId}");
            Console.WriteLine($"ReportId: {data.ReportId}");
            Console.WriteLine($"Date: {data.CreateDate:yyyy-MM-dd}");
            Console.WriteLine($"Relative path: {data.ExistingPath}");
            Console.WriteLine($"Absolute path: {data.FullPath}");
            Console.WriteLine($"NewPath : {data.NewPath}");
            Console.WriteLine($"NewRelativePath : {data.NewRelativePath}");

            Console.WriteLine();
        }

        var oldDatereportdata = context.ExpenseReportImages;

        foreach (var data in existData)
        {
            var FoundData = oldDatereportdata.FirstOrDefault(u => u.ExpenseReportId == data.ReportId);
            if (FoundData != null)
            {
                FoundData.ImagePath = data.NewRelativePath;
            }

        }

        List<int> reportIds = new List<int>();

        foreach (var a in existData)
        {
            reportIds.Add(a.ReportId);
        }
        ;

        var recordsToLog = context.ExpenseReportImages
            .Where(x => !reportIds.Contains(x.ExpenseReportId))
            .ToList();
        var sb = new StringBuilder();


        sb.AppendLine("Records of ExpenseReportImages which are not updated");
        sb.AppendLine("-------------------------------------------------------------------------------");


        foreach (var record in recordsToLog)
        {
            var fullPath = Path.GetFullPath(Path.Combine(basePath, record.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));
            sb.AppendLine($"RecordId: {record.Id}\t ExpenceReportId: {record.ExpenseReportId}\t ImagePath: {record.ImagePath}\tFullPath: {fullPath}\t CreateDate:  {record.CreateDate:yyyy-MM-dd}");
        }


        string logFilePath = Path.Combine(AppContext.BaseDirectory, "MissingReportIds.txt");
        File.WriteAllText(logFilePath, sb.ToString());

        await context.SaveChangesAsync();


        Console.WriteLine($"Logged {recordsToLog.Count} records to: {logFilePath}");

        Console.WriteLine("Entries with missing or empty image paths:\n");
        foreach (var data in notExistData)
        {
            Console.WriteLine($"Id: {data.Id}");
            Console.WriteLine($"ReportId: {data.ExpenseReportId}");
            Console.WriteLine($"Date: {data.CreateDate:yyyy-MM-dd}");
            Console.WriteLine($"OldRelative path: {data.ImagePath}");
            Console.WriteLine();
        }
    }
}
