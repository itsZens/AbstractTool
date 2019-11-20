using System;

namespace M06_UF1_PersistenciaFitxers
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Insert the name of a file to analyze." + Environment.NewLine);
            Console.WriteLine("Name: ");
            Console.SetCursorPosition(6, 2);
            string file = Console.ReadLine();
            Console.WriteLine(Environment.NewLine);

            if (AbstractTool.FileChecker())
            {
                AbstractTool.FileInformation(file);
            }
                

        }
    }
}
