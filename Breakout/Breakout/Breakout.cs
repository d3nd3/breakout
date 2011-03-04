using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Breakout {
    /// <summary>
    /// This is main class encapsulating the game logic
    /// for the Breakout game.
    /// </summary>
    public class Breakout {
        private SpriteBatch spriteBatch;
        private Paddle paddle;
        private Ball ball;
        private Wall wall;

        public Breakout(SpriteBatch spriteBatch) {
            this.spriteBatch = spriteBatch;
            this.ball = new Ball(spriteBatch);
            this.paddle = new Paddle(spriteBatch);
            this.wall = new Wall(spriteBatch);
        }

        /// <summary>
        /// Loads the content for the Breakout game
        /// </summary>
        /// <param name="contentManager"></param>
        public void LoadContent(ContentManager contentManager) {
            ball.LoadContent(contentManager);
            paddle.LoadContent(contentManager);
            wall.LoadContent(contentManager);
        }

        internal void Draw(GameTime gameTime) {
            spriteBatch.Begin();
            ball.Draw(gameTime);
            paddle.Draw(gameTime);
            wall.Draw(gameTime);
            spriteBatch.End();
        }

        internal void Update(GameTime gameTime) {
            paddle.Update(gameTime);
        }
    }

    /// <summary
    /// Defines the ball used in a breakout game.
    /// </summary>
    class Ball {
        private SpriteBatch spriteBatch;

        public Ball(SpriteBatch spriteBatch) {
            this.spriteBatch = spriteBatch;
        }

        internal void LoadContent(ContentManager contentManager) {
            //throw new NotImplementedException();
        }

        internal void Draw(GameTime gameTime) {
            //throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Defines the Paddle used in a breakout game.
    /// 
    /// This is responsible for drawing the paddle using the paddle
    /// sprite and moving the paddle left and right based on user input.
    /// </summary>
    class Paddle {
        // The initial at which the paddle will start to move.
        const int INITIAL_SPEED = 400;
        // The factor by which the paddle's movement will
        // accelerate per second.
        Vector2 ACCELERATION = new Vector2(1600, 0);
        const int PADDLE_HEIGHT = 24;
        // How far of the bottom of the screen paddle is drawn
        const int PADDLE_OFFSET = 10;
        
        private Texture2D sprite;
        // The direction and speed at which the paddle is moving.
        private Vector2 velocity = Vector2.Zero;
        // The current position of the paddle
        private Vector2 position;
        // The paddle can't be further to the left of this
        private Vector2 minPosition;
        // The paddle can't be further to the right of this
        private Vector2 maxPosition;
        // The part of the sprite we should draw
        private Rectangle source;

        private SpriteBatch spriteBatch;
        private int screenHeight;
        private int screenWidth;
        private KeyboardState previousKeyboard;

        public Vector2 Position {
            get { return position; }
        }

        public Vector2 Velocity {
            get { return velocity; }
        }

        public Rectangle Bounds {
            get { return new Rectangle((int) position.X, (int) position.Y, sprite.Width, PADDLE_HEIGHT); }
        }

        /// <summary>
        /// Creates a new Paddle instance.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch used to draw the paddle.</param>
        public Paddle(SpriteBatch spriteBatch) {
            this.previousKeyboard = Keyboard.GetState();
            this.spriteBatch = spriteBatch;
            this.screenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            this.screenHeight = spriteBatch.GraphicsDevice.Viewport.Height;
        }

        /// <summary>
        /// Loads the paddle texture from the ContentManager.
        /// </summary>
        /// <param name="contentManager">The content manager from which to load the paddle texture</param>
        internal void LoadContent(ContentManager contentManager) {
            sprite = contentManager.Load<Texture2D>("paddle");

            // The initial position is centered on the X axis and PADDLE_OFFSET from the screen bottom
            position = new Vector2(screenWidth - (screenWidth / 2) - (sprite.Width / 2),
                                   screenHeight - PADDLE_OFFSET - PADDLE_HEIGHT);
            minPosition = new Vector2(0, position.Y);
            maxPosition = new Vector2(screenWidth - 1 - sprite.Width, position.Y);
            // The paddle texture is a sheet containing two textures, start with the first.
            source = new Rectangle(0, 0, sprite.Width, PADDLE_HEIGHT);
        }

        /// <summary>
        /// Updates the positon of the paddle based on the user input.
        /// </summary>
        /// <param name="gameTime"></param>
        /// TODO: This KeyBoard logic needs to be extracted into a Service.
        internal void Update(GameTime gameTime) {
            float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Left) && !previousKeyboard.IsKeyDown(Keys.Left)) {
                velocity = new Vector2(-INITIAL_SPEED, 0);
            } else if (keyboard.IsKeyDown(Keys.Right) && !previousKeyboard.IsKeyDown(Keys.Right)) {
                velocity = new Vector2(INITIAL_SPEED, 0);
            } else if (keyboard.IsKeyDown(Keys.Left) && previousKeyboard.IsKeyDown(Keys.Left)) {
                velocity = velocity - (ACCELERATION * time);
            } else if (keyboard.IsKeyDown(Keys.Right) && previousKeyboard.IsKeyDown(Keys.Right)) {
                velocity = velocity + (ACCELERATION * time);
            } else {
                velocity = Vector2.Zero;
            }

            position = position + (velocity * time);
            position = Vector2.Clamp(position, minPosition, maxPosition);
            
            previousKeyboard = keyboard;
        }

        /// <summary>
        /// Just draws the paddle at the position.
        /// </summary>
        /// <param name="gameTime"></param>
        internal void Draw(GameTime gameTime) {
            spriteBatch.Draw(sprite, position, source, Color.White);
        }
    }

    /// <summary>
    /// Defines the wall of bricks in a breakout game.
    /// </summary>
    class Wall {
        private SpriteBatch spriteBatch;

        public Wall(SpriteBatch spriteBatch) {
            this.spriteBatch = spriteBatch;
        }

        internal void LoadContent(ContentManager contentManager) {
           // throw new NotImplementedException();
        }

        internal void Draw(GameTime gameTime) {
            //throw new NotImplementedException();
        }
    }
}
