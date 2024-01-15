using System;
using System.IO.Compression;


namespace packmaker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando zip!");
            string? pathToZip = "C:\\inetpub\\wwwroot\\SH0743_GSS_HOSP\\_includes\\Templates";
            string? pathToExit = "C:\\dev\\Projects\\testezip.zip";

            ZipFile.CreateFromDirectory(pathToZip, pathToExit);

            Console.WriteLine("Zip completo!"); 


        }
    }
}