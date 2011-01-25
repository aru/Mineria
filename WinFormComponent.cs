using System;
using System.Diagnostics;
using Microsoft.Xna;
using Microsoft.Xna.Framework;

namespace Stereo
{
    public class WinFormComponent : IGameComponent, IUpdateable
    {
        public virtual void Initialize()
        {
            
        }

        public bool Enabled
        {
            get { return true; }
        }

        public event EventHandler<EventArgs> EnabledChanged;

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

        private int updateOrder;

        public event EventHandler<EventArgs> UpdateOrderChanged;
    }
}
