using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace Mehni.Misc.Modifications
{
    public class StatWorker_RangedWeaponDPSPawn : StatWorker
    {

        private float Dist
        {
            get
            {
                if (stat == MeMiMo_StatDefOf.RangedWeapon_TouchDPSPawn)
                    return ShootTuning.DistTouch;
                if (stat == MeMiMo_StatDefOf.RangedWeapon_ShortDPSPawn)
                    return ShootTuning.DistShort;
                if (stat == MeMiMo_StatDefOf.RangedWeapon_MediumDPSPawn)
                    return ShootTuning.DistMedium;
                if (stat == MeMiMo_StatDefOf.RangedWeapon_LongDPSPawn)
                    return ShootTuning.DistLong;
                return 0f;
            }
        }

        public override bool ShouldShowFor(StatRequest req) =>
            (req.Thing as Pawn) != null && MeMiMoSettings.displayRangedDPS;

        public override bool IsDisabledFor(Thing thing) =>
            base.IsDisabledFor(thing) || StatDefOf.ShootingAccuracyPawn.Worker.IsDisabledFor(thing);

        public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized) =>
            value.ToStringByStyle(stat.toStringStyle, numberSense);

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true) =>
            GetRangedDamagePerSecond(req);

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            Pawn pawn = req.Thing as Pawn;

            Thing weapon = pawn?.equipment?.Primary;
            if (weapon == null || !weapon.def.IsRangedWeapon)
                return "M4_NoRangedWeapon".Translate();

            return RangedWeaponDPSUtility.GetExplanation(weapon, Dist, pawn);
        }

        private float GetRangedDamagePerSecond(StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;
            Thing weapon = pawn.equipment?.Primary;

            if (weapon == null || !weapon.def.IsRangedWeapon)
                return 0f;

            return RangedWeaponDPSUtility.GetDPS(weapon, Dist, pawn);
        }

    }
}
