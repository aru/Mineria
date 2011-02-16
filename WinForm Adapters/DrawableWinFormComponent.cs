using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Stereo
{
    public class DrawableWinFormComponent : WinFormComponent, IDrawable, IDisposable
    {
        public virtual void Draw(GameTime gameTime)
        {
            
        }

        public virtual void Draw(System.Diagnostics.Stopwatch stopwatch)
        {

        }

        public virtual void LoadContent() { }

        /// <summary>
        ///   Indicates when the drawable component should be drawn in relation to other
        ///   drawables. Has no effect by itself.
        /// </summary>
        public int DrawOrder
        {
            get { return this.drawOrder; }
            set
            {
                if (value != this.drawOrder)
                {
                    this.drawOrder = value;
                    OnDrawOrderChanged();
                }
            }
        }

        /// <summary>Triggered when the value of the draw order property is changed.</summary>
        public event EventHandler<EventArgs> DrawOrderChanged;

        /// <summary>Fires the DrawOrderChanged event</summary>
        protected virtual void OnDrawOrderChanged()
        {
            if (this.DrawOrderChanged != null)
            {
                this.DrawOrderChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>True when the drawable component is visible and should be drawn.</summary>
        public bool Visible
        {
            get { return this.visible; }
            set
            {
                if (value != this.visible)
                {
                    this.visible = value;
                    OnVisibleChanged();
                }
            }
        }

        /// <summary>Triggered when the value of the visible property is changed.</summary>
        public event EventHandler<EventArgs> VisibleChanged;

        /// <summary>Fires the VisibleChanged event</summary>
        protected virtual void OnVisibleChanged()
        {
            if (this.VisibleChanged != null)
            {
                this.VisibleChanged(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            
        }

        /// <summary>
        ///   Used to determine the drawing order of this object in relation to other
        ///   objects in the same list.
        /// </summary>
        private int drawOrder;
        /// <summary>Whether this object is visible (and should thus be drawn)</summary>
        private bool visible;
    }
}
