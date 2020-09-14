using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Mehni.Misc.Modifications
{
    class TimeAssignmentExtension : DefModExtension
    {

        public static readonly TimeAssignmentExtension defaultValues = new TimeAssignmentExtension();

        public float globalWorkSpeedFactor = 1f;

    }
}
