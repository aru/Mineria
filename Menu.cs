using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace Stereo
{
    class StereoControl : GraphicsDeviceControl
    {
        BasicEffect effect;

        ContentManager content;
        SpriteBatch spriteBatch;
        SpriteFont font;

        // Collection for game components
        GameComponentCollection Components;

        // Store a list of primitive models, plus which one is currently selected.
        List<GeometricPrimitive> primitives = new List<GeometricPrimitive>();

        int currentPrimitiveIndex = 0;

        // Timestep
        Stopwatch stopWatch;

        readonly TimeSpan TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);
        readonly TimeSpan MaxElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 10);

        TimeSpan accumulatedTime;
        TimeSpan lastTime;

        // We use matrixes to move the primitives around
        Matrix translation = Matrix.Identity;
        Matrix rotation = Matrix.Identity;
        Matrix scale = Matrix.Identity;

        FreeCamera camera;

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

        /// <summary>
        /// Initializes the control.
        /// </summary>
        protected override void Initialize()
        {
            // Initialize Component Collection
            Components = new GameComponentCollection();

            // BackBuffer size and stuff
            GraphicsDevice.PresentationParameters.BackBufferHeight = 480;
            GraphicsDevice.PresentationParameters.BackBufferWidth = 640;

            // Create our effect.
            effect = new BasicEffect(GraphicsDevice);
            //effect.VertexColorEnabled = true;
            effect.EnableDefaultLighting();

            // Start the animation timer.
            stopWatch = Stopwatch.StartNew();

            // Content stuff
            content = new ContentManager(Services, "Content");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //font = content.Load<SpriteFont>("hudFont"); font doesn't exist yet

            // Load the primitives
            primitives.Add(new SpherePrimitive(GraphicsDevice));
            primitives.Add(new CubePrimitive(GraphicsDevice));
            primitives.Add(new CylinderPrimitive(GraphicsDevice));
            primitives.Add(new TorusPrimitive(GraphicsDevice));
            primitives.Add(new EllipticalCylinder(GraphicsDevice));

            //camera = new Camera(GraphicsDevice, stopWatch, new Vector3(0, 0, 5),
            //    Vector3.Zero, Vector3.Up);

            camera = new FreeCamera( GraphicsDevice );

            Components.Add(camera);

            // Go through every component and execute their Initialize method
            foreach (WinFormComponent component in Components)
            {
                component.Initialize();
            }

            // Hook the idle event to constantly redraw our animation.
            Application.Idle += TickWhileIdle;
        }


        /// <summary>
        /// Draws the control.
        /// </summary>
        protected override void Draw()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Spin the primitives according to how much time has passed.
            float time = (float)stopWatch.Elapsed.TotalSeconds;

            float yaw = time * 0.7f;
            float pitch = time * 0.8f;
            float roll = time * 0.9f;

            // Set the color
            GeometricPrimitive currentPrimitive = primitives[currentPrimitiveIndex];
            Color color = Color.Red;

            // Set transform matrices.
            float aspect = GraphicsDevice.Viewport.AspectRatio;

            // end of my stuff

            effect.World = Matrix.CreateTranslation( 0.0f, 0.0f, 0.0f );
            effect.World *= Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

            effect.View = camera.viewMatrix;
            effect.Projection = camera.projectionMatrix;
            //effect.View = Matrix.CreateLookAt(new Vector3(0, 0, -5),
            //                                  Vector3.Zero, Vector3.Up);

            //effect.Projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 10);

            effect.DiffuseColor = color.ToVector3();
            effect.Alpha = color.A / 255.0f;

            effect.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            if (color.A < 255)
            {
                // Set renderstates for alpha blended rendering.
                effect.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            }
            else
            {
                // Set renderstates for opaque rendering.
                effect.GraphicsDevice.BlendState = BlendState.Opaque;
            }

            float size = 1.0f;

            //camera.Update(stopWatch);

            float pos = 0.0f;

            foreach (GeometricPrimitive primitive in primitives)
	        {

                effect.World = Matrix.CreateTranslation(new Vector3(pos, 0.0f, 0.0f));
                
		        primitive.Draw( effect );

                pos += 2.0f;
	        }

            /// go through every component and update them
            foreach (WinFormComponent component in Components)
            {
                component.Update(stopWatch);
            }

        }
    }
}
