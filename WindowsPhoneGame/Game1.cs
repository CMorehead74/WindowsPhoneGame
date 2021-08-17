using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace WindowsPhoneGame1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState prevKeyboard;
        SpriteFont font;
        float gameRunningTime = 0.0f;   //when how long game has run, when im not paused
        List<String> menuOptions = new List<String>();
        int selectedMenuOption = 0;
        Vector2 slantDirection = new Vector2(0.0f, 1.0f);

        bool isPaused = false;
        public Game1()
        {

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            menuOptions.Add("PLAY");
            menuOptions.Add("HELP & OPTIONS");
            menuOptions.Add("LEADERBOARD");
            menuOptions.Add("BACK TO ARCADE");

            base.Initialize();

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            font = Content.Load<SpriteFont>("SpriteFont1");
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            if (!isPaused)
            this.gameRunningTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            base.Update(gameTime);

            if (Keyboard.GetState().IsKeyUp(Keys.P))
                isPaused = !isPaused;

            prevKeyboard = Keyboard.GetState();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        void DrawShadowed(SpriteBatch spriteBatch, SpriteFont font, String text, Vector2 position, Vector2 origin, float scale, Color textColor, Color shadowColor)
        {
            Vector2 offset = new Vector2(2, 2);
            spriteBatch.DrawString(font, text, position + offset, shadowColor, 0.0f, origin, scale, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(font, text, position, shadowColor, 0.0f, origin, scale, SpriteEffects.None, 0.0f);
        }

        void DrawFontOutLined(SpriteBatch spriteBatch, SpriteFont font, String text, Vector2 position, Vector2 origin, float scale, Color textColor, Color shadowColor)
        {
            Vector2 offset = new Vector2(1, 1);
            spriteBatch.DrawString(font, text, position + new Vector2(1,1), shadowColor, 0.0f, origin, scale, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(font, text, position + new Vector2(-1, 1), shadowColor, 0.0f, origin, scale, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(font, text, position + new Vector2(1, -1), shadowColor, 0.0f, origin, scale, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(font, text, position + new Vector2(-1, -1), shadowColor, 0.0f, origin, scale, SpriteEffects.None, 0.0f);

        }

        protected override void Draw(GameTime gameTime)
        {


            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            // TODO: Add your drawing code here
            //Vector2 origin = font.MeasureString("Hello World!");
            //float scale = (float)Math.Abs(Math.Sin(gameRunningTime * 4) * 0.5f + 0.85f);
            //spriteBatch.DrawString(font, "HELLO WORLD!", new Vector2(400, 300), Color.White, 0.0f, origin /2, scale, SpriteEffects.None, 0.0f);

            spriteBatch.DrawString(font, gameRunningTime.ToString(), new Vector2(10,10), Color.White);

            Vector2 menuTopCenter = new Vector2(400, 300);
            float fontHeight = font.MeasureString("ANY").Y;

            for (int i = 0; i < menuOptions.Count; i++)
            {
                Vector2 origin = font.MeasureString(menuOptions[i]);

                float scale = 1.0f;
                if (selectedMenuOption ==i)
                            spriteBatch.DrawString(font, "HELLO WORLD!", new Vector2(400, 300), Color.White, 0.0f, origin /2, scale, SpriteEffects.None, 0.0f);

                spriteBatch.DrawString(font, menuOptions[i], menuTopCenter + slantDirection * i *(fontHeight + 5), Color.White);
                

            }
            spriteBatch.End();
            base.Draw(gameTime);
        }

    }
}
