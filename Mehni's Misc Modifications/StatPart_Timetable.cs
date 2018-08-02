using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;
using Verse.AI.Group;
using Harmony;
using RimWorld.Planet;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;


namespace Mehni.Misc.Modifications
{

    public class StatPart_TimeTable : StatPart
    {

        private const float WorkSpeedOffsetNotAssignedToWork = -0.2f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (MeMiMoSettings.workAssignmentMatters && req.Thing is Pawn pawn && pawn.timetable.CurrentAssignment != TimeAssignmentDefOf.Work)
                val += WorkSpeedOffsetNotAssignedToWork;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (MeMiMoSettings.workAssignmentMatters && req.Thing is Pawn pawn && pawn.timetable.CurrentAssignment != TimeAssignmentDefOf.Work)
                return "M4_StatPart_TimeTable_ExplanationString".Translate() + ": " + WorkSpeedOffsetNotAssignedToWork.ToStringPercent();
            return null;
        }
        
    }

}
