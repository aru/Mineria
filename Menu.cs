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
        #region Class Variables

        BasicEffect effect;

        ContentManager content;
        Texture2D backgroundTexture;
        Rectangle mainFrame;
        SpriteBatch spriteBatch;
        SpriteFont font;

        // Collection for game components
        GameComponentCollection Components;

        // Store a list of primitive models, plus which one is currently selected.
        List<GeometricPrimitive> primitives = new List<GeometricPrimitive>();

        int currentPrimitiveIndex = 0;

        // We use matrixes to move the primitives around
        Matrix[] worldTransforms;

        FreeCamera camera;

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

        /// <summary>
        /// Initializes the control.
        /// </summary>
        protected override void Initialize()
        {
            // Initialize our custom Component Collection
            Components = new GameComponentCollection();

            // Initialize RasterizerState to Enable wireFrames
            RasterizerState wireFrame;
            wireFrame = new RasterizerState()
            {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.None,
            };

            // BackBuffer size and stuff
            GraphicsDevice.PresentationParameters.BackBufferHeight = 480;
            GraphicsDevice.PresentationParameters.BackBufferWidth = 640;
            GraphicsDevice.RasterizerState = wireFrame; // Optional, here for debugging purposes and stuff

            // Create our BasicEffect.
            effect = new BasicEffect(GraphicsDevice);
            //effect.VertexColorEnabled = true;
            effect.EnableDefaultLighting();

            // Content stuff
            content = new ContentManager(Services, "Content");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //font = content.Load<SpriteFont>("hudFont"); font doesn't exist yet
            backgroundTexture = content.Load<Texture2D>(@"Texturas\bg");
            mainFrame = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);

            spriteBatch.Draw(backgroundTexture, mainFrame,
                             new Color(0, 0, 0));

            spriteBatch.End();

            // Load the primitives
            primitives.Add(new SpherePrimitive(GraphicsDevice));
            primitives.Add(new CubePrimitive(GraphicsDevice));
            primitives.Add(new CylinderPrimitive(GraphicsDevice));
            primitives.Add(new TorusPrimitive(GraphicsDevice));
            primitives.Add(new EllipticalCylinder(GraphicsDevice));
            primitives.Add(new HyperbollicCylinder(GraphicsDevice));

            // Set the starting position of these new primitives
            worldTransforms = new Matrix[ primitives.Capacity ];

            float initPos = -5.0f;

            for (int i = 0; i < worldTransforms.Length; i++)
            {
                worldTransforms[i] = Matrix.CreateTranslation( new Vector3( initPos, 0, 0 ) );
                initPos += 1.0f;
            }

            // Add a new camera to our scene
            camera = new FreeCamera( GraphicsDevice );
            // Attach it as a game component
            Components.Add(camera);

            // Go through every component and execute their Initialize method
            foreach (WinFormComponent component in Components)
            {
                component.Initialize();
            }

            // Start the animation timer.
            stopWatch = Stopwatch.StartNew();

            // Hook the idle event to constantly redraw our animation.
            Application.Idle += TickWhileIdle;
        }


        /// <summary>
        /// Draws the control.
        /// </summary>
        protected override void Draw()
        {

            // Clear the Graphics Device to render the scene
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


            // Move every primitive next to each other <--- to be erased soon
            float pos = 0.0f;

            Vector3 rotation = new Vector3(1.0f, 0.0f, 0.0f);

            foreach (GeometricPrimitive primitive in primitives)
	        {
                effect.World = Matrix.CreateTranslation(new Vector3(pos, 0.0f, 0.0f));
		        primitive.Draw( effect );
                pos += 2.0f;
	        }

            /// Go through every Component attached and Update() them
            foreach (WinFormComponent component in Components)
            {
                component.Update(stopWatch);
            }

        }
    }
}
