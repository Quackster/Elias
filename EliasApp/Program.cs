using System;
using System.IO;

namespace EliasApp
{
    class Program
    {
        private static string FFDEC_PATH = @"C:\Program Files (x86)\FFDec\ffdec.jar";
        private static string OUTPUT_PATH = @"C:\Users\Alex\Documents\GitHub\Elias\EliasDirector\temp";
        private static string DIRECTOR_PATH = @"C:\Users\Alex\Documents\GitHub\Elias\EliasDirector\elias_app.exe";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments supplied");

#if DEBUG
            Console.Read();
#endif
                return;
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

#if DEBUG
                    Console.Read();
#endif
                    return;
                }

                var outputDirectory = Path.Combine(Environment.CurrentDirectory, "output");

                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                var smallElias = new EliasLibrary.Elias(Path.GetFileNameWithoutExtension(fileName),
    true, fullFileName, int.Parse(args[1]), int.Parse(args[2]),
    FFDEC_PATH, OUTPUT_PATH, DIRECTOR_PATH);

                smallElias.Parse();

                var castFile = string.Format("hh_furni_xx_s_{0}.cct", smallElias.Sprite);
                var castFilePath = Path.Combine(Environment.CurrentDirectory, "output", castFile);

                if (File.Exists(castFilePath))
                    File.Delete(castFilePath);

                File.Copy(Path.Combine(OUTPUT_PATH, "cast_data", castFile), castFilePath);

                var elias = new EliasLibrary.Elias(Path.GetFileNameWithoutExtension(fileName),
                    false, fullFileName, int.Parse(args[1]), int.Parse(args[2]),
                    FFDEC_PATH, OUTPUT_PATH, DIRECTOR_PATH);

                elias.Parse();

                castFile = string.Format("hh_furni_xx_{0}.cct", elias.Sprite);
                castFilePath = Path.Combine(Environment.CurrentDirectory, "output", castFile);

                if (File.Exists(castFilePath))
                    File.Delete(castFilePath);

                File.Copy(Path.Combine(OUTPUT_PATH, "cast_data", castFile), castFilePath);

            }
            catch (Exception ex)
            {
                WriteError(ex.ToString());
            }

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
