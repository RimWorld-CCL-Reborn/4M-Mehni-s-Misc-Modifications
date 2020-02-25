using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace Mehni.Misc.Modifications
{
    public class StatWorker_RangedWeaponDPS : StatWorker
    {

        private float Dist
        {
            get
            {
                if (stat == MeMiMo_StatDefOf.RangedWeapon_TouchDPS)
                    return ShootTuning.DistTouch;
                if (stat == MeMiMo_StatDefOf.RangedWeapon_ShortDPS)
                    return ShootTuning.DistShort;
                if (stat == MeMiMo_StatDefOf.RangedWeapon_MediumDPS)
                    return ShootTuning.DistMedium;
                if (stat == MeMiMo_StatDefOf.RangedWeapon_LongDPS)
                    return ShootTuning.DistLong;
                return 0f;
            }
        }

        public override bool ShouldShowFor(StatRequest req)
            => MeMiMoSettings.displayRangedDPS
                && req.Def is ThingDef def && def.IsRangedWeapon && def.Verbs?[0]?.defaultProjectile != null
                && def.Verbs[0].defaultProjectile.projectile.damageDef.harmsHealth;

        public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized)
            => value.ToStringByStyle(stat.toStringStyle, numberSense);

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
            => GetRangedDamagePerSecond(req);

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            Thing weapon = GetThingFromReq(req);

            string explanation = RangedWeaponDPSUtility.GetExplanation(weapon, Dist);

            if (req.Thing == null)
                weapon.Destroy();

            return explanation;
        }

        private float GetRangedDamagePerSecond(StatRequest req)
        {
            Thing weapon = GetThingFromReq(req);

            float DPS = RangedWeaponDPSUtility.GetDPS(weapon, Dist);

            if (req.Thing == null)
                weapon.Destroy();

            return DPS;
        }

        private Thing GetThingFromReq(StatRequest req)
        {
            ThingDef def = req.Def as ThingDef;
            return req.Thing ?? ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
        }
    }
}
