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
        private StreamReader input;

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
                Console.WriteLine("Saved!");
            }
            catch (Exception)
            {
                output.Close();
                Console.WriteLine("Error saving file.");
            }
        }

        public bool SaveCustomInfo(string levelInfo)
        {
            try
            {
                output = new StreamWriter(filepath);
                output.WriteLine(levelInfo);
                output.Close();
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("Error! File \"info.wal\" is already open!");
                return false;
            }
        }

        public bool LoadFile(out char[,] level, out int rows, out int columns)
        {
            string line;
            string[] data;

            try
            {
                input = new StreamReader(filepath);

                //Get number of rows and columns
                line = input.ReadLine();
                data = line.Split(',');
                level = new char[int.Parse(data[0]), int.Parse(data[1])];
                rows = int.Parse(data[0]);
                columns = int.Parse(data[1]);

                //Load 2D array into level
                line = input.ReadLine();
                for (int row = 0; row < level.GetLength(0); row++)
                {
                    for (int column = 0; column < level.GetLength(1); column++)
                    {
                        level[row, column] = line[column];
                    }
                    line = input.ReadLine();
                }

                input.Close();
                return true;
            }
            catch (Exception)
            {
                if(input != null)
                    input.Close();
                Console.WriteLine("Error loading file.\n");
                level = null;
                rows = 0;
                columns = 0;
                return false;
            }
        }
    }
}
