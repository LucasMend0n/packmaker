﻿using System;
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
            Console.Write("Digite a branch que deseja comparar: ");
            string branch = Console.ReadLine();
            Console.Write("Digite o nome do arquivo zip: ");
            string folderBaseName = Console.ReadLine();
            string finalZipPath = @"C:/packmaker_out";

            //string repoPath = @"C:/dev/SH0743_GSS_HOSP";
            //string branch = "main";
            //string repoPath = @"C:/inetpub/wwwroot/SH0743_GSS_HOSP";
            //string folderBaseName = "packmaker_testZip";
            //string txtFileName = "proceduresFound";

            Console.WriteLine("Obtendo arquivos....");
            List<string> modifiedFiles = GetModifiedFiles(repoPath, branch);
            List<string> aspFiles = modifiedFiles.Where(file => file.EndsWith(".asp", StringComparison.OrdinalIgnoreCase)).ToList();
            
            if(modifiedFiles.Count == 0)
            {
                Console.WriteLine("Nenhum arquivo obtido. Programa encerrado.");
                return;
            }

            Console.WriteLine("Procurando por chamadas de procedures...");

            List<string> procedureCalls = FindProcedureCalls(repoPath, aspFiles);

            if(procedureCalls.Count != 0)
            {
                Console.WriteLine($"Chamadas de procedures encontradas! Elas serão adicionadas ao arquivo: {folderBaseName}.txt");
                string txtPathFile = Path.Combine(finalZipPath, $"{folderBaseName}_procs.txt").Replace("/","\\");
                FileInfo fileInfo = new FileInfo(txtPathFile);
                fileInfo.Directory.Create();
                File.WriteAllLines(txtPathFile, procedureCalls); 
            }
            else
            {
                Console.WriteLine("Nenhuma procedure encontrada!"); 
            }



            Console.WriteLine("Iniciando montagem do zip....");

            CriarZip(repoPath, modifiedFiles, finalZipPath, folderBaseName);

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
                        if(line.Contains("p_", StringComparison.OrdinalIgnoreCase))
                        {
                            procedureCalls.Add($"Arquivo: {file}, Linha: {i + 1 }, Conteudo: {line.Trim()}");
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
