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

        public override bool ShouldShowFor(StatRequest req) =>
            req.Def is ThingDef def && def.IsRangedWeapon && def.Verbs?[0]?.defaultProjectile != null &&
            def.Verbs[0].defaultProjectile.projectile.damageDef.harmsHealth && MeMiMoSettings.displayRangedDPS;

        public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq) =>
            value.ToStringByStyle(stat.toStringStyle, numberSense);

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true) =>
            GetRangedDamagePerSecond(req);

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            ThingDef def = req.Def as ThingDef;
            Thing weapon = req.Thing ?? ThingMaker.MakeThing(def);

            string explanation = RangedWeaponDPSUtility.GetExplanation(weapon, Dist);

            if (req.Thing == null)
                weapon.Destroy();

            return explanation;
        }

        private float GetRangedDamagePerSecond(StatRequest req)
        {
            ThingDef def = req.Def as ThingDef;
            Thing weapon = req.Thing ?? ThingMaker.MakeThing(def);

            float DPS = RangedWeaponDPSUtility.GetDPS(weapon, Dist);

            if (req.Thing == null)
                weapon.Destroy();

            return DPS;
        }

        //100 from Projectile.StartingTicksToImpact
        private float GetProjectileImpactDelay(float speed, float dist) =>
            Mathf.RoundToInt(Math.Max(dist / (speed / 100), 1)).TicksToSeconds();

        private float GetRangedDamagePerSecond(float damage, float cooldown, float warmup, float accuracy, int burstCount, float burstShotDelay, float projectileImpactDelay, float explosionDelay) =>
            (damage * burstCount * accuracy) / (cooldown + warmup + ((burstCount - 1) * burstShotDelay) + projectileImpactDelay + explosionDelay);

    }
}
