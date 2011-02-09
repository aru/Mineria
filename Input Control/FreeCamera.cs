using System;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Stereo
{
    class FreeCamera : WinFormComponent
    {

        public Matrix viewMatrix { get; protected set; }
        public Matrix projectionMatrix { get; protected set; }

        Vector3 cameraPosition;
        Quaternion cameraRotation;

        GraphicsDevice graphicsDevice;

        public FreeCamera( GraphicsDevice graphicsDevice )
        {
            this.graphicsDevice = graphicsDevice;
        }

        public override void Initialize()
        {
            float viewAngle = MathHelper.PiOver4;
            float aspectRatio = graphicsDevice.Viewport.AspectRatio;
            float nearPlane = 0.5f;
            float farPlane = 100.0f;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio,
            nearPlane, farPlane);

            cameraPosition = new Vector3(-1, 1, 10);
            cameraRotation = Quaternion.Identity;
            UpdateViewMatrix();
        }

        public override void Update(Stopwatch stopwatch)
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            //if (gamePadState.Buttons.Back == ButtonState.Pressed)
            //    this.Exit();

            float updownRotation = 0.0f;
            float leftrightRotation = 0.0f;

            leftrightRotation -= gamePadState.ThumbSticks.Left.X / 50.0f;
            updownRotation += gamePadState.ThumbSticks.Left.Y / 50.0f;

            KeyboardState keys = Keyboard.GetState();

            if (keys.IsKeyDown(Keys.Up))
                updownRotation = 0.05f;
            if (keys.IsKeyDown(Keys.Down))
                updownRotation = -0.05f;
            if (keys.IsKeyDown(Keys.Right))
                leftrightRotation = -0.05f;
            if (keys.IsKeyDown(Keys.Left))
                leftrightRotation = 0.05f;

            Quaternion additionalRotation = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), updownRotation) * Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), leftrightRotation);
            cameraRotation = cameraRotation * additionalRotation;

            if(keys.IsKeyDown(Keys.W))
                AddToCameraPosition(new Vector3(0, 0, -1));
            if (keys.IsKeyDown(Keys.S))
                AddToCameraPosition(new Vector3(0, 0, 1));
            if(keys.IsKeyUp(Keys.W) && keys.IsKeyUp(Keys.S) )
                AddToCameraPosition(new Vector3(0, 0, 0));
        }

        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            float moveSpeed = 0.05f;
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, cameraRotatedUpVector);
        }

    }
}
