using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NotSoSimpleLevelDesigner
{
    public enum EditorState
    {
        Walls,
        bulletOnlyWalls,
        Mirrors,
        Homing,
        Ensconcing,
        Player
    }

    public class Game1 : Game
    {
       /*
        * Level Key:
        * 
        * 0 - Empty
        * W - Wall
        * B - Bullet Only Wall (B.O.W.)
        * M - Mirror
        * E - Enemy
        * P - Player
        * G - Toggle Grid
        * S - Save
        * Arrow Keys - Move Level
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
        Texture2D gridTexture;
        private EditorState editorState = EditorState.Walls;
        private KeyboardState kbState;
        private KeyboardState prevKb;
        private MouseState mouseState;
        private Point maxMouse;
        private bool hasSelectedSource = false;
        private bool hasGrid = true;
        private string welcome = @"   _____                   _        _   Welcome to the   _   _____            _
  / ____\     (not so)    | |      | |                  | | |  __ \          (_)                      
 | (___  _ _ __ ___  _ __ | | ___  | |     _____   _____| | | |  | | ___  ___ _  __ _ _ __   ___ _ __ 
  \___ \| | '_ ` _ \| '_ \| |/ _ \ | |    / _ \ \ / / _ \ | | |  | |/ _ \/ __| |/ _` | '_ \ / _ \ '__|
  ____) | | | | | | | |_) | |  __/ | |___|  __/\ V /  __/ | | |__| |  __/\__ \ | (_| | | | |  __/ |   
 |_____/|_|_| |_| |_| .__/|_|\___| |______\___| \_/ \___|_| |_____/ \___||___/_|\__, |_| |_|\___|_|   
                    | |                                                          __/ |                   
                    |_|               by Jackson Majewski                       |___/        v1.5.0     
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

                //Update level array
                level[y, x] = c;
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
                Console.Write("(G)enerate, (L)oad, or (C)reate Custom Level file> ");
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
                        if (!int.TryParse(userInput, out columns))
                            columns = 50;

                        //Get height of room in tiles
                        Console.Write("height> ");
                        userInput = Console.ReadLine();
                        if (!int.TryParse(userInput, out rows))
                            rows = 30;

                        //Get height of room in tiles
                        Console.Write("tile size> ");
                        userInput = Console.ReadLine();
                        if (!int.TryParse(userInput, out tileSize))
                            tileSize = 16;

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

                    case "C":
                        //Prompt
                        Console.WriteLine("\nWelcome to the Custom Level File Generator!");
                        Console.WriteLine("Enter the names of your levels below. When you are done, enter a blank line. \n(NOTE: THIS WILL OVERWRITE PREEXISTING LEVELINFO FILES IN THE LEVELS DIRECTORY)");
                        Console.Write("> ");
                        levelManager = new LevelManager("levels\\info.wal");

                        string customLevelInfo = Console.ReadLine().Trim();

                        //Check if exit case
                        if(customLevelInfo == "")
                        {
                            break;
                        }

                        string currentLevelName;

                        do
                        {
                            Console.Write("> ");
                            currentLevelName = Console.ReadLine().Trim();

                            //Check if exit case
                            if (currentLevelName != "")
                            {
                                customLevelInfo += "," + currentLevelName;
                            }
                        }
                        while (currentLevelName != "");

                        if (levelManager.SaveCustomInfo(customLevelInfo))
                        {
                            Console.WriteLine("File successfully saved!");
                        }

                        break;
                }

            }
            while (!hasSelectedSource);

            //Display key to user
            Console.WriteLine("\nKey:\n\n" +
                "W - Wall Editor\n" +
                "B - Bullet Only Wall Editor\n" +
                "M - Mirror Editor\n" +
                "H - Homing Enemy Editor\n" +
                "E - Ensconcing Enemy Editor\n" +
                "P - Player Editor\n" +
                "G - Toggle Grid\n" +
                "S - Save\n" +
                "Arrow Keys - Move Level");

            //Flavor
            Console.WriteLine("Entering Wall editor");
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            gameObjectTexture = Content.Load<Texture2D>("gameObject");
            gridTexture = Content.Load<Texture2D>("grid");
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

            //Check grid
            if (IsValidKeypress(Keys.G))
            {
                if (hasGrid)
                {
                    Console.WriteLine("Grid disabled");
                    hasGrid = false;
                }
                else
                {
                    Console.WriteLine("Grid enabled");
                    hasGrid = true;
                }
                
            }

            //Check whether to move left, and if possible
            if (IsValidKeypress(Keys.Left) && level[0, 0] == '0' && level[level.GetLength(0) - 1, 0] == '0' && level[((int)level.GetLength(0) - 1) / 2, 0] == '0')
            {
                char[,] tempArray = new char[level.GetLength(0), level.GetLength(1) - 1];

                for(int i = 0; i < rows; i++)
                {
                    for (int j = 1; j < columns; j++)
                    {
                        tempArray[i, j - 1] = level[i, j];
                    }
                }
                columns--;
                maxMouse = new Point(columns * tileSize, rows * tileSize);
                level = tempArray;
            }

            //Check whether to move right, and if possible
            //TODO: fix bug where you can move it off the screen
            if (IsValidKeypress(Keys.Right))
            {
                char[,] tempArray = new char[level.GetLength(0), level.GetLength(1) + 1];

                for(int i = 0; i < rows; i++)
                {
                    tempArray[i, 0] = '0';
                }

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 1; j < columns + 1; j++)
                    {
                        tempArray[i, j] = level[i, j - 1];
                    }
                }
                columns++;
                maxMouse = new Point(columns * tileSize, rows * tileSize);
                level = tempArray;
            }

            //Check whether to move up, and if possible
            if (IsValidKeypress(Keys.Up) && level[0, 0] == '0' && level[0, level.GetLength(1) - 1] == '0' && level[0, ((int)level.GetLength(1) - 1) / 2] == '0')
            {
                char[,] tempArray = new char[level.GetLength(0) - 1, level.GetLength(1)];

                for (int i = 1; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        tempArray[i - 1, j] = level[i, j];
                    }
                }
                rows--;
                maxMouse = new Point(columns * tileSize, rows * tileSize);
                level = tempArray;
            }

            //Check whether to move down, and if possible
            //TODO: fix bug where you can move it off the screen
            if (IsValidKeypress(Keys.Down))
            {
                char[,] tempArray = new char[level.GetLength(0) + 1, level.GetLength(1)];

                for (int j = 0; j < columns; j++)
                {
                    tempArray[0, j] = '0';
                }

                for (int i = 1; i < rows + 1; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        tempArray[i, j] = level[i - 1, j];
                    }
                }
                rows++;
                maxMouse = new Point(columns * tileSize, rows * tileSize);
                level = tempArray;
            }

            //FSM for EditorState
            switch (editorState)
            {
                case EditorState.Walls:

                    //Change state
                    if (IsValidKeypress(Keys.B))
                    {
                        Console.WriteLine("Entering B.O.W. editor");
                        editorState = EditorState.bulletOnlyWalls;
                    }
                    if (IsValidKeypress(Keys.M))
                    {
                        Console.WriteLine("Entering Mirror editor");
                        editorState = EditorState.Mirrors;
                    }
                    if (IsValidKeypress(Keys.E))
                    {
                        Console.WriteLine("Entering Ensconcing Enemy editor");
                        editorState = EditorState.Ensconcing;
                    }
                    if (IsValidKeypress(Keys.H))
                    {
                        Console.WriteLine("Entering Homing Enemy editor");
                        editorState = EditorState.Homing;
                    }
                    if (IsValidKeypress(Keys.P))
                    {
                        Console.WriteLine("Entering Player editor");
                        editorState = EditorState.Player;
                    }
                    HandleMouseInput('W');
                    break;

                case EditorState.bulletOnlyWalls:

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
                        Console.WriteLine("Entering Ensconcing Enemy editor");
                        editorState = EditorState.Ensconcing;
                    }
                    if (IsValidKeypress(Keys.H))
                    {
                        Console.WriteLine("Entering Homing Enemy editor");
                        editorState = EditorState.Homing;
                    }
                    if (IsValidKeypress(Keys.P))
                    {
                        Console.WriteLine("Entering Player editor");
                        editorState = EditorState.Player;
                    }
                    HandleMouseInput('B');
                    break;

                case EditorState.Mirrors:

                    //Change state
                    if (IsValidKeypress(Keys.W))
                    {
                        Console.WriteLine("Entering Wall editor");
                        editorState = EditorState.Walls;
                    }
                    if (IsValidKeypress(Keys.B))
                    {
                        Console.WriteLine("Entering B.O.W. editor");
                        editorState = EditorState.bulletOnlyWalls;
                    }
                    if (IsValidKeypress(Keys.E))
                    {
                        Console.WriteLine("Entering Ensconcing Enemy editor");
                        editorState = EditorState.Ensconcing;
                    }
                    if (IsValidKeypress(Keys.H))
                    {
                        Console.WriteLine("Entering Homing Enemy editor");
                        editorState = EditorState.Homing;
                    }
                    if (IsValidKeypress(Keys.P))
                    {
                        Console.WriteLine("Entering Player editor");
                        editorState = EditorState.Player;
                    }
                    HandleMouseInput('M');
                    break;

                case EditorState.Ensconcing:

                    //Change state
                    if (IsValidKeypress(Keys.W))
                    {
                        Console.WriteLine("Entering Wall editor");
                        editorState = EditorState.Walls;
                    }
                    if (IsValidKeypress(Keys.B))
                    {
                        Console.WriteLine("Entering B.O.W. editor");
                        editorState = EditorState.bulletOnlyWalls;
                    }
                    if (IsValidKeypress(Keys.H))
                    {
                        Console.WriteLine("Entering Homing Enemy editor");
                        editorState = EditorState.Homing;
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

                case EditorState.Homing:

                    //Change state
                    if (IsValidKeypress(Keys.W))
                    {
                        Console.WriteLine("Entering Wall editor");
                        editorState = EditorState.Walls;
                    }
                    if (IsValidKeypress(Keys.B))
                    {
                        Console.WriteLine("Entering B.O.W. editor");
                        editorState = EditorState.bulletOnlyWalls;
                    }
                    if (IsValidKeypress(Keys.M))
                    {
                        Console.WriteLine("Entering Mirror editor");
                        editorState = EditorState.Mirrors;
                    }
                    if (IsValidKeypress(Keys.E))
                    {
                        Console.WriteLine("Entering Ensconcing Enemy editor");
                        editorState = EditorState.Ensconcing;
                    }
                    if (IsValidKeypress(Keys.P))
                    {
                        Console.WriteLine("Entering Player editor");
                        editorState = EditorState.Player;
                    }
                    HandleMouseInput('H');
                    break;

                case EditorState.Player:

                    //Change state
                    if (IsValidKeypress(Keys.W))
                    {
                        Console.WriteLine("Entering Wall editor");
                        editorState = EditorState.Walls;
                    }
                    if (IsValidKeypress(Keys.B))
                    {
                        Console.WriteLine("Entering B.O.W. editor");
                        editorState = EditorState.bulletOnlyWalls;
                    }
                    if (IsValidKeypress(Keys.M))
                    {
                        Console.WriteLine("Entering Mirror editor");
                        editorState = EditorState.Mirrors;
                    }
                    if (IsValidKeypress(Keys.E))
                    {
                        Console.WriteLine("Entering Ensconcing Enemy editor");
                        editorState = EditorState.Ensconcing;
                    }
                    if (IsValidKeypress(Keys.H))
                    {
                        Console.WriteLine("Entering Homing Enemy editor");
                        editorState = EditorState.Homing;
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
                    switch (level[i, j])
                    {
                        case 'W':
                            _spriteBatch.Draw(gameObjectTexture, new Rectangle(j * tileSize, i * tileSize, tileSize, tileSize), Color.White);
                            break;

                        case 'P':
                            _spriteBatch.Draw(gameObjectTexture, new Rectangle(j * tileSize, i * tileSize, tileSize, tileSize), Color.Purple);
                            break;

                        case 'B':
                            _spriteBatch.Draw(gameObjectTexture, new Rectangle(j * tileSize, i * tileSize, tileSize, tileSize), Color.Blue);
                            break;

                        case 'M':
                            _spriteBatch.Draw(gameObjectTexture, new Rectangle(j * tileSize, i * tileSize, tileSize, tileSize), Color.Green);
                            break;

                        case 'E':
                            _spriteBatch.Draw(gameObjectTexture, new Rectangle(j * tileSize, i * tileSize, tileSize, tileSize), Color.Orange);
                            break;
                        
                        case 'H':
                            _spriteBatch.Draw(gameObjectTexture, new Rectangle(j * tileSize, i * tileSize, tileSize, tileSize), Color.DarkRed);
                            break;
                    }

                    //Draw grid
                    if (hasGrid)
                    {
                        //For some reason this won't draw if the tileSize isn't 16, but everthing else does i feel like I'm going insane
                        _spriteBatch.Draw(gridTexture, new Rectangle(j * tileSize, i * tileSize, tileSize, tileSize), Color.White);
                    }
                }
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
