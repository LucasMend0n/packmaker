using System;
using System.Diagnostics;
using System.IO.Compression;


namespace packmaker
{
    public class Program
    {
        static void Main()
        {
            Console.Write("Digite o caminho para o repositório Git: ");
            string repoPath = Console.ReadLine();
            SetBranchMain(repoPath);
            Console.Write("Digite a branch que deseja comparar: ");
            string branch = Console.ReadLine();

            ExecuteGitDiff(repoPath, branch);



        }
        static void SetBranchMain(string repoPath)
        {
            Console.WriteLine("Mudando para branch main...");
            try
            {
                Process ps = new Process();
                ProcessStartInfo stInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"checkout main",
                    WorkingDirectory = repoPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                ps.StartInfo = stInfo;
                ps.Start();

                string output = ps.StandardOutput.ReadToEnd();
                string error = ps.StandardError.ReadToEnd();

                ps.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("Erro ao mudar de branch: ");
                    Console.WriteLine(error);
                    return;
                }
                Console.WriteLine("Branch alterada para main! ");

            }
            catch (System.Exception)
            {

                throw;
            }
        }
        static void ExecuteGitDiff(string repoPath, string branch)
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"diff --name-only {branch}",
                    WorkingDirectory = repoPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.StartInfo = startInfo;
                process.Start();


                List<string> filePaths = new List<string>();
                StreamReader stdout = process.StandardOutput;

                while(!stdout.EndOfStream){
                    filePaths.Add($"{repoPath}\\{stdout.ReadLine().Replace("/","\\")}"); 
                }
                
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                Console.WriteLine($"Arquivos modificados na branch: {branch}");
                filePaths.ForEach(i => Console.WriteLine(i));

                Console.WriteLine($"Entrando na branch {branch}..."); 

                Process psChangeBranch = new Process();
                ProcessStartInfo srtInfoChangeBranch = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"checkout {branch}",
                    WorkingDirectory = repoPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                psChangeBranch.StartInfo = srtInfoChangeBranch;
                psChangeBranch.Start();

                psChangeBranch.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("Erro ao executar o comando Git Diff:");
                    Console.WriteLine(error);
                    return;
                }

                Console.WriteLine("Informe o destino para o zip: ");
                string pathForZip = Console.ReadLine();
                
                if (string.IsNullOrEmpty(pathForZip))
                {
                    Console.WriteLine("Path não selecionao, encerrando pathmaker...");
                    return;
                }

                CreateZipFile(filePaths, pathForZip); 

            }
            
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao executar o comando Git Diff:");
                Console.WriteLine(ex.Message);
            }
        }
        static void CreateZipFile(List<String> filePaths, string zipPath)
        {
            Console.WriteLine("Digite o nome do arquivo zip: ");
            string zipFile = Console.ReadLine();

            string fullZipPath = zipPath + $"\\{zipFile}.zip";

            Console.WriteLine("Iniciando compressão dos arquivos....");

            var zip = ZipFile.Open(fullZipPath, ZipArchiveMode.Create);

            foreach (var file in filePaths)
            {
                zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
            }
            zip.Dispose();

            Console.WriteLine("Compressão dos arquivos finalizada!");

        }
    }

}