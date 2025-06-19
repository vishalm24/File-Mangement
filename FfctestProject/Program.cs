using ConsoleApp1.Dto;
using FfctestProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            //Commented code for moving files to a new directory.
            //var newDirectory = AppContext.BaseDirectory + $"MissingImagesInDB\\{tableName}\\";
            //if (!Directory.Exists(newDirectory))
            //{
            //    Directory.CreateDirectory(newDirectory);

            //}
            //Console.WriteLine("Target Directory: " + newDirectory);
            //var count = 0;
            //var count1 = 0;

            foreach (var item in apiPaths)
            {
                if(context.CRMLeadTransactionImages.Any(x => x.ImagePath == item))
                    continue;
                var fullPath= AppContext.BaseDirectory + item.Replace("/API/", "").Replace("/","\\");
                sb.AppendLine($"Full path : {fullPath}\t ReativePath :{item}");

                //Commented code for moving files to a new directory.
                //var fileName = Path.GetFileName(fullPath);
                //var newPath = AppContext.BaseDirectory + $"MissingImagesInDB\\{tableName}\\{fileName}";
                ////File.Move(fullPath, newPath, true);
                //File.Copy(fullPath, newPath, true);
                //Console.WriteLine($"\nFile copied sucessfully...\nNew path: {newPath}\nOld Path: {fullPath}");
                //if(File.Exists(newPath))
                //{
                //    count1++;
                //}
                //count++;
            }
            //Commented code for moving files to a new directory.
            //Console.WriteLine($"\nTotal files: {count}\n");
            //Console.WriteLine($"Total files copied with new path: {count1}\n");
            Console.WriteLine($"File Path: {tableName}MissingImagePathData.txt");
            File.WriteAllText(logPath, sb.ToString());

            Console.ReadLine();
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