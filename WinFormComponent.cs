using System;
using Microsoft.Xna;
using Microsoft.Xna.Framework;

namespace Stereo
{
    class WinFormComponent : IGameComponent, IUpdateable
    {
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public bool Enabled
        {
            get { return true; }
        }

        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
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
