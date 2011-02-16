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
    class StereoControl : GraphicsDeviceControl
    {

        #region Constructor
        //public void Menu()
        //{
            
        //}
        #endregion

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
            "Cilindro eliptico",
            "Cilindro hiperbolico",
        };

        // The cursor is used to tell what the user's pointer/mouse is over. The cursor
        // is moved with the left thumbstick. On windows, the mouse can be used as well.
        Cursor cursor;

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
            // Set our root directory
            content.RootDirectory = "Content";
            // use new spriteBatch
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = content.Load<SpriteFont>(@"fonts\hudFont");
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

            // Add a new cursor to our scene
            cursor = new Cursor(GraphicsDevice, content, spriteBatch);
            Components.Add(cursor);

            // Set up the mouse and stuff
            Mouse.WindowHandle = this.Handle;

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

            // Go through every DrawableComponent and Draw() them
            //foreach (DrawableWinFormComponent drawable in Components)
            //{
            //    if( drawable.ToString() == "DrawableWinFormComponent" )
            //        drawable.Draw(stopWatch);
            //}

            cursor.Draw(stopWatch);

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

                    // We already have the center position of each primitive even if they move accros the world
                    // by using the translate vector of each primitive as a center for each boundingSphere, we
                    // can just easily create a new sphere, take it shawn
                    BoundingSphere sphere = new BoundingSphere(primitive.Transformation.Translate, 0.5f);

                    // draw the sphere with our renderer
                    BoundingSphereRenderer.Draw(sphere, viewMatrix, projectionMatrix);

                }

            }

            // We loop again for every primitive, why? because we now want to be drawing some text above each primitive
            // using spriteBatch(), since spriteBatch changes some states in the graphicsDevice, we need to reset some
            // parameters to draw 3D again, since initializing, ending, resetting uses up cycles, we only want to do this
            // once, not every time we go through a primitive

            // Begin the SpriteBatch
            spriteBatch.Begin();

            // If the cursor is over a model, we'll draw its name. To figure out if
            // the cursor is over a model, we'll use cursor.CalculateCursorRay. That
            // function gives us a world space ray that starts at the "eye" of the
            // camera, and shoots out in the direction pointed to by the cursor.
            Ray cursorRay = cursor.CalculateCursorRay(projectionMatrix, viewMatrix);

            // Initialize a variable
            int i = 0;

            // Loop through the primitives
            foreach (GeometricPrimitive primitive in primitives)
            {
                // Gimme a big enough sphere
                BoundingSphere sphere = new BoundingSphere(primitive.Transformation.Translate, 0.5f);

                // To check for intersection
                if (sphere.Intersects(cursorRay) != null)
                {
                    // if there is an intersection

                    // now we know that we want to draw the model's name. We want to
                    // draw the name a little bit above the model: but where's that?
                    // SpriteBatch.DrawString takes screen space coordinates, but the 
                    // model's position is stored in world space. 

                    // we'll use Viewport.Project, which will project a world space
                    // point into screen space. We'll project the vector (0,0,0) using
                    // the model's world matrix, and the view and projection matrices.
                    // that will tell us where the model's origin is on the screen.
                    Vector3 screenSpace = GraphicsDevice.Viewport.Project(
                        Vector3.Zero, camera.projectionMatrix, camera.viewMatrix,
                        primitive.Transformation.Matrix);

                    // we want to draw the text a little bit above that, so we'll use
                    // the screen space position - 60 to move up a little bit. A better
                    // approach would be to calculate where the top of the model is, and
                    // draw there. It's not that much harder to do, but to keep the
                    // sample easy, we'll take the easy way out.
                    Vector2 textPosition =
                        new Vector2(screenSpace.X, screenSpace.Y - 60);

                    // we want to draw the text centered around textPosition, so we'll
                    // calculate the center of the string, and use that as the origin
                    // argument to spriteBatch.DrawString. DrawString automatically
                    // centers text around the vector specified by the origin argument.
                    Vector2 stringCenter =
                        font.MeasureString(ModelFilenames[i]) / 2;

                    // to make the text readable, we'll draw the same thing twice, once
                    // white and once black, with a little offset to get a drop shadow
                    // effect.

                    // first we'll draw the shadow...
                    Vector2 shadowOffset = new Vector2(1, 1);
                    spriteBatch.DrawString(font, ModelFilenames[i],
                        textPosition + shadowOffset, Color.Black, 0.0f,
                        stringCenter, 1.0f, SpriteEffects.None, 0.0f);

                    // ...and then the real text on top.
                    spriteBatch.DrawString(font, ModelFilenames[i],
                        textPosition, Color.White, 0.0f,
                        stringCenter, 1.0f, SpriteEffects.None, 0.0f);
                }

                // add up i
                i++;

            }

            // End the spriteBatch, duh
            spriteBatch.End();

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

        /// <summary>
        /// This helper function checks to see if a ray will intersect with a model.
        /// The model's bounding spheres are used, and the model is transformed using
        /// the matrix specified in the worldTransform argument.
        /// </summary>
        /// <param name="ray">the ray to perform the intersection check with</param>
        /// <param name="model">the model to perform the intersection check with.
        /// the model's bounding spheres will be used.</param>
        /// <param name="worldTransform">a matrix that positions the model
        /// in world space</param>
        /// <param name="absoluteBoneTransforms">this array of matrices contains the
        /// absolute bone transforms for the model. this can be obtained using the
        /// Model.CopyAbsoluteBoneTransformsTo function.</param>
        /// <returns>true if the ray intersects the model.</returns>
        private static bool RayIntersectsSphere(Ray ray, BoundingSphere sphere)
        {

            if (sphere.Intersects(ray) != null)
            {
                return true;
            }

            // If we've gotten this far, This means that there was no collision,
            // and we should return false.
            return false;
        }

        #endregion
    }
}
