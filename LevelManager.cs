using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NotSoSimpleLevelDesigner
{
    class LevelManager
    {
        //Fields
        private string filepath;
        private StreamWriter output;

        //Constructor
        public LevelManager(string filepath)
        {
            this.filepath = filepath;
        }

        //Methods
        public void SaveFile(char[,] level)
        {
            try {
                output = new StreamWriter(filepath);

                output.WriteLine("{0}, {1}", level.GetLength(0), level.GetLength(1));

                for (int row = 0; row < level.GetLength(0); row++)
                {
                    for (int column = 0; column < level.GetLength(1); column++)
                    {
                        output.Write(level[row, column]);
                    }
                    output.WriteLine();
                }
                output.Close();
            }
            catch (Exception)
            {
                output.Close();
                Console.WriteLine("Error saving file.");
            }
        }
    }
}
