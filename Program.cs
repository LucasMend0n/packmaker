using System;
using System.Diagnostics;
using System.IO.Compression;


namespace packmaker
{
    public class Program
    {
        static void Main()
        {
            int loop = 0;   
            while (loop == 0) {

                Console.Write("Digite o caminho para o repositório Git: ");
                string repoPath = Console.ReadLine();
                Console.Write("Digite a configuração do Git Diff: ");
                string config = Console.ReadLine();
                Console.Write("Digite o nome do arquivo zip: ");
                string folderBaseName = Console.ReadLine();
                string finalZipPath = @"C:/packmaker_out/" + $"{folderBaseName}";

                //string repoPath = @"C:/dev/SH0743_GSS_HOSP";
                //string repoPath = @"C:/inetpub/wwwroot/SH0743_GSS_HOSP";
                //string branch = "main";
                //string folderBaseName = "packmaker_testZip";
                //string txtFileName = "proceduresFound";

                Console.WriteLine("Criando pasta destino...");

                Directory.CreateDirectory(finalZipPath);

                Console.WriteLine("Obtendo arquivos....");

                List<string> modifiedFiles = GetModifiedFiles(repoPath, config);
                List<string> aspFiles = modifiedFiles.Where(file => file.EndsWith(".asp", StringComparison.OrdinalIgnoreCase)).ToList();

                if (modifiedFiles.Count == 0)
                {
                    Console.WriteLine("Nenhum arquivo obtido. Programa encerrado.");
                    return;
                }
                else
                {
                    Console.WriteLine("Arquivos obtidos!");
                    string modifiedFilesPath = Path.Combine(finalZipPath, $"{folderBaseName}_files.txt").Replace("/", "\\");
                    File.WriteAllLines(modifiedFilesPath, modifiedFiles);
                }

                Console.WriteLine("Procurando por chamadas de procedures...");

                List<string> procedureCalls = FindProcedureCalls(repoPath, aspFiles);
                string txtPathFile = Path.Combine(finalZipPath, $"{folderBaseName}_procs.txt").Replace("/", "\\");

                if (procedureCalls.Count != 0)
                {
                    Console.WriteLine($"Chamadas de procedures encontradas! Elas serão adicionadas ao arquivo: {folderBaseName}.txt");
                    File.WriteAllLines(txtPathFile, procedureCalls);
                }
                else
                {
                    Console.WriteLine("Nenhuma procedure encontrada!");
                }

                FileInfo fileInfo = new FileInfo(txtPathFile);
                fileInfo.Directory.Create();

                Console.WriteLine("Iniciando montagem do zip....");

                CriarZip(repoPath, modifiedFiles, finalZipPath, folderBaseName);

                Console.WriteLine("Compressão dos arquivos finalizada!");

                string zipFolder = finalZipPath.Replace("/", "\\");

                Process.Start("explorer.exe", zipFolder);

                Console.WriteLine("Deseja converter novo pack? [y/n]");
                var resp = Console.ReadLine();

                if (resp == "n")
                {
                    loop = 1;
                }
            }

        }
        static List<string> GetModifiedFiles(string repoPath, string content)
        {
            List<string> modifiedFiles = new List<string>();
            using (var process = new Process())
            {
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = $"diff --name-only {content}"; 
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
            return modifiedFiles;

        }

        static List<string> FindProcedureCalls(string repository, List<string> aspFilesList)
        {
            List<string> procedureCalls = new List<string>();
            foreach (var file in aspFilesList)
            {
                string aspFilePath = Path.Combine(repository, file);
                try
                {
                    string[] lines = File.ReadAllLines(aspFilePath); 
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];
                        int start = 0; 
                        while(start < line.Length)
                        {
                            start = line.IndexOf("p_",start,StringComparison.OrdinalIgnoreCase); 
                            if (start < 0)
                            {
                                break; 
                            }
                            int end = line.IndexOf(' ', start); 
                            if (end < 0)
                            {
                                end = line.Length;
                            }

                            string storedProcedure = line.Substring(start, end - start).Trim();
                            procedureCalls.Add($"Procedure: {storedProcedure} | Arquivo: {file} | Linha: {i + 1 }");
                            start = end;
                        }
 
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao ler o arquivo {aspFilePath}: {ex.Message}");
                }
            }
            return procedureCalls;
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
