using ConsoleApp1.Dto;
using FfctestProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            var services = new ServiceCollection();

            services.AddDbContext<FfctestContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            var serviceProvider = services.BuildServiceProvider();

            using var context = serviceProvider.GetRequiredService<FfctestContext>();

            string path = AppContext.BaseDirectory;
            string basePath = path.Substring(0, path.Length - 1);

            var existData = new List<ExnpensesDto>();
            var notExistData = new List<ExpenseReportImage>();

            var sql = @"
                            SELECT
                                ISNULL(CAST(cti.Id AS INT), 0) AS Id,
                                cmr.ShopId AS CompanyId,
                                cti.LeadTransactionId AS ReportId,
                                cti.ImagePath AS ExistingPath,
                                ISNULL(cti.CreateDate, CAST('1900-01-01' AS DATETIME)) AS CreateDate
                            FROM CRMLeadTransactionImages cti
                            LEFT JOIN CRMLeadTransaction ct ON ct.Id = cti.LeadTransactionId
                            LEFT JOIN CompanyMemberRoleMapping cmr ON cmr.UserId = ct.UserId";

            var rawResults = context.Database.SqlQueryRaw<ExnpensesRawDto>(sql).ToList();

            var expenseEntities = rawResults.Select(x => new ExnpensesDto
            {
                Id = x.Id,
                CompanyId = x.CompanyId,
                ReportId = x.ReportId,
                ExistingPath = x.ExistingPath ?? "",
                FullPath = string.Empty,
                NewPath = string.Empty,
                NewRelativePath = string.Empty,
                CreateDate = x.CreateDate
            }).ToList();

            Console.WriteLine($"Total entries :{expenseEntities.Count}\n");

            var missingImagePath = new List<ExnpensesDto>();
            var missingCompanyId = new List<ExnpensesDto>();
            var imagePathNull = new List<ExnpensesDto>();
            

            foreach (var expense in expenseEntities)
            {
                var imagePath = expense.ExistingPath;
                var imageFilename = Path.GetFileName(imagePath);
                bool hasValidPath = !string.IsNullOrWhiteSpace(imagePath) || !string.IsNullOrEmpty(imagePath);
                string TrimedImagePath = Regex.Replace(imagePath, @"\d.*", "");
                string date = expense.CreateDate.ToString("yyyy-MM-dd");

                var dto = new ExnpensesDto
                {
                    Id = expense.Id,
                    CompanyId = expense.CompanyId,
                    ExistingPath = imagePath ?? string.Empty,
                    ReportId = expense.ReportId,
                    FullPath = hasValidPath
                        ? Path.GetFullPath(Path.Combine(basePath, imagePath.Substring("/API/".Length).Replace('/', Path.DirectorySeparatorChar)))
                        : string.Empty,
                    CreateDate = expense.CreateDate
                };

                if(dto.CompanyId == null)
                {
                    missingCompanyId.Add(dto);
                }
                else if (!hasValidPath)
                {
                    imagePathNull.Add(dto);
                }
                else if (!File.Exists(dto.FullPath))
                {
                    missingImagePath.Add(dto);
                }
            }

            var tableName = "CRMLeadTransactionImages";

            var pathDirectory = AppContext.BaseDirectory + $"\\Missing{tableName}";
            if (!Directory.Exists(pathDirectory))
            {
                Directory.CreateDirectory(pathDirectory);
            }

            Console.WriteLine($"Path Directory: {pathDirectory}\n");

            //When CompanyId is missing.
            var logPath = Path.Combine(pathDirectory, $"{tableName}CompanyIdData.txt");
            SavingFiles(missingCompanyId, $"{tableName}CompanyIdData.txt", logPath, "When CompanyId is missing.");

            //When ImagePath is missing.
            logPath = Path.Combine(pathDirectory, $"{tableName}MissingImagePathData.txt");
            SavingFiles(missingImagePath, $"{tableName}MissingImagePathData.txt", logPath, "When ImagePath is missing.");

            //When ImagePath is null or empty.
            logPath = Path.Combine(pathDirectory, $"{tableName}ImagePathNull.txt");
            SavingFiles(imagePathNull, $"{tableName}ImagePathNull.txt", logPath, "When ImagePath is null or empty.");

            //When Image exist in the directory. But not in the database.
            Console.WriteLine("Saving file... When Image exist in the directory. But not in the database.");
            var imageFiles = Directory.GetFiles(AppContext.BaseDirectory + "Images\\LRM", "*.*", SearchOption.AllDirectories);
            var apiPaths = imageFiles.Select(path => path.Replace(AppContext.BaseDirectory, "/API/").Replace('\\', '/')).ToList();

            var sb = new StringBuilder();
            logPath = Path.Combine(pathDirectory, $"{tableName}MissingImagePathInDB.txt");

            sb.AppendLine("When Image exist in the directory. But not in the database.\n");

            foreach (var item in apiPaths)
            {
                if(context.CRMLeadTransactionImages.Any(x => x.ImagePath == item))
                    continue;
                var fullPath= AppContext.BaseDirectory+item.Replace("/API/", "").Replace("/","\\");
                sb.AppendLine($"Full path : {fullPath}\t ReativePath :{item}");
            }
            Console.WriteLine($"File Path: {tableName}MissingImagePathData.txt");

            File.WriteAllText(logPath, sb.ToString());
            Console.ReadLine();



            //Console.WriteLine($"Total entries :{expenseEntities.Count}\n");

            //foreach (var expense in expenseEntities)
            //{
            //    var imagePath = expense.ExistingPath;
            //    var imageFilename = Path.GetFileName(imagePath);
            //    bool hasValidPath = !string.IsNullOrWhiteSpace(imagePath);
            //    string TrimedImagePath = Regex.Replace(imagePath, @"\d.*", "");
            //    string date = expense.CreateDate.ToString("yyyy-MM-dd");

            //    var dto = new ExnpensesDto
            //    {
            //        Id = expense.Id,
            //        CompanyId = expense.CompanyId,
            //        ExistingPath = imagePath ?? string.Empty,
            //        ReportId = expense.ReportId,
            //        FullPath = hasValidPath
            //            ? Path.GetFullPath(Path.Combine(basePath, imagePath.Substring("/API/".Length).Replace('/', Path.DirectorySeparatorChar)))
            //            : string.Empty,
            //        CreateDate = expense.CreateDate,
            //        NewPath = ($"{basePath}{TrimedImagePath.Replace("/API/", "/")}{expense.CompanyId}\\{expense.ReportId}\\{date}\\{imageFilename}").Replace('/', '\\'),
            //        NewRelativePath = ($"{TrimedImagePath}{expense.CompanyId}\\{expense.ReportId}\\{date}\\{imageFilename}").Replace('\\', '/')
            //    };

            //    if (hasValidPath && expense.CompanyId != null)
            //        existData.Add(dto);
            //}

            //var notExist = new List<ExnpensesDto>();

            //Console.WriteLine("Entries with valid image paths:\n");
            //foreach (var data in existData)
            //{
            //    if (File.Exists(data.FullPath))
            //    {
            //        string targetDirectory = Path.GetDirectoryName(data.NewPath);
            //        if (!Directory.Exists(targetDirectory))
            //        {
            //            Directory.CreateDirectory(targetDirectory);
            //            Console.WriteLine("Target Directory: " + targetDirectory);
            //        }
            //        File.Move(data.FullPath, data.NewPath, true);
            //        Console.WriteLine("File moved successfully.");
            //        Console.WriteLine($"Id: {data.Id}");
            //        Console.WriteLine($"CompanyId: {data.CompanyId}");
            //        Console.WriteLine($"ReportId: {data.ReportId}");
            //        Console.WriteLine($"Date: {data.CreateDate:yyyy-MM-dd}");
            //        Console.WriteLine($"Relative path: {data.ExistingPath}");
            //        Console.WriteLine("FullPath: " + data.FullPath);
            //        Console.WriteLine("NewPath: " + data.NewPath);
            //        Console.WriteLine($"NewRelativePath : {data.NewRelativePath}");
            //    }
            //    else
            //    {
            //        notExist.Add(data);
            //    }
            //    //Console.WriteLine($"Absolute path: {data.FullPath}");
            //    //Console.WriteLine($"NewPath : {data.NewPath}");
            //    Console.WriteLine();
            //}

            //var oldDatereportdata = context.CRMLeadTransactionImages;

            //foreach (var data in existData)
            //{
            //    if (File.Exists(data.NewPath))
            //    {
            //        var FoundData = oldDatereportdata.FirstOrDefault(u => u.Id == data.Id);
            //        if (FoundData != null)
            //        {
            //            FoundData.ImagePath = data.NewRelativePath;
            //        }
            //    }
            //}

            //List<int> reportIds = new List<int>();

            //foreach (var a in existData)
            //{
            //    if (a.ReportId.HasValue)
            //    {
            //        reportIds.Add(a.ReportId.Value);
            //    }
            //};

            //var recordsToLog = expenseEntities.Where(x => x.ReportId == null || !reportIds.Contains(x.ReportId.Value)).ToList();
            //var sb = new StringBuilder();

            //sb.AppendLine("Records of CRMLeadTransactionImages which are not updated\n");
            //sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //sb.AppendLine("-------------------------------------------------------------------------------");

            //foreach (var record in recordsToLog)
            //{
            //    var isEmptyOrNullPath = string.IsNullOrWhiteSpace(record.ExistingPath);
            //    var existingPath = isEmptyOrNullPath ? "Null" : record.ExistingPath;

            //    var fullPath = isEmptyOrNullPath
            //        ? "Null"
            //        : Path.GetFullPath(Path.Combine(basePath, record.ExistingPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

            //    sb.AppendLine($"RecordId: {record.Id}\t LeadTransactionId: {record.ReportId}\t ImagePath: {existingPath}\t FullPath: {fullPath}\t CreateDate: {record.CreateDate:yyyy-MM-dd}");
            //}

            //sb.AppendLine("\nRecords of CRMLeadTransactionImages ImagePath does not exist");
            //sb.AppendLine("-------------------------------------------------------------------------------");

            //foreach (var record in notExist)
            //{
            //    sb.AppendLine($"RecordId: {record.Id}\t LeadTransactionId: {record.ReportId}\t ImagePath: {record.ExistingPath}\t FullPath: {record.FullPath}\t CreateDate: {record.CreateDate:yyyy-MM-dd}");
            //}

            //string logFilePath = Path.Combine(AppContext.BaseDirectory, "MissingCRMLeadTransactionImages.txt");
            ////File.WriteAllText(logFilePath, sb.ToString());
            //File.AppendAllText(logFilePath, sb.ToString());

            //await context.SaveChangesAsync();

            //Console.WriteLine($"Logged {recordsToLog.Count} records with missing data to: {logFilePath}");
            //Console.WriteLine("Press enter to continue");
            //Console.ReadLine();
        }
        catch (Exception ex)
        {
            string tableName = "CRMLeadTransactionImages";
            Console.WriteLine("An unexpected error occurred:");
            Console.WriteLine(ex.Message);
            File.AppendAllText("UnhandledExceptionLog.txt", $"Table: {tableName}\n");
            File.AppendAllText("UnhandledExceptionLog.txt", ex.ToString());
            Console.ReadLine();
        }
    }
    public static void SavingFiles(List<ExnpensesDto> missingData, string fileName, string logPath, string message)
    {
        Console.WriteLine($"Saving file... {message}");
        var sb = new StringBuilder();

        sb.AppendLine(message);

        foreach (var item in missingData)
        {
            sb.AppendLine($"Id: {item.Id}\t CompanyId {item.CompanyId}\t LeadTransactionId: {item.ReportId}\t ImagePath: {item.ExistingPath}\t FullPath: {item.FullPath}\t CreateDate: {item.CreateDate:yyyy-MM-dd}");
        }
        Console.WriteLine($"File Name: {fileName}\n");

        File.WriteAllText(logPath, sb.ToString());
    }
}