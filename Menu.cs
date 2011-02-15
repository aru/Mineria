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

        // Our Basic effect
        BasicEffect effect;

        // Content stuff
        ContentManager content;
        Texture2D backgroundTexture;
        Rectangle mainFrame;
        SpriteBatch spriteBatch;
        SpriteFont font;

        // Collection for game components
        GameComponentCollection Components;

        // Store a list of primitive models, plus which one is currently selected.
        List<GeometricPrimitive> primitives = new List<GeometricPrimitive>();

        // Our camera used for this example
        FreeCamera camera;

        // Some cool stuff
        RasterizerState wireFrame;
        bool drawBoundingSphere = true;

        // The names of each primitive, these will appear right above a pointed primitive
        static readonly string[] ModelFilenames = new string[]{
            "Esfera",
            "Cubo",
            "Cilindro",
            "Toroide",
            "Cilindro elíptico",
            "Cilindro hiperbólico",
        };

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
            // Initialize our custom Component Collection
            Components = new GameComponentCollection();

            // Initialize RasterizerState to Enable wireFrames
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

            // Load the primitives we want to be drawn
            // Load the primitives
            primitives.Add(new SpherePrimitive(GraphicsDevice));
            primitives.Add(new CubePrimitive(GraphicsDevice));
            primitives.Add(new CylinderPrimitive(GraphicsDevice));
            primitives.Add(new TorusPrimitive(GraphicsDevice));
            primitives.Add(new EllipticalCylinder(GraphicsDevice));
            primitives.Add(new HyperbollicCylinder(GraphicsDevice));

            // Initialize the renderer for our bounding spheres
            BoundingSphereRenderer.Initialize(GraphicsDevice, 45);

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
        #endregion

        #region Draw
        /// <summary>
        /// Draws the control.
        /// </summary>
        protected override void Draw()
        {

            // Clear the Graphics Device to render the scene
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
            //spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Additive);
            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
            //    null, null, null, null, transformMatrix);

            spriteBatch.Draw(backgroundTexture, mainFrame,
                             Color.White);

            spriteBatch.End();

            //we reset the states modified by the spritebatch
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //Depending on the 3D content
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.RasterizerState = wireFrame;

            // Spin the primitives according to how much time has passed.
            float time = (float)stopWatch.Elapsed.TotalSeconds;

            // Lulz done to get that weird spinning rotation
            float yaw = time * 0.7f;
            float pitch = time * 0.8f;
            float roll = time * 0.9f;

            // Set the color
            Color color = Color.Red;

            // Set transform matrices via the camera
            effect.View = camera.viewMatrix;
            effect.Projection = camera.projectionMatrix;

            /// <summary> 
            /// This part just sets some values for the basic shader to work and stuff
            /// </summary>
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

            // Draw them prims
            DrawPrimitives(effect, camera.viewMatrix, camera.projectionMatrix, time);

            /// Go through every Component attached and Update() them
            foreach (WinFormComponent component in Components)
            {
                component.Update(stopWatch);
            }

        }
        #endregion

        #region HelperFunctions

        /// <summary>
        ///  This function draws every primitive on screen, takes 2 matrices, the current time, and does magic 
        ///  to render the boundingSpheres around every primitive
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="viewMatrix"></param>
        /// <param name="projectionMatrix"></param>
        /// <param name="time"></param>
        void DrawPrimitives( BasicEffect effect, Matrix viewMatrix, Matrix projectionMatrix, float time )
        {

            // Lulz done to get that weird spinning rotation
            float yaw = time * 0.7f;
            float pitch = time * 0.8f;
            float roll = time * 0.9f;

            // Move every primitive next to each other <--- to be erased soon <--- LIE!
            float pos = -5.0f;

            foreach (GeometricPrimitive primitive in primitives)
            {
                // Rotate them for t3h lulz
                primitive.Transformation.Rotate = new Vector3(yaw, pitch, roll) * 30.0f;
                // Offset each primitive by a factor
                primitive.Transformation.Translate = new Vector3(pos, 0.0f, 0.0f);
                // Update the world matrix by this factor
                effect.World = primitive.Transformation.Matrix;
                // Draw the primitive
                primitive.Draw(effect);
                // Update this factor
                pos += 2.0f;

                if (drawBoundingSphere)
                {

                    // the mesh's BoundingSphere is stored relative to the mesh itself.
                    // (Mesh space). We want to get this BoundingSphere in terms of world
                    // coordinates. To do this, we calculate a matrix that will transform
                    // from coordinates from mesh space into world space....
                    //Matrix world = absoluteBoneTransforms[mesh.ParentBone.Index] * worldTransform;
                    Matrix world = primitive.Transformation.Matrix;

                    // ... and then transform the BoundingSphere using that matrix.
                    BoundingSphere sphere = TransformBoundingSphere(new BoundingSphere(primitive.Transformation.Translate, 0.5f), Matrix.Identity);
                    //BoundingSphere sphere = new BoundingSphere(primitive.Transformation.Matrix, world);

                    // now draw the sphere with our renderer
                    BoundingSphereRenderer.Draw(sphere, viewMatrix, projectionMatrix);


                }

            }

        }

        /// <summary>
        /// This helper function takes a BoundingSphere and a transform matrix, and
        /// returns a transformed version of that BoundingSphere.
        /// </summary>
        /// <param name="sphere">the BoundingSphere to transform</param>
        /// <param name="world">how to transform the BoundingSphere.</param>
        /// <returns>the transformed BoundingSphere/</returns>
        private static BoundingSphere TransformBoundingSphere(BoundingSphere sphere, Matrix transform)
        {
            BoundingSphere transformedSphere;

            // the transform can contain different scales on the x, y, and z components.
            // this has the effect of stretching and squishing our bounding sphere along
            // different axes. Obviously, this is no good: a bounding sphere has to be a
            // SPHERE. so, the transformed sphere's radius must be the maximum of the 
            // scaled x, y, and z radii.

            // to calculate how the transform matrix will affect the x, y, and z
            // components of the sphere, we'll create a vector3 with x y and z equal
            // to the sphere's radius...
            Vector3 scale3 = new Vector3(sphere.Radius, sphere.Radius, sphere.Radius);

            // then transform that vector using the transform matrix. we use
            // TransformNormal because we don't want to take translation into account.
            scale3 = Vector3.TransformNormal(scale3, transform);

            // scale3 contains the x, y, and z radii of a squished and stretched sphere.
            // we'll set the finished sphere's radius to the maximum of the x y and z
            // radii, creating a sphere that is large enough to contain the original 
            // squished sphere.
            transformedSphere.Radius = Math.Max(scale3.X, Math.Max(scale3.Y, scale3.Z));

            // transforming the center of the sphere is much easier. we can just use 
            // Vector3.Transform to transform the center vector. notice that we're using
            // Transform instead of TransformNormal because in this case we DO want to 
            // take translation into account.
            transformedSphere.Center = Vector3.Transform(sphere.Center, transform);

            return transformedSphere;
        }

        #endregion
    }
}
