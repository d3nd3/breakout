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
            this.wall = new Wall(screenWidth, screenHeight);
            this.ball = new Ball(paddle, wall, screenWidth, screenHeight);
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
        /// <summary>
        /// The state of the ball
        /// </summary>
        enum State {
            /// <summary>
            /// Active, in the sense that the ball is moving on the board
            /// </summary>
            Active,
            /// <summary>
            /// Dead, in the sense that it has gone off the bottom of the screen
            /// </summary>
            Dead
        }

        // The current state of the ball
        private State state;

        // A reference to the paddle. This is used
        // to detect hits and determine the new direction
        private Paddle paddle;

        private Wall wall;

        // The screen width and height
        private int screenWidth, screenHeight;

        // The sprite we are drawing for the ball
        private Texture2D sprite;
        
        // The starting position of the ball
        private Vector2 initialPosition;

        private Vector2 lastPosition;
        // The current position of the ball
        private Vector2 position;

        // The starting direction of the ball, just straight down for now
        private Vector2 initialDirection = new Vector2(0, 1);
        // A unit vector for the direction of the ball
        private Vector2 direction = new Vector2(0, 1);

        // in pixels per second
        private const int initialSpeed = 150;
        private const int speedIncrement = 75;
        private const int maxSpeed = 750;
        private int speed = initialSpeed;

        /// <summary>
        /// The bounding rectangle of the ball
        /// </summary>
        public Rectangle Bounds {
            get { return new Rectangle((int)position.X, (int)position.Y, sprite.Width, sprite.Height); }
        }

        /// <summary>
        /// Creates a new ball. The breakout game should only need once of these,
        /// unless I make it so more than one ball is active at a time.
        /// </summary>
        /// <param name="paddle">The Paddle which will hit the ball</param>
        /// <param name="screenWidth">Width of the screen for bounds checking.</param>
        /// <param name="screenHeight">Height of the screen for bounds checking.</param>
        public Ball(Paddle paddle, Wall wall, int screenWidth, int screenHeight) {
            this.state = State.Active;
            this.paddle = paddle;
            this.wall = wall;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            initialPosition = new Vector2(screenWidth / 2, screenHeight / 2);
        }

        /// <summary>
        /// Load the sprite for the ball.
        /// </summary>
        /// <param name="contentManager"></param>
        internal void LoadContent(ContentManager contentManager) {
            sprite = contentManager.Load<Texture2D>("ball");
            position = initialPosition - new Vector2(sprite.Width / 2, sprite.Height / 2);
        }

        /// <summary>
        /// Update the state of the ball. 
        /// 
        /// If the ball is active it is moved, otherwise
        /// we check if the ball is being launched.
        /// </summary>
        /// <param name="gameTime"></param>
        internal void Update(GameTime gameTime) {
            if (state == State.Active) {
                UpdatePosition(gameTime);

                if (position.Y > screenHeight) {
                    state = State.Dead;
                } else {
                    HandleCollisions();
                }
            } else if (state == State.Dead && Keyboard.GetState().IsKeyDown(Keys.Space)) {
                LaunchBall();
            }
        }

        /// <summary>
        /// Launch the ball by setting it's initial state.
        /// </summary>
        private void LaunchBall() {
            state = State.Active;
            speed = initialSpeed;
            direction = initialDirection;
            position = initialPosition - new Vector2(sprite.Width / 2, sprite.Height / 2);
        }

        private void UpdatePosition(GameTime gameTime) {
            lastPosition = position;
            position = position + direction * (float)(speed * gameTime.ElapsedGameTime.TotalSeconds);
        }

        private void HandleCollisions() {
            HandleBoardCollisions();
            HandlePaddleCollision();
            HandlWallCollision();
        }

        private void HandleBoardCollisions() {
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
        }

        private void HandlePaddleCollision() {
            if (direction.Y > 0 && paddle.Bounds.Intersects(this.Bounds)) {
                direction.Y = -direction.Y;
                position.Y = paddle.Bounds.Y - sprite.Height;

                // figure out the new X direction based on distance from the paddle center
                direction.X = ((float)Bounds.Center.X - paddle.Bounds.Center.X) / (paddle.Bounds.Width / 2);
                direction = Vector2.Normalize(direction);

                // Increase the speed when the ball is hit
                speed += speedIncrement;
                speed = Math.Min(speed, maxSpeed);
            }
        }

        private void HandlWallCollision() {
            Vector2 centerTop = new Vector2(Bounds.Width / 2, 0);
            Vector2 centerLeft = new Vector2(0, Bounds.Height / 2);
            Vector2 centerRight = new Vector2(Bounds.Width, Bounds.Height / 2);
            Vector2 centerBottom = new Vector2(Bounds.Width / 2, Bounds.Height);
            Vector2 newDirection = new Vector2(direction.X, direction.Y);
            List<Rectangle> destroyed = wall.DestroyBricksAt(this.Bounds);

            foreach (Rectangle brick in destroyed) {
                bool changed = false;
                Vector2 topLeft = new Vector2(brick.Left, brick.Top);
                Vector2 topRight = new Vector2(brick.Right, brick.Top);
                Vector2 bottomLeft = new Vector2(brick.Left, brick.Bottom);
                Vector2 bottomRight = new Vector2(brick.Right, brick.Bottom);

                if (direction.X > 0 && LineIntersects(lastPosition + centerRight, position + centerRight, topLeft, bottomLeft)) {
                   newDirection.X *= -1;
                   Console.WriteLine("left");
                   changed = true;
               } else if (direction.X < 0 && LineIntersects(lastPosition + centerLeft, position + centerLeft, topRight, bottomRight)) {
                    newDirection.X *= -1;
                    Console.WriteLine("right");
                    changed = true;
                }

                if (direction.Y > 0 && LineIntersects(lastPosition + centerBottom, position + centerBottom, topLeft, topRight)) {
                    newDirection.Y *= -1;
                    Console.WriteLine("top");
                    changed = true;
                } else if (direction.Y < 0 && LineIntersects(lastPosition + centerTop, position + centerTop, bottomLeft, bottomRight)) {
                    newDirection.Y *= -1;
                    Console.WriteLine("bottom");
                    changed = true;
                }

                // Hit a corner - bounce it back
                if (!changed) {
                    newDirection.X *= -1;
                    newDirection.Y *= -1;
                }


                Console.WriteLine(direction);
                Console.WriteLine(newDirection);
                break;
            }

            direction = newDirection;
        }

        private bool LineIntersects(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
            float ua = (p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X);
            float ub = (p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X);
            float de = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);
            bool intersects = false;

            if (Math.Abs(de) >= 0.00001f) {
                ua /= de;
                ub /= de;

                if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1) {
                    intersects = true;
                }
            }

            return intersects;
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
        private const int bricksPerRow = 20;
        private const double wallHeightRatio = 0.20;
        private const int numRows = 7;
        private static readonly Color[] colors = {
            Color.Red, Color.Orange, Color.Yellow, Color.Green, 
            Color.Blue, Color.Indigo, Color.Violet
        };

        private readonly int screenWidth;
        private readonly int screenHeight;
        private float brickHeight;
        private float brickWidth;
        private Rectangle wallBounds;
        private Texture2D sprite;
        private List<Brick> bricks = new List<Brick>();

        public Rectangle Bounds {
            get { return wallBounds; }
        }

        public Wall(int screenWidth, int screenHeight) {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;

            double wallHeight = screenHeight * wallHeightRatio;
            wallBounds = new Rectangle(0, (int)(wallHeight / 2),
                        screenWidth, (int)(wallHeight + wallHeight / 2));
            brickHeight = wallBounds.Height / numRows;
            brickWidth = screenWidth / bricksPerRow;
            
        }

        internal void LoadContent(ContentManager contentManager) {
            sprite = contentManager.Load<Texture2D>("brick");
            InitialiseBricks();
        }

        private void InitialiseBricks() {
            for (int i = 0; i < numRows * bricksPerRow; i++) {
                int row = i / bricksPerRow;
                int column = i % bricksPerRow;
                bricks.Add(new Brick(
                        row, column,
                        column * brickWidth, 
                        wallBounds.Top + (row * brickHeight), 
                        brickWidth, 
                        brickHeight));
            }
        }

        internal void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
            foreach (Brick brick in bricks) {
                if (brick.State == Brick.BrickState.ALIVE) {
                    spriteBatch.Draw(sprite, brick.Bounds, colors[colors.Length - brick.Row - 1]);
                }
            }
        }

        internal List<Rectangle> DestroyBricksAt(Rectangle r) {
            List<Rectangle> destroyedRectangles = new List<Rectangle>();

            if (wallBounds.Intersects(r)) {
                foreach (Brick brick in bricks) {
                    if (brick.State == Brick.BrickState.ALIVE && brick.Bounds.Intersects(r)) {
                        brick.State = Brick.BrickState.BROKEN;
                        destroyedRectangles.Add(brick.Bounds);
                        break;
                    }
                }
            }

            return destroyedRectangles;
        }
    }

    class Brick {
        public enum BrickState { ALIVE, BROKEN };
        private readonly int column;
        private readonly int row;
        private Rectangle bounds;
        private BrickState state;

        public int Row {
            get { return row; }
        }

        public Rectangle Bounds {
            get { return bounds; }
        }

        public BrickState State { get; set; }

        public Brick(int row, int column, float x, float y, float w, float h) {
            this.state = BrickState.ALIVE;
            this.row = row;
            this.column = column;
            this.bounds = new Rectangle((int) x, (int) y, (int) w, (int) h);
        }
    }
}
