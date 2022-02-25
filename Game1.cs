using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NotSoSimpleLevelDesigner
{
    /*
    public enum ProgramState
    {
        Setup,
        Edit,
        Done
    }
    */
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
        private int tileSize = 16;
        private string userInput;
        private int columns;
        private int rows;
        private string filepath;
        private char[,] level;
        private LevelManager levelManager;
        Texture2D gameObjectTexture;
        //private ProgramState programState = ProgramState.Edit;
        private EditorState editorState = EditorState.Walls;
        private KeyboardState kbState;
        private KeyboardState prevKb;
        private MouseState mouseState;
        private Point maxMouse;
        private bool hasSelectedSource = false;
        private string welcome = @"   _____                   _        _   Welcome to the   _   _____            _
  / ____\     (not so)    | |      | |                  | | |  __ \          (_)                      
 | (___  _ _ __ ___  _ __ | | ___  | |     _____   _____| | | |  | | ___  ___ _  __ _ _ __   ___ _ __ 
  \___ \| | '_ ` _ \| '_ \| |/ _ \ | |    / _ \ \ / / _ \ | | |  | |/ _ \/ __| |/ _` | '_ \ / _ \ '__|
  ____) | | | | | | | |_) | |  __/ | |___|  __/\ V /  __/ | | |__| |  __/\__ \ | (_| | | | |  __/ |   
 |_____/|_|_| |_| |_| .__/|_|\___| |______\___| \_/ \___|_| |_____/ \___||___/_|\__, |_| |_|\___|_|   
                    | |                                                          __/ |                   
                    |_|               by Jackson Majewski                       |___/        v1.1.0     
";

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        /// <summary>
        /// Generates walls of specified width and height
        /// </summary>
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

        /// <summary>
        /// Helper method to avoid frame perfect input
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsValidKeypress(Keys key)
        {
            return (kbState.IsKeyDown(key) && !prevKb.IsKeyDown(key));
        }

        /// <summary>
        /// Helper method to handle input and convert from pixel point to array address
        /// </summary>
        /// <param name="c"></param>
        public void HandleMouseInput(char c)
        {
            if(mouseState.LeftButton == ButtonState.Pressed)
            {
                //Snap to tile
                int x = Math.Clamp(
                        ((int)mouseState.X / tileSize),
                        0,
                        maxMouse.X / tileSize - 1);

                int y = Math.Clamp(
                        ((int)mouseState.Y / tileSize),
                        0,
                        maxMouse.Y / tileSize - 1);
                Point mousePosition = new Point(x, y);

                //Update level array
                level[mousePosition.Y, mousePosition.X] = c;
            }
            if(mouseState.RightButton == ButtonState.Pressed)
            {
                //Snap to tile
                int x = Math.Clamp(
                        ((int)mouseState.X / tileSize),
                        0,
                        maxMouse.X / tileSize - 1);

                int y = Math.Clamp(
                        ((int)mouseState.Y / tileSize),
                        0,
                        maxMouse.Y / tileSize - 1);
                Point mousePosition = new Point(x, y);

                //Update level array
                level[mousePosition.Y, mousePosition.X] = '0';
            }
        }

        protected override void Initialize()
        {
            //Print welcome message
            Console.WriteLine(welcome + "\n");

            System.IO.Directory.CreateDirectory("levels"); //Create levels directory if it doesn't exist
            do
            {
                Console.Write("(G)enerate or (L)oad> ");
                userInput = Console.ReadLine().Trim().ToUpper();

                switch (userInput)
                {
                    case "G":
                        //Set level file
                        Console.Write("level name> ");
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

                        //Get height of room in tiles
                        Console.Write("tile size> ");
                        userInput = Console.ReadLine();
                        tileSize = int.Parse(userInput);

                        level = new char[rows, columns];
                        levelManager = new LevelManager(filepath);
                        GenerateWalls();
                        hasSelectedSource = true;
                        break;

                    case "L":
                        Console.Write("filepath> ");
                        userInput = Console.ReadLine();
                        levelManager = new LevelManager("levels\\" + userInput + ".sslvl");
                        hasSelectedSource = levelManager.LoadFile(out level, out rows, out columns);
                        break;
                }

            }
            while (!hasSelectedSource);

            //Display key to user
            Console.WriteLine("\nKey:\n\n" +
                "W - Wall Editor\n" +
                "I - Invis Wall\n" +
                "M - Mirror\n" +
                "E - Enemy\n" +
                "P - Player\n");

            //Flavor
            Console.WriteLine("Entering Wall editor");
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            gameObjectTexture = Content.Load<Texture2D>("gameObject");
            maxMouse = new Point(columns * tileSize, rows * tileSize);
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                levelManager.SaveFile(level);
                Exit();
            }

            //Get input states
            kbState = Keyboard.GetState();
            mouseState = Mouse.GetState();

            //Check whether to save
            if (IsValidKeypress(Keys.S))
            {
                Console.WriteLine("Saving to file...");
                levelManager.SaveFile(level);
            }

            //FSM for EditorState
            switch (editorState)
            {
                case EditorState.Walls:

                    //Change state
                    if (IsValidKeypress(Keys.I))
                    {
                        Console.WriteLine("Entering Invisible Wall editor");
                        editorState = EditorState.InvisibleWalls;
                    }
                    if (IsValidKeypress(Keys.M))
                    {
                        Console.WriteLine("Entering Mirror editor");
                        editorState = EditorState.Mirrors;
                    }
                    if (IsValidKeypress(Keys.E))
                    {
                        Console.WriteLine("Entering Enemy editor");
                        editorState = EditorState.Enemies;
                    }
                    if (IsValidKeypress(Keys.P))
                    {
                        Console.WriteLine("Entering Player editor");
                        editorState = EditorState.Player;
                    }
                    HandleMouseInput('W');
                    break;

                case EditorState.InvisibleWalls:

                    //Change state
                    if (IsValidKeypress(Keys.W))
                    {
                        Console.WriteLine("Entering Wall editor");
                        editorState = EditorState.Walls;
                    }
                    if (IsValidKeypress(Keys.M))
                    {
                        Console.WriteLine("Entering Mirror editor");
                        editorState = EditorState.Mirrors;
                    }
                    if (IsValidKeypress(Keys.E))
                    {
                        Console.WriteLine("Entering Enemy editor");
                        editorState = EditorState.Enemies;
                    }
                    if (IsValidKeypress(Keys.P))
                    {
                        Console.WriteLine("Entering Player editor");
                        editorState = EditorState.Player;
                    }
                    HandleMouseInput('I');
                    break;

                case EditorState.Mirrors:

                    //Change state
                    if (IsValidKeypress(Keys.W))
                    {
                        Console.WriteLine("Entering Wall editor");
                        editorState = EditorState.Walls;
                    }
                    if (IsValidKeypress(Keys.I))
                    {
                        Console.WriteLine("Entering Invisible Wall editor");
                        editorState = EditorState.InvisibleWalls;
                    }
                    if (IsValidKeypress(Keys.E))
                    {
                        Console.WriteLine("Entering Enemy editor");
                        editorState = EditorState.Enemies;
                    }
                    if (IsValidKeypress(Keys.P))
                    {
                        Console.WriteLine("Entering Player editor");
                        editorState = EditorState.Player;
                    }
                    HandleMouseInput('M');
                    break;

                case EditorState.Enemies:

                    //Change state
                    if (IsValidKeypress(Keys.W))
                    {
                        Console.WriteLine("Entering Wall editor");
                        editorState = EditorState.Walls;
                    }
                    if (IsValidKeypress(Keys.I))
                    {
                        Console.WriteLine("Entering Invisible Wall editor");
                        editorState = EditorState.InvisibleWalls;
                    }
                    if (IsValidKeypress(Keys.M))
                    {
                        Console.WriteLine("Entering Mirror editor");
                        editorState = EditorState.Mirrors;
                    }
                    if (IsValidKeypress(Keys.P))
                    {
                        Console.WriteLine("Entering Player editor");
                        editorState = EditorState.Player;
                    }
                    HandleMouseInput('E');
                    break;

                case EditorState.Player:

                    //Change state
                    if (IsValidKeypress(Keys.W))
                    {
                        Console.WriteLine("Entering Wall editor");
                        editorState = EditorState.Walls;
                    }
                    if (IsValidKeypress(Keys.I))
                    {
                        Console.WriteLine("Entering Invisible Wall editor");
                        editorState = EditorState.InvisibleWalls;
                    }
                    if (IsValidKeypress(Keys.M))
                    {
                        Console.WriteLine("Entering Mirror editor");
                        editorState = EditorState.Mirrors;
                    }
                    if (IsValidKeypress(Keys.E))
                    {
                        Console.WriteLine("Entering Enemy editor");
                        editorState = EditorState.Enemies;
                    }
                    HandleMouseInput('P');
                    break;
            }
            prevKb = kbState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < columns; j++)
                {
                    //Check what to draw
                    switch(level[i, j])
                    {
                        case 'W':
                            _spriteBatch.Draw(gameObjectTexture, new Rectangle(j * 16, i * 16, 16, 16), Color.White);
                            break;

                        case 'P':
                            _spriteBatch.Draw(gameObjectTexture, new Rectangle(j * 16, i * 16, 16, 16), Color.Purple);
                            break;

                        case 'I':
                            _spriteBatch.Draw(gameObjectTexture, new Rectangle(j * 16, i * 16, 16, 16), Color.Blue);
                            break;

                        case 'M':
                            _spriteBatch.Draw(gameObjectTexture, new Rectangle(j * 16, i * 16, 16, 16), Color.Green);
                            break;

                        case 'E':
                            _spriteBatch.Draw(gameObjectTexture, new Rectangle(j * 16, i * 16, 16, 16), Color.DarkRed);
                            break;
                    }
                }
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
