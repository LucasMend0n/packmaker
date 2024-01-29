using System;
using System.Diagnostics;
using System.IO.Compression;


namespace packmaker
{
    public class Program
    {
        static void Main()
        {
            //Console.Write("Digite o caminho para o repositório Git: ");
            string repoPath = @"C:/inetpub/wwwroot/SH0743_GSS_HOSP";  //Console.ReadLine();
            Console.Write("Digite a branch que deseja comparar: ");
            string branch = Console.ReadLine();
            Console.Write("Digite o caminho para a saída do zip: "); 
            string finalZipPath = Console.ReadLine();
            Console.Write("Digite o nome do arquivo zip: ");
            string zipFile = Console.ReadLine();

            Console.WriteLine("Obtendo arquivos....");
            List<string> modifiedFiles = GetModifiedFiles(repoPath, branch); 
            
            if(modifiedFiles.Count == 0)
            {
                Console.WriteLine("Nenhum arquivo obtido. Programa encerrado.");
                return;
            }

            Console.WriteLine("Iniciando montagem do zip....");

            CriarZip(repoPath, modifiedFiles, finalZipPath, zipFile);

            Console.WriteLine("Compressão dos arquivos finalizada!");

            string zipFolder = finalZipPath.Replace("/","\\");

            Process.Start("explorer.exe", zipFolder);

        }
        static List<string> GetModifiedFiles(string repoPath, string branch)
        {
            List<string> modifiedFiles = new List<string>();
            using (var process = new Process())
            {
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = $"diff --name-only ..{branch}"; 
                process.StartInfo.WorkingDirectory = repoPath;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start(); 
                
                while (!process.StandardOutput.EndOfStream)
                {
                    string filePath = process.StandardOutput.ReadLine();
                    modifiedFiles.Add(filePath);
                }
                process.WaitForExit(); 
;            }
            Console.WriteLine("Arquivos obtidos!");
            return modifiedFiles;

        }
        static void CreateZipFile(List<string> filePaths, string zipPath, string zipFileName)
        {

            string fullZipPath = zipPath + $"\\{zipFileName}.zip";

            var zip = ZipFile.Open(fullZipPath, ZipArchiveMode.Create);

            foreach (var file in filePaths)
            {
                zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
            }
            zip.Dispose();

        }

        static void CriarZip(string rootPath, List<string> files, string zipFilePath, string zipFileName)
        {
            using (var zipArchive = ZipFile.Open(zipFilePath + $"\\{zipFileName}.zip", ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    string filePath = Path.Combine(rootPath, file);
                    string relativePath = Path.GetRelativePath(rootPath, filePath);

                    zipArchive.CreateEntryFromFile(filePath, relativePath);
                }
            }
        }
    }
}
