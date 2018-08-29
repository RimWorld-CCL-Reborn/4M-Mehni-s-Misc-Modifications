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

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (MeMiMoSettings.workAssignmentMatters && req.Thing is Pawn pawn && pawn.timetable is Pawn_TimetableTracker timetable)
            {
                TimeAssignmentDef currentAssignment = timetable.CurrentAssignment;
                val *= (currentAssignment.GetModExtension<TimeAssignmentExtension>() is TimeAssignmentExtension tAE) ?
                    tAE.globalWorkSpeedFactor : TimeAssignmentExtension.defaultValues.globalWorkSpeedFactor;
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (MeMiMoSettings.workAssignmentMatters && req.Thing is Pawn pawn && pawn.timetable is Pawn_TimetableTracker timetable)
            {
                TimeAssignmentDef currentAssignment = timetable.CurrentAssignment;
                float workSpeedFactor = (currentAssignment.GetModExtension<TimeAssignmentExtension>() is TimeAssignmentExtension tAE) ?
                    tAE.globalWorkSpeedFactor : TimeAssignmentExtension.defaultValues.globalWorkSpeedFactor;
                return "M4_StatPart_TimeTable_ExplanationString".Translate(currentAssignment.label) + ": x" + workSpeedFactor.ToStringPercent();
            }
            return null;
        }
        
    }

}
