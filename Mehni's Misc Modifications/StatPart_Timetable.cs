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

        private const float WorkSpeedOffsetLazyAssignment = -0.2f;
        private const float WorkSpeedOffsetWorkAssignment = 0.2f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (MeMiMoSettings.workAssignmentMatters && req.Thing is Pawn pawn && pawn.timetable is Pawn_TimetableTracker timetable)
            {
                TimeAssignmentDef currentAssignment = timetable.CurrentAssignment;
                if (currentAssignment == TimeAssignmentDefOf.Joy || currentAssignment == TimeAssignmentDefOf.Sleep)
                    val += WorkSpeedOffsetLazyAssignment;
                if (currentAssignment == TimeAssignmentDefOf.Work)
                    val += WorkSpeedOffsetWorkAssignment;
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (MeMiMoSettings.workAssignmentMatters && req.Thing is Pawn pawn && pawn.timetable is Pawn_TimetableTracker timetable)
            {
                TimeAssignmentDef currentAssignment = pawn.timetable.CurrentAssignment;
                string baseExplanationString = "M4_StatPart_TimeTable_ExplanationString".Translate() + ": ";
                if (currentAssignment == TimeAssignmentDefOf.Joy || currentAssignment == TimeAssignmentDefOf.Sleep)
                    return baseExplanationString + WorkSpeedOffsetLazyAssignment.ToStringPercent();
                if (currentAssignment == TimeAssignmentDefOf.Work)
                    return baseExplanationString + "+" + WorkSpeedOffsetWorkAssignment.ToStringPercent();
            }
            return null;
        }
        
    }

}
