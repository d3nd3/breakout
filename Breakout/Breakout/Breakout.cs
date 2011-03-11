using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
/// <summary>
/// This is main class encapsulating the game logic
/// for the Breakout game.
/// </summary>
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Breakout {
    public class Breakout {
        private SpriteBatch spriteBatch;
        private Paddle paddle;
        private Ball ball;
        private Wall wall;
        private int screenHeight;
        private int screenWidth;

        public Breakout(SpriteBatch spriteBatch) {
            this.spriteBatch = spriteBatch;
            this.screenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            this.screenHeight = spriteBatch.GraphicsDevice.Viewport.Height;
            this.paddle = new Paddle(screenWidth, screenHeight);
            this.ball = new Ball(paddle, screenWidth, screenHeight);
            this.wall = new Wall();
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
            ball.Draw(gameTime, spriteBatch);
            paddle.Draw(gameTime, spriteBatch);
            wall.Draw(gameTime, spriteBatch);
            spriteBatch.End();
        }

        internal void Update(GameTime gameTime) {
            paddle.Update(gameTime);
            ball.Update(gameTime);
        }
    }

   

    /// <summary
    /// Defines the ball used in a breakout game.
    /// </summary>
    class Ball {
        enum State {
            Active, Dead
        }

        private State state;
        private Paddle paddle;
        private int screenWidth, screenHeight;
        private Texture2D sprite;
        
        private Vector2 initialPosition;
        private Vector2 position;

        private Vector2 initialDirection = new Vector2(0, 1);
        // A unit vector for the direction of the ball
        private Vector2 direction = new Vector2(0, 1);

        // in pixels per second
        private const int initialSpeed = 150;
        private const int speedIncrement = 75;
        private const int maxSpeed = 1000;
        private int speed = initialSpeed;

        public Rectangle Bounds {
            get { return new Rectangle((int)position.X, (int)position.Y, sprite.Width, sprite.Height); }
        }

        public Ball(Paddle paddle, int screenWidth, int screenHeight) {
            this.state = State.Active;
            this.paddle = paddle;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            initialPosition = new Vector2(screenWidth / 2, screenHeight / 2);
        }

        internal void LoadContent(ContentManager contentManager) {
            sprite = contentManager.Load<Texture2D>("ball");
            position = initialPosition - new Vector2(sprite.Width / 2, sprite.Height / 2);
        }

        internal void Update(GameTime gameTime) {
            if (state == State.Active) {
                UpdatePosition(gameTime);
            } else if (state == State.Dead && Keyboard.GetState().IsKeyDown(Keys.Space)) {
                LaunchBall();
            }
        }

        private void LaunchBall() {
            state = State.Active;
            speed = initialSpeed;
            direction = initialDirection;
            position = initialPosition - new Vector2(sprite.Width / 2, sprite.Height / 2);
        }

        private void UpdatePosition(GameTime gameTime) {
            position = position + direction * (float)(speed * gameTime.ElapsedGameTime.TotalSeconds);

            if (position.Y > screenHeight) {
                state = State.Dead;
            } else {
                DetectCollisions();
            }
        }

        private void DetectCollisions() {
            if (position.Y < 0) {
                position.Y = 0;
                direction.Y = -direction.Y;
            }

            if (position.X < 0) {
                position.X = 0;
                direction.X = -direction.X;
            } else if (position.X + sprite.Width > screenWidth) {
                position.X = screenWidth - sprite.Height;
                direction.X = -direction.X;
            }

            if (direction.Y > 0) {
                if (paddle.Bounds.Intersects(this.Bounds)) {
                    direction.Y = -direction.Y;
                    position.Y = paddle.Bounds.Y - sprite.Height;

                    // figure out the new X direction based on distance from the paddle center
                    direction.X = (position.X - paddle.Center.X) / (paddle.Bounds.Width / 2);
                    direction = Vector2.Normalize(direction);

                    // Add a bit to the speed
                    speed += speedIncrement;
                    speed = Math.Min(speed, maxSpeed);
                }
            }
        }

        internal void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
            if (state == State.Active) {
                spriteBatch.Draw(sprite, position, Color.White);
            }
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
        const int INITIAL_SPEED = 300;
        // The factor by which the paddle's movement will
        // accelerate per second.
        Vector2 ACCELERATION = new Vector2(2000, 0);
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
            get { return new Rectangle((int)position.X, (int)position.Y, sprite.Width, PADDLE_HEIGHT); }
        }

        public Point Center {
            get { return new Point((int)position.X + sprite.Width / 2, (int)position.Y + sprite.Height / 2); }
        }

        /// <summary>
        /// Creates a new Paddle instance.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch used to draw the paddle.</param>
        public Paddle(int screenWidth, int screenHeight) {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.previousKeyboard = Keyboard.GetState();
        }

        /// <summary>
        /// Loads the paddle texture from the ContentManager.
        /// </summary>
        /// <param name="contentManager">The content manager from which to load the paddle texture</param>
        internal void LoadContent(ContentManager contentManager) {
            sprite = contentManager.Load<Texture2D>("paddle");

            // The initial position is centered on the X axis and PADDLE_OFFSET from the screen bottom
            int x = screenWidth - (screenWidth / 2) - (sprite.Width / 2);
            int y = screenHeight - PADDLE_OFFSET - PADDLE_HEIGHT;
            position = new Vector2(x, y);
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
                velocity = velocity + (-ACCELERATION * time);
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
        internal void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
            spriteBatch.Draw(sprite, position, source, Color.White);
        }
    }

    /// <summary>
    /// Defines the wall of bricks in a breakout game.
    /// </summary>
    class Wall {

        public Wall() {
        }

        internal void LoadContent(ContentManager contentManager) {
            // throw new NotImplementedException();
        }

        internal void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
            //throw new NotImplementedException();
        }
    }
}
