using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Runtime.InteropServices;

namespace Stereo
{
    public class StereoCopy : GraphicsDeviceControl
    {

        #region Constructors
        public StereoCopy()
        {
        }
        public StereoCopy(Texture2D stereoTexture)
        {
            backgroundTexture = stereoTexture;
        }
        #endregion

        #region Class Variables

        // Our Basic effect
        BasicEffect effect;

        // Content stuff
        public Texture2D backgroundTexture { get; set; }
        Rectangle mainFrame;
        SpriteBatch spriteBatch;

        #endregion

        #region Timestep Fixing
        Stopwatch stopWatch;

        readonly TimeSpan TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);
        readonly TimeSpan MaxElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 10);

        TimeSpan accumulatedTime;
        TimeSpan lastTime;

        void Tick(object sender, EventArgs e)
        {
            TimeSpan currentTime = stopWatch.Elapsed;
            TimeSpan elapsedTime = currentTime - lastTime;
            lastTime = currentTime;

            if (elapsedTime > MaxElapsedTime)
            {
                elapsedTime = MaxElapsedTime;
            }

            accumulatedTime += elapsedTime;

            bool updated = false;

            while (accumulatedTime >= TargetElapsedTime)
            {
                Update();

                accumulatedTime -= TargetElapsedTime;
                updated = true;
            }

            if (updated)
            {
                Invalidate();
            }
        }

        // Ticking function
        void TickWhileIdle(object sender, EventArgs e)
        {
            NativeMethods.Message message;

            while (!NativeMethods.PeekMessage(out message, IntPtr.Zero, 0, 0, 0))
            {
                Tick(sender, e);
            }
        }

#endregion

        #region Initialize
        /// <summary>
        /// Initializes the control.
        /// </summary>
        protected override void Initialize()
        {

            // BackBuffer size and stuff
            GraphicsDevice.PresentationParameters.BackBufferHeight = 480;
            GraphicsDevice.PresentationParameters.BackBufferWidth = 640;

            backgroundTexture = new Texture2D(GraphicsDevice, GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);

            // Create our BasicEffect.
            effect = new BasicEffect(GraphicsDevice);

            // set a spritebatch
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // set the frame
            mainFrame = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            // Start the animation timer.
            stopWatch = Stopwatch.StartNew();

            // Hook the idle event to constantly redraw our animation.
            Application.Idle += Tick;
        }
        #endregion

        #region Draw
        /// <summary>
        /// Draws the control.
        /// </summary>
        protected override void Draw()
        {

            // has the window resized?
            mainFrame.Width = GraphicsDevice.Viewport.Width;
            mainFrame.Height = GraphicsDevice.Viewport.Height;

            // Clear the Graphics Device to render the scene
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);

            spriteBatch.Draw(backgroundTexture, mainFrame,
                             Color.White);

            spriteBatch.End();

        }
        #endregion

    }
}
