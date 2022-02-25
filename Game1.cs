using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NotSoSimpleLevelDesigner
{
    public enum ProgramState
    {
        Setup,
        Edit,
        Done
    }
    public enum EditorState
    {
        Walls,
        InvisibleWalls,
        Mirrors,
        Enemies,
        Player
    }

    public class Game1 : Game
    {
       /*
        * Level Key:
        * 
        * 0 - Empty
        * W - Wall
        * I - Invisible Wall
        * M - Mirror
        * E - Enemy
        * P - Player
        * 
        */
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //Variables
        string userInput;
        int columns;
        int rows;
        string filepath;
        char[,] level;
        LevelManager levelManager;
        string welcome = @"   _____                   _        _   Welcome to the   _   _____            _
  / ____\     (not so)    | |      | |                  | | |  __ \          (_)                      
 | (___  _ _ __ ___  _ __ | | ___  | |     _____   _____| | | |  | | ___  ___ _  __ _ _ __   ___ _ __ 
  \___ \| | '_ ` _ \| '_ \| |/ _ \ | |    / _ \ \ / / _ \ | | |  | |/ _ \/ __| |/ _` | '_ \ / _ \ '__|
  ____) | | | | | | | |_) | |  __/ | |___|  __/\ V /  __/ | | |__| |  __/\__ \ | (_| | | | |  __/ |   
 |_____/|_|_| |_| |_| .__/|_|\___| |______\___| \_/ \___|_| |_____/ \___||___/_|\__, |_| |_|\___|_|   
                    | |                                                          __/ |                
                    |_|               by Jackson Majewski                       |___/                 
";

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        public void GenerateWalls()
        {
            //Top row
            for (int i = 0; i < columns; i++)
                level[0, i] = 'W';

            for (int i = 1; i < rows - 1; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (j == 0 || j == columns - 1)
                    {
                        level[i, j] = 'W';
                    }
                    else
                        level[i, j] = '0'; //Not a wall
                }
            }

            //Bottom row
            for (int i = 0; i < columns; i++)
                level[rows - 1, i] = 'W';

        }

        protected override void Initialize()
        {
            //Print welcome message
            Console.WriteLine(welcome + "\n");

            System.IO.Directory.CreateDirectory("levels"); //Create levels directory if it doesn't exist

            //Set level file
            Console.Write("Level Name> ");
            userInput = Console.ReadLine();
            filepath = "levels\\" + userInput + ".sslvl";

            //Get width of room in tiles
            Console.Write("width> ");
            userInput = Console.ReadLine();
            columns = int.Parse(userInput);

            //Get height of room in tiles
            Console.Write("height> ");
            userInput = Console.ReadLine();
            rows = int.Parse(userInput);

            level = new char[rows, columns];
            levelManager = new LevelManager(filepath);
            GenerateWalls();
            levelManager.SaveFile(level);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
