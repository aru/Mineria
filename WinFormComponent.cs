using System;
using System.Diagnostics;
using Microsoft.Xna;
using Microsoft.Xna.Framework;

namespace Stereo
{
    public class WinFormComponent : IGameComponent, IUpdateable
    {
        public event EventHandler<EventArgs> EnabledChanged;
        /// <summary>Triggered when the value of the update order property is changed.</summary>
        public event EventHandler<EventArgs> UpdateOrderChanged;

        /// <summary>Gives the game component a chance to initialize itself</summary>
        public virtual void Initialize() { }

        public bool Enabled
        {
            get { return true; }
        }

        public virtual void Update(GameTime gameTime)
        {
            
        }

        public virtual void Update(Stopwatch stopwatch)
        {

        }

        public int UpdateOrder
        {
            get { return updateOrder; }

            set
            {
                if ( updateOrder != value )
                {
                    updateOrder = value;

                    if (UpdateOrderChanged != null)
                    {
                        UpdateOrderChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>Fires the EnabledChanged event</summary>
        protected virtual void OnEnabledChanged()
        {
            if (this.EnabledChanged != null)
            {
                this.EnabledChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///   Used to determine the updating order of this object in relation to other
        ///   objects in the same list.
        /// </summary>
        private int updateOrder;

    }
}
