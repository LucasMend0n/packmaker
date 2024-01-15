using System;
using System.Diagnostics;
using System.IO.Compression;


namespace packmaker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando zip!");
            string? pathToZip = "C:\\inetpub\\wwwroot\\SH0743_GSS_HOSP\\_includes\\Templates";
            string? repo = "C:\\inetpub\\wwwroot\\SH0743_GSS_HOSP";
            string? pathToExit = "C:\\dev\\Projects\\testezip.zip";

            string? gitCommand = "git";
            string? gitChangeBr = @"checkout main"; 
            string? gitPull = @"pull";
            Process.Start(gitCommand, gitPull);

            Process.Start(new ProcessStartInfo(gitCommand, gitChangeBr)
            {
                WorkingDirectory = repo
            })
            .WaitForExit();

            ZipFile.CreateFromDirectory(pathToZip, pathToExit);

            Console.WriteLine("Zip completo!"); 


        }
    }
}