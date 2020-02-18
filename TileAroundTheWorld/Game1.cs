using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TileAroundTheWorld
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState kb, oldkb;
        MouseState mouseState, oldmouseState;
        Point mousePos;
        Random rand = new Random();
        SpriteFont customfont;

        Character[,] backgroundCharacterGrid;
        Character backgroundOutline;

        List<int> pathFindingMovementTracker = new List<int>(0);

        int gameClock = 0;
        int cooldownTimer = 0;

        int gridXDimension = 16;
        int gridYDimension = 12;

        int totalBlockingTiles = 0;
        int currentTileNum = 0;
        bool isShowingNum = false;

        int screenWidth, screenHeight;

        int startingSquareX = 0;
        int startingSquareY = 0;

        int choicesMadeSoFar = 0;
        int numChoicesToMake = 30;

        int pathFindXBefore = 0;
        int pathFindYBefore = 0;

        bool valueChanged = false;

        bool hasMovedYet = false;

        bool shouldReset = false;

        bool hasWon = false;

        bool shouldShowNeighbors = false;

        int playerPosX = 0;
        int playerPosY = 0;

        int pathFindX = 0;
        int pathFindY = 0;

        int randomDecider = 0;

        bool isEditModeEnabled = false;

        bool hasDoneOneTimeCode = false;

        bool isShowingAnswer = false;
        bool shouldDraw = true;

        List<int> neededBlockingTileXCoords = new List<int>(0);
        List<int> neededBlockingTileYCoords = new List<int>(0);



        int directionDecider = 0;
        int tempDecider = 0;
        bool hasChosenStartingAndEnding = false;

        int oldPathFindingDirection = 0;

        int showPuzzleCooldown = 0;

        bool hasReset = false;


        #region gamestateThings

        enum gameState
        {

            titleScreen, gamePlay, options

        }


        gameState state = gameState.gamePlay;

        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            this.graphics.PreferredBackBufferWidth = 1000;
            this.graphics.PreferredBackBufferHeight = 720;

            screenWidth = this.graphics.PreferredBackBufferWidth;
            screenHeight = this.graphics.PreferredBackBufferHeight;

            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            customfont = Content.Load<SpriteFont>("customfont");

            backgroundCharacterGrid = new Character[gridXDimension, gridYDimension];

            for (int y = 0; y < gridYDimension; y++)
            {
                for (int x = 0; x < gridXDimension; x++)
                {
                    backgroundCharacterGrid[x, y] = new Character(Content.Load<Texture2D>("buttonOutline"),
                        new Rectangle(x * (screenWidth / gridXDimension), y * (screenHeight / gridYDimension), screenWidth / gridXDimension, screenHeight / gridYDimension));


                }

            }

            backgroundOutline = new Character(Content.Load<Texture2D>("buttonOutline"), new Rectangle(0, 0, screenWidth, screenHeight));


        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            kb = Keyboard.GetState();
            mouseState = Mouse.GetState();
            mousePos = new Point(mouseState.X, mouseState.Y);



            switch (state)
            {

                case gameState.titleScreen:
                    titleScreen();
                    break;
                case gameState.gamePlay:

                    gamePlay(gameTime);
                    break;
                case gameState.options:

                    options();

                    break;

            }
            //changeColor();
            oldkb = kb;
            oldmouseState = mouseState;
            base.Update(gameTime);
        }

        private void titleScreen()
        {


        }

        private void options()
        {


        }

        private void gamePlay(GameTime gameTime)
        {
            oneTimeCode();
            userControls();
            checkWins();
            //fixSmallGrid();
            gameClock++;
        }

        private void userControls()
        {
            if (kb.IsKeyDown(Keys.LeftShift) && kb.IsKeyDown(Keys.R) && oldkb.IsKeyUp(Keys.R))
            {

                resetGrid();
            }

            if (kb.IsKeyDown(Keys.T) && oldkb.IsKeyUp(Keys.T))
            {
                shouldShowNeighbors = !shouldShowNeighbors;
            }

            if (kb.IsKeyDown(Keys.N) && oldkb.IsKeyUp(Keys.N))
            {
                isShowingNum = !isShowingNum;
            }

            if (kb.IsKeyDown(Keys.C) && oldkb.IsKeyUp(Keys.C))
            {
                restartCurrentLevel();
            }

            if (kb.IsKeyDown(Keys.H) && oldkb.IsKeyUp(Keys.H))
            {
                if (isShowingAnswer == true)
                {
                    isShowingAnswer = false;

                }
                else
                {
                    isShowingAnswer = true;
                }
            }

            if ((kb.IsKeyDown(Keys.W) && oldkb.IsKeyUp(Keys.W)) || (kb.IsKeyDown(Keys.Up) && oldkb.IsKeyUp(Keys.Up)) || ((kb.IsKeyDown(Keys.W)
            || kb.IsKeyDown(Keys.Up)) && kb.IsKeyDown(Keys.LeftShift)))
            {
                if (playerPosY - 1 >= 0 && backgroundCharacterGrid[playerPosX, playerPosY - 1].getIsBlockingTile() == false && backgroundCharacterGrid[playerPosX, playerPosY - 1].getHasBeenTouched() == false)
                {
                    playerPosY -= 1;
                    backgroundCharacterGrid[playerPosX, playerPosY].setHasBeenTouched(true);
                }
            }
            else if ((kb.IsKeyDown(Keys.D) && oldkb.IsKeyUp(Keys.D)) || (kb.IsKeyDown(Keys.Right) && oldkb.IsKeyUp(Keys.Right)) || ((kb.IsKeyDown(Keys.D)
            || kb.IsKeyDown(Keys.Right)) && kb.IsKeyDown(Keys.LeftShift)))
            {
                if (playerPosX + 1 < gridXDimension && backgroundCharacterGrid[playerPosX + 1, playerPosY].getIsBlockingTile() == false && backgroundCharacterGrid[playerPosX + 1, playerPosY].getHasBeenTouched() == false)
                {
                    playerPosX += 1;
                    backgroundCharacterGrid[playerPosX, playerPosY].setHasBeenTouched(true);
                }
            }
            else if ((kb.IsKeyDown(Keys.S) && oldkb.IsKeyUp(Keys.S)) || (kb.IsKeyDown(Keys.Down) && oldkb.IsKeyUp(Keys.Down)) || ((kb.IsKeyDown(Keys.S)
            || kb.IsKeyDown(Keys.Down)) && kb.IsKeyDown(Keys.LeftShift)))
            {
                if (playerPosY + 1 < gridYDimension && backgroundCharacterGrid[playerPosX, playerPosY + 1].getIsBlockingTile() == false && backgroundCharacterGrid[playerPosX, playerPosY + 1].getHasBeenTouched() == false)
                {
                    playerPosY += 1;
                    backgroundCharacterGrid[playerPosX, playerPosY].setHasBeenTouched(true);
                }
            }
            else if ((kb.IsKeyDown(Keys.A) && oldkb.IsKeyUp(Keys.A)) || (kb.IsKeyDown(Keys.Left) && oldkb.IsKeyUp(Keys.Left)) || ((kb.IsKeyDown(Keys.A)
            || kb.IsKeyDown(Keys.Left)) && kb.IsKeyDown(Keys.LeftShift)))
            {
                if (playerPosX - 1 >= 0 && backgroundCharacterGrid[playerPosX - 1, playerPosY].getIsBlockingTile() == false && backgroundCharacterGrid[playerPosX - 1, playerPosY].getHasBeenTouched() == false)
                {
                    playerPosX -= 1;
                    backgroundCharacterGrid[playerPosX, playerPosY].setHasBeenTouched(true);
                }
            }



            if ((kb.IsKeyDown(Keys.I) && oldkb.IsKeyUp(Keys.I)))
            {
                moveUp();
            }
            else if (kb.IsKeyDown(Keys.L) && oldkb.IsKeyUp(Keys.L))
            {
                moveRight();
            }
            else if ((kb.IsKeyDown(Keys.K) && oldkb.IsKeyUp(Keys.K)))
            {
                moveDown();
            }
            else if ((kb.IsKeyDown(Keys.J) && oldkb.IsKeyUp(Keys.J)))
            {
                
            }


            //remove later
            if (kb.IsKeyDown(Keys.V) && oldkb.IsKeyUp(Keys.V))
            {
                makeUTurnDown();
            }

            if (kb.IsKeyDown(Keys.B) && oldkb.IsKeyUp(Keys.B))
            {
                makeUTurnUp();
            }

            if (kb.IsKeyDown(Keys.N) && oldkb.IsKeyUp(Keys.N))
            {
                makeUTurnLeft();
            }

            if (kb.IsKeyDown(Keys.M) && oldkb.IsKeyUp(Keys.M))
            {
                makeUTurnRight();
            }


        }

        private void restartCurrentLevel()
        {
            for (int y = 0; y < gridYDimension; y++)
            {
                for (int x = 0; x < gridXDimension; x++)
                {
                    backgroundCharacterGrid[x, y].setHasBeenTouched(false);
                }

            }
            backgroundCharacterGrid[startingSquareX, startingSquareY].setHasBeenTouched(true);

            playerPosX = startingSquareX;
            playerPosY = startingSquareY;
        }

        private void oneTimeCode()
        {
            while(hasDoneOneTimeCode == false)
            {
                hasDoneOneTimeCode = true;
                playerPosX = startingSquareX;
                playerPosY = startingSquareY;

                pathFindX = startingSquareX;
                pathFindY = startingSquareY;

                backgroundCharacterGrid[startingSquareX, startingSquareY].setHasBeenTouched(true);

                currentTileNum = 0;
                //placeRandomBlocks();

                //checkImpossible();

                //countSurrounding();

                choicesMadeSoFar = 0;

                pathDecider();

                placeBlockingTiles();

                fixSmallGrid();
                
            }

                shouldDraw = true;
            
        }

        private void checkWins()
        {
            for (int y = 0; y < gridYDimension; y++)
            {
                for (int x = 0; x < gridXDimension; x++)
                {
                    if (backgroundCharacterGrid[x, y].getIsBlockingTile() || backgroundCharacterGrid[x, y].getHasBeenTouched())
                    {
                        if (y == gridYDimension - 1 && x == gridXDimension - 1)
                        {
                            //hasWon = true;

                            resetGrid();
                        }
                    }
                    else
                    {
                        x = gridXDimension;
                        y = gridYDimension;
                        break;
                    }



                }

            }
        }

        private void countSurrounding()
        {
            for (int i = 0; i < backgroundCharacterGrid.GetLength(0); i++)
            {
                //nested for loop for 2nd dimension
                for (int j = 0; j < backgroundCharacterGrid.GetLength(1); j++)
                {
                    //backgroundCharacterGrid[i, j].addOneToSurrounding();
                    //cardinal directions
                    backgroundCharacterGrid[i, j].setNumSurrounding(0);
                    if (i + 1 != backgroundCharacterGrid.GetLength(0) && backgroundCharacterGrid[i + 1, j].getIsBlockingTile() == true)
                    {
                        backgroundCharacterGrid[i, j].addOneToSurrounding();
                    }
                    if (i - 1 >= 0 && backgroundCharacterGrid[i - 1, j].getIsBlockingTile() == true)
                    {
                        backgroundCharacterGrid[i, j].addOneToSurrounding();
                    }
                    if (j - 1 >= 0 && backgroundCharacterGrid[i, j - 1].getIsBlockingTile() == true)
                    {
                        backgroundCharacterGrid[i, j].addOneToSurrounding();
                    }
                    if (j + 1 != backgroundCharacterGrid.GetLength(1) && backgroundCharacterGrid[i, j + 1].getIsBlockingTile() == true)
                    {
                        backgroundCharacterGrid[i, j].addOneToSurrounding();
                    }


                }

            }

            fixImpossible();



        }

        private void fixSmallGrid()
        {
            if (totalBlockingTiles > (gridXDimension * gridYDimension) / 2 && totalBlockingTiles !=  0)
            {
                resetGrid();
            }
        }

        private void placeBlockingTiles()
        {
            totalBlockingTiles = 0;
            for (int i = 0; i < backgroundCharacterGrid.GetLength(0); i++)
            {
                //nested for loop for 2nd dimension
                for (int j = 0; j < backgroundCharacterGrid.GetLength(1); j++)
                {
                    if(backgroundCharacterGrid[i,j].getHasBeenTouchedByPathFind() == false && (i != 0 || j != 0))
                    {
                        backgroundCharacterGrid[i, j].setIsBlockingTile(true);
                        totalBlockingTiles += 1;
                    }
                }
            }


        }

        private void fixImpossible()
        {

            for (int i = 0; i < backgroundCharacterGrid.GetLength(0); i++)
            {
                //nested for loop for 2nd dimension
                for (int j = 0; j < backgroundCharacterGrid.GetLength(1); j++)
                {
                    //backgroundCharacterGrid[i, j].addOneToSurrounding();
                    //cardinal directions
                    if (backgroundCharacterGrid[i, j].getIsBlockingTile() == false &&
                        (backgroundCharacterGrid[i, j].getNumSurrounding() == 3 || backgroundCharacterGrid[i, j].getNumSurrounding() == 4))
                    {
                        backgroundCharacterGrid[i, j].setIsBlockingTile(true);
                        valueChanged = true;
                    }


                }

            }

            if (valueChanged)
            {
                valueChanged = false;
                countSurrounding();
            }

        }

        private void checkImpossible()
        {
            if (backgroundCharacterGrid[gridXDimension - 2, gridYDimension - 2].getIsBlockingTile() == false &&
                backgroundCharacterGrid[gridXDimension - 3, gridYDimension - 2].getIsBlockingTile() == true &&
                backgroundCharacterGrid[gridXDimension - 2, gridYDimension - 3].getIsBlockingTile() == true)
            {
                backgroundCharacterGrid[gridXDimension - 1, gridYDimension - 1].setIsBlockingTile(true);
            }

            //for (int y = 1; y < gridYDimension - 1; y++)
            //{
            //    for (int x = 1; x < gridXDimension - 1; x++)
            //    {
            //        if(backgroundCharacterGrid[x + 1, y].getIsBlockingTile() == true &&
            //            backgroundCharacterGrid[x, y + 1].getIsBlockingTile() == true &&
            //            backgroundCharacterGrid[x - 1, y].getIsBlockingTile() == true &&
            //            backgroundCharacterGrid[x, y - 1].getIsBlockingTile() == true)
            //        {
            //            backgroundCharacterGrid[x, y].setIsBlockingTile(true);
            //        }
            //    }

            //}
        }

        private void pathDecider()
        {
            countSurrounding();
           while(choicesMadeSoFar < numChoicesToMake)
            {
                choiceMaker();
                countSurrounding();
            }
        }

        private void choiceMaker()
        {
            pathFindXBefore = pathFindX;
            pathFindYBefore = pathFindY;

            randomDecider = rand.Next(1, 7);

            if (randomDecider == 1)
            {
                makeUTurnUp();
            }
            else if (randomDecider == 2)
            {
                makeUTurnRight();
            }
            else if (randomDecider == 3)
            {
                makeUTurnDown();
            }
            else if (randomDecider == 4)
            {
                makeUTurnLeft();
            }
            else if (randomDecider == 5)
            {
                zigzagRightDown();
            }

            if (randomDecider == 6)
            {
                randomDecider = rand.Next(5, 9);
                int temp = rand.Next(1, 4);

                if (randomDecider == 5)
                {
                    moveUp(temp);
                }
                else if (randomDecider == 6)
                {
                    moveRight(temp);
                }
                else if (randomDecider == 7)
                {
                    moveDown(temp);
                }
                else if (randomDecider == 8)
                {
                    moveLeft(temp);
                }
            }

            //if (pathFindX != pathFindXBefore || pathFindY != pathFindYBefore)
            //{
            //    choicesMadeSoFar += 1;
            //}

                choicesMadeSoFar += 1;


        }

        private void resetGrid()
        {
            
            //shouldDraw = false;
            for (int y = 0; y < gridYDimension; y++)
            {
                for (int x = 0; x < gridXDimension; x++)
                {
                    backgroundCharacterGrid[x, y].setIsBlockingTile(false);
                    backgroundCharacterGrid[x, y].setHasBeenTouched(false);
                    backgroundCharacterGrid[x, y].setNumSurrounding(0);
                    backgroundCharacterGrid[x, y].setHasBeenTouchedByPathFinder(false);
                    backgroundCharacterGrid[x, y].setIsBlockingTile(false);
                }

            }


            hasDoneOneTimeCode = false;

        }

        private void makeUTurnUp()
        {
            if (pathFindY > 0)
            {
                randomDecider = rand.Next(1, pathFindY);
            }
            else
            {
                return;
            }
            for (int i = 0; i < randomDecider; i++)
            {
                if (pathFindY - 1 >= 0 && backgroundCharacterGrid[pathFindX, pathFindY - 1].getHasBeenTouchedByPathFind() == false)
                {
                    pathFindY -= 1;
                    backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                    currentTileNum += 1;

                }
                else
                {
                    return;
                }

                backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);

            }

            if (pathFindX + 1 < gridXDimension && backgroundCharacterGrid[pathFindX + 1, pathFindY].getHasBeenTouchedByPathFind() == false)
            {
                pathFindX += 1;
                backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);
                backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                currentTileNum += 1;

                for (int i = 0; i < randomDecider; i++)
                {
                    if (pathFindY + 1 < gridYDimension && backgroundCharacterGrid[pathFindX, pathFindY + 1].getHasBeenTouchedByPathFind() == false)
                    {
                        pathFindY += 1;
                        backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                        currentTileNum += 1;

                    }
                    else
                    {
                        return;
                    }
                    backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);

                }
            }

            moveRight();


        }

        private void makeUTurnDown()
        {
            randomDecider = rand.Next(1, gridYDimension - pathFindY);

            for (int i = 0; i < randomDecider; i++)
            {
                if (pathFindY + 1 < gridYDimension && backgroundCharacterGrid[pathFindX, pathFindY + 1].getHasBeenTouchedByPathFind() == false)
                {
                    pathFindY += 1;
                    backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                    currentTileNum += 1;

                }
                else
                {
                    return;
                }

                backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);
            }

            //pathFindX += 1;
            //backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);

            if (pathFindX + 1 < gridXDimension && backgroundCharacterGrid[pathFindX + 1, pathFindY].getHasBeenTouchedByPathFind() == false)
            {
                pathFindX += 1;
                backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);
                backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                currentTileNum += 1;

                for (int i = 0; i < randomDecider; i++)
                {
                    if (pathFindY - 1 >= 0 && backgroundCharacterGrid[pathFindX, pathFindY - 1].getHasBeenTouchedByPathFind() == false)
                    {
                        pathFindY -= 1;
                        backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                        currentTileNum += 1;

                    }
                    else
                    {
                        return;
                    }
                    backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);

                }
            }
          
            moveRight();



        }

        private void makeUTurnLeft()
        {
            if (pathFindX > 0)
            {
                randomDecider = rand.Next(1, pathFindX);
            }
            else
            {
                return;
            }
            for (int i = 0; i < randomDecider; i++)
            {
                if (pathFindX - 1 >= 0 && backgroundCharacterGrid[pathFindX - 1, pathFindY].getHasBeenTouchedByPathFind() == false)
                {
                    pathFindX -= 1;
                    backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                    currentTileNum += 1;
                }
                else
                {
                    return;
                }

                backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);

            }

            if (pathFindY + 1 < gridYDimension && backgroundCharacterGrid[pathFindX, pathFindY + 1].getHasBeenTouchedByPathFind() == false)
            {
                pathFindY += 1;
                backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);
                backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                currentTileNum += 1;

                for (int i = 0; i < randomDecider; i++)
                {
                    if (pathFindX + 1 < gridXDimension && backgroundCharacterGrid[pathFindX + 1, pathFindY].getHasBeenTouchedByPathFind() == false)
                    {
                        pathFindX += 1;
                        backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                        currentTileNum += 1;
                    }
                    else
                    {
                        return;
                    }
                    backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);

                }
            }

            moveDown();
        }

        private void makeUTurnRight()
        {
            if (pathFindX > 0)
            {
                randomDecider = rand.Next(1, pathFindX);
            }
            else
            {
                return;
            }
            for (int i = 0; i < randomDecider; i++)
            {
                if (pathFindX + 1 < gridYDimension && backgroundCharacterGrid[pathFindX + 1, pathFindY].getHasBeenTouchedByPathFind() == false)
                {
                    pathFindX += 1;
                    backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                    currentTileNum += 1;
                }
                else
                {
                    return;
                }

                backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);
            }

            //pathFindX += 1;
            //backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);

            if (pathFindY + 1 < gridYDimension && backgroundCharacterGrid[pathFindX, pathFindY + 1].getHasBeenTouchedByPathFind() == false)
            {
                pathFindY += 1;
                backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);
                backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                currentTileNum += 1;

                for (int i = 0; i < randomDecider; i++)
                {
                    if (pathFindX - 1 >= 0 && backgroundCharacterGrid[pathFindX - 1, pathFindY].getHasBeenTouchedByPathFind() == false)
                    {
                        pathFindX -= 1;
                        backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                        currentTileNum += 1;
                    }
                    else
                    {
                        return;
                    }
                    backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);

                }
            }

            moveDown();

        }

        private void zigzagRightDown()
        {
            moveRight();
            moveDown();
        }

        private void moveRight()
        {
            if (pathFindX + 1 < gridXDimension && backgroundCharacterGrid[pathFindX + 1, pathFindY].getHasBeenTouchedByPathFind() == false)
            {
                pathFindX += 1;
                backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);
                backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                currentTileNum += 1;


            }
        }

        private void moveUp()
        {
            if (pathFindY - 1 >= 0 && backgroundCharacterGrid[pathFindX, pathFindY - 1].getIsBlockingTile() == false && backgroundCharacterGrid[pathFindX, pathFindY - 1].getHasBeenTouchedByPathFind() == false)
            {
                pathFindY -= 1;
                backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);
                backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                currentTileNum += 1;
            }
        }

        private void moveDown()
        {
            if (pathFindY + 1 < gridYDimension && backgroundCharacterGrid[pathFindX, pathFindY + 1].getIsBlockingTile() == false && backgroundCharacterGrid[pathFindX, pathFindY + 1].getHasBeenTouchedByPathFind() == false)
            {
                pathFindY += 1;
                backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);
                backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                currentTileNum += 1;
            }
        }

        private void moveLeft()
        {
            if (pathFindX - 1 >= 0 && backgroundCharacterGrid[pathFindX - 1, pathFindY].getIsBlockingTile() == false && backgroundCharacterGrid[pathFindX - 1, pathFindY].getHasBeenTouchedByPathFind() == false)
            {
                pathFindX -= 1;
                backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);
                backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                currentTileNum += 1;
            }
        }

        private void moveRight(int timesToRun)
        {
            for (int i = 0; i < timesToRun; i++)
            {
                if (pathFindX + 1 < gridXDimension && backgroundCharacterGrid[pathFindX + 1, pathFindY].getHasBeenTouchedByPathFind() == false)
                {
                    pathFindX += 1;
                    backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);
                    backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                    currentTileNum += 1;

                }
            }
        }

        private void moveUp(int timesToRun)
        {
            for (int i = 0; i < timesToRun; i++)
            {
                if (pathFindY - 1 >= 0 && backgroundCharacterGrid[pathFindX, pathFindY - 1].getIsBlockingTile() == false && backgroundCharacterGrid[pathFindX, pathFindY - 1].getHasBeenTouchedByPathFind() == false)
                {
                    pathFindY -= 1;
                    backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);
                    backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                    currentTileNum += 1;
                }
            }
        }

        private void moveDown(int timesToRun)
        {
            for (int i = 0; i < timesToRun; i++)
            {
                if (pathFindY + 1 < gridYDimension && backgroundCharacterGrid[pathFindX, pathFindY + 1].getIsBlockingTile() == false && backgroundCharacterGrid[pathFindX, pathFindY + 1].getHasBeenTouchedByPathFind() == false)
                {
                    pathFindY += 1;
                    backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);
                    backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                    currentTileNum += 1;
                }
            }
        }

        private void moveLeft(int timesToRun)
        {
            for (int i = 0; i < timesToRun; i++)
            {
                if (pathFindX - 1 >= 0 && backgroundCharacterGrid[pathFindX - 1, pathFindY].getIsBlockingTile() == false && backgroundCharacterGrid[pathFindX - 1, pathFindY].getHasBeenTouchedByPathFind() == false)
                {
                    pathFindX -= 1;
                    backgroundCharacterGrid[pathFindX, pathFindY].setHasBeenTouchedByPathFinder(true);
                    backgroundCharacterGrid[pathFindX, pathFindY].setTileNum(currentTileNum);
                    currentTileNum += 1;
                }
            }
        }

        private void placeOnSide(int sideNum)
        {


        }

        private void drawTitleScreen()
        {


        }

        private void drawGamePlay(GameTime gameTime)
        {
            //backgroundOutline.DrawCharacter(spriteBatch);
            //backgroundCharacterGrid[playerPosX, playerPosY].DrawCharacter(spriteBatch);
            //if (shouldDraw)
            //{
                for (int y = 0; y < gridYDimension; y++)
                {
                    for (int x = 0; x < gridXDimension; x++)
                    {
                        if (backgroundCharacterGrid[x, y].getIsBlockingTile())
                        {
                            backgroundCharacterGrid[x, y].DrawCharacter(spriteBatch, Color.Black);
                        }
                        else if (backgroundCharacterGrid[x, y].getHasBeenTouched())
                        {
                            backgroundCharacterGrid[x, y].DrawCharacter(spriteBatch, Color.LightBlue);
                        }
                        else if (backgroundCharacterGrid[x, y].getHasBeenTouchedByPathFind())
                        {
                        
                        //backgroundCharacterGrid[x, y].getTileNum();

                        backgroundCharacterGrid[x, y].DrawCharacter(spriteBatch, Color.DarkCyan);
                        spriteBatch.DrawString(customfont, backgroundCharacterGrid[x, y].getTileNum().ToString(),
                                new Vector2(backgroundCharacterGrid[x, y].GetRekt.Center.X, backgroundCharacterGrid[x, y].GetRekt.Center.Y), Color.Red);
                    }
                        else
                        {
                            backgroundCharacterGrid[x, y].DrawCharacter(spriteBatch);
                        }

                        if (shouldShowNeighbors)
                        {
                            spriteBatch.DrawString(customfont, backgroundCharacterGrid[x, y].getNumSurrounding().ToString(),
                                new Vector2(backgroundCharacterGrid[x, y].GetRekt.Center.X, backgroundCharacterGrid[x, y].GetRekt.Center.Y), Color.Red);

                        }
                    }

               // }
            }
            backgroundCharacterGrid[playerPosX, playerPosY].DrawCharacter(spriteBatch, Color.Red);

            spriteBatch.DrawString(customfont, "blockingTiles: " + totalBlockingTiles, new Vector2(400, 350), Color.White);
            spriteBatch.DrawString(customfont, "totalTles: " + gridYDimension * gridXDimension, new Vector2(400, 380), Color.White);
            //spriteBatch.DrawString(customfont, "playerPosY: " + playerPosY, new Vector2(400, 370), Color.Blue);

            //spriteBatch.DrawString(customfont, "endingX: " + endingSquareX, new Vector2(400, 350), Color.Blue);
            //spriteBatch.DrawString(customfont, "endingY: " + endingSquareY, new Vector2(400, 370), Color.Blue);

            //spriteBatch.DrawString(customfont, "startingSquareX: " + startingSquareX, new Vector2(480, 350), Color.Blue);
            //spriteBatch.DrawString(customfont, "startingSquareY: " + startingSquareY, new Vector2(480, 370), Color.Blue);


        }

        private void drawOptionsScreen()
        {


        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here


            spriteBatch.Begin();

            switch (state)
            {

                case gameState.titleScreen:
                    drawTitleScreen();
                    break;
                case gameState.gamePlay:

                    drawGamePlay(gameTime);
                    break;

                case gameState.options:

                    drawOptionsScreen();

                    break;

            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
