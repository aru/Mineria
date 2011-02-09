using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stereo
{
    class PrimitiveManager : WinFormComponent
    {
        List<GeometricPrimitive> primitives = new List<GeometricPrimitive>();

        public override void Update(System.Diagnostics.Stopwatch stopwatch)
        {
            base.Update(stopwatch);
        }
    }
}
