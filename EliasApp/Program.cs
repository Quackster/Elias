﻿using System;
using System.IO;

namespace EliasApp
{
    class Program
    {
        private static string FFDEC_PATH = @"C:\Program Files (x86)\FFDec\ffdec.jar";
        private static string OUTPUT_PATH = @"C:\Users\Alex\Documents\GitHub\Elias\EliasApp\bin\Debug\temp";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments supplied");
                goto exit;
            }

            try
            {
                string fullFileName = args[0];
                string fileName = Path.GetFileName(fullFileName);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Reading file: ");
                Console.ResetColor();
                Console.WriteLine(args[0]);

                var fileExtension = Path.GetExtension(fileName);

                if (fileExtension != ".swf")
                {
                    WriteError("The input file is not a .swf file!");
                    goto exit;
                }

                var elias = new EliasLibrary.Elias(args[3] == "32", fullFileName, int.Parse(args[1]), int.Parse(args[2]), FFDEC_PATH, OUTPUT_PATH);
                elias.Parse();
            }
            catch (Exception ex)
            {
                WriteError(ex.ToString());
            }

            exit:
#if DEBUG
            Console.Read();
#endif
        }

        private static void WriteError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error occurred: ");
            Console.ResetColor();
            Console.WriteLine(errorMessage);
        }
    }
}