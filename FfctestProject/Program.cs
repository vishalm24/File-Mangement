using ConsoleApp1.Dto;
using FfctestProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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


            //var sql = @"
            //                SELECT 
            //                    ISNULL(CAST(ei.Id AS FLOAT), 0) AS Id, 
            //                    ed.CompanyId,
            //                    ISNULL(ei.ExpenseReportId, ec.ExpenseReportId) AS ReportId,
            //                    ISNULL(ei.ImagePath, '') AS ExistingPath,
            //                    ISNULL(ei.CreateDate, CAST('1900-01-01' AS DATETIME)) AS CreateDate
            //                FROM ExpenseApprovalHistory ec
            //                FULL JOIN ExpenseReportImages ei ON ec.ExpenseReportId = ei.ExpenseReportId
            //                FULL JOIN ExpenseDefaultFinanceApprovers ed ON ed.UserId = ec.UserId";
            var sql = @"
                            SELECT
                                u.UserId AS Id,
                                cmr.ShopId AS CompanyId,
                                u.UserId AS ReportId,
                                CASE
                                    WHEN u.UserImage IS NULL OR u.UserImage = 'NA' OR u.UserImage = '' THEN ''
                                    WHEN u.UserImage NOT LIKE 'Images/UserProfileImage/%' OR u.UserImage NOT LIKE '%API/%' THEN '/API/Images/UserProfileImage/'+u.UserImage
		                            ELSE ''
                                END AS ExistingPath,
                            ISNULL(u.CreatedDate, CAST('1900-01-01' AS DATETIME)) AS CreateDate
                            FROM [User] u
                            LEFT JOIN CompanyMemberRoleMapping cmr ON cmr.UserId = u.UserId;";

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

            foreach (var expense in expenseEntities)
            {
                var imagePath = expense.ExistingPath;
                var imageFilename = Path.GetFileName(imagePath);
                bool hasValidPath = !string.IsNullOrWhiteSpace(imagePath);
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
                    CreateDate = expense.CreateDate,
                    NewPath = ($"{basePath}{TrimedImagePath.Replace("/API/", "/")}{expense.CompanyId}\\{expense.ReportId}\\{date}\\{imageFilename}").Replace('/', '\\'),
                    NewRelativePath = ($"{TrimedImagePath}{expense.CompanyId}\\{expense.ReportId}\\{date}\\{imageFilename}").Replace('\\', '/')
                };

                if (hasValidPath && expense.CompanyId != null)
                    existData.Add(dto);
            }

            var notExist = new List<ExnpensesDto>();

            Console.WriteLine("Entries with valid image paths:\n");
            foreach (var data in existData)
            {
                if (File.Exists(data.FullPath))
                {
                    string targetDirectory = Path.GetDirectoryName(data.NewPath);
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                        Console.WriteLine("Target Directory: " + targetDirectory);
                    }
                    File.Move(data.FullPath, data.NewPath, true);
                    Console.WriteLine("File moved successfully.");
                    Console.WriteLine($"Id: {data.Id}");
                    Console.WriteLine($"CompanyId: {data.CompanyId}");
                    Console.WriteLine($"ReportId: {data.ReportId}");
                    Console.WriteLine($"Date: {data.CreateDate:yyyy-MM-dd}");
                    Console.WriteLine($"Relative path: {data.ExistingPath}");
                    Console.WriteLine("FullPath: " + data.FullPath);
                    Console.WriteLine("NewPath: " + data.NewPath);
                    Console.WriteLine($"NewRelativePath : {data.NewRelativePath}");
                }
                else
                {
                    notExist.Add(data);
                }
                //Console.WriteLine($"Absolute path: {data.FullPath}");
                //Console.WriteLine($"NewPath : {data.NewPath}");
                Console.WriteLine();
            }

            var oldDatereportdata = context.Users;

            foreach (var data in existData)
            {
                if (File.Exists(data.NewPath))
                {
                    var FoundData = oldDatereportdata.FirstOrDefault(u => u.UserId == data.Id);
                    if (FoundData != null)
                    {
                        FoundData.UserImage = data.NewRelativePath;
                    }
                }
            }

            List<int> reportIds = new List<int>();

            foreach (var a in existData)
            {
                if (a.ReportId.HasValue)
                {
                    reportIds.Add(a.ReportId.Value);
                }
            };

            var recordsToLog = expenseEntities.Where(x => x.ReportId == null || !reportIds.Contains(x.ReportId.Value)).ToList();
            var sb = new StringBuilder();

            sb.AppendLine("\nRecords of User which are not updated\n");
            sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("-------------------------------------------------------------------------------");

            foreach (var record in recordsToLog)
            {
                var isEmptyOrNullPath = string.IsNullOrWhiteSpace(record.ExistingPath);
                var existingPath = isEmptyOrNullPath ? "Null" : record.ExistingPath;

                var fullPath = isEmptyOrNullPath
                    ? "Null"
                    : Path.GetFullPath(Path.Combine(basePath, record.ExistingPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

                sb.AppendLine($"RecordId: {record.Id}\t ImagePath: {existingPath}\t FullPath: {fullPath}\t CreateDate: {record.CreateDate:yyyy-MM-dd}");
            }

            sb.AppendLine("\nRecords of User ImagePath does not exist");
            sb.AppendLine("-------------------------------------------------------------------------------");

            foreach (var record in notExist)
            {
                sb.AppendLine($"RecordId: {record.Id}\t ImagePath: {record.ExistingPath}\t FullPath: {record.FullPath}\t CreateDate: {record.CreateDate:yyyy-MM-dd}");
            }

            string logFilePath = Path.Combine(AppContext.BaseDirectory, "MissingUser.txt");
            //File.WriteAllText(logFilePath, sb.ToString());
            File.AppendAllText(logFilePath, sb.ToString());

            var flag = false;
            foreach (var item in existData)
            {
                if (File.Exists(item.NewPath))
                {
                    flag = true;
                }

                await context.SaveChangesAsync();
            }

            

            Console.WriteLine($"Logged {recordsToLog.Count} records with missing data to: {logFilePath}");
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            string tableName = "User";
            Console.WriteLine("An unexpected error occurred:");
            Console.WriteLine(ex.Message);
            File.AppendAllText("UnhandledExceptionLog.txt", $"Table: {tableName}\n");
            File.AppendAllText("UnhandledExceptionLog.txt", ex.ToString());
            Console.ReadLine();
        }
    }
}