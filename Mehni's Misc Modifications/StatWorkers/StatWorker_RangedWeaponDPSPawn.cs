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

        public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq) =>
            value.ToStringByStyle(stat.toStringStyle, numberSense);

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true) =>
            GetRangedDamagePerSecond(req);

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            // Not very efficient but does the job
            Pawn pawn = req.Thing as Pawn;
            if (pawn == null)
                return String.Empty;

            Thing weapon = pawn.equipment?.Primary;
            if (weapon == null || !weapon.def.IsRangedWeapon)
                return String.Empty;

            // Values
            VerbProperties verb = weapon.def.Verbs[0];
            ProjectileProperties projectile = verb.defaultProjectile.projectile;
            bool singleUse = verb.verbClass == typeof(Verb_ShootOneUse);

            float damage = projectile.GetDamageAmount(weapon);
            float cooldown = (singleUse) ? 0f : weapon.GetStatValue(StatDefOf.RangedWeapon_Cooldown).SecondsToTicks().TicksToSeconds();
            float warmup = verb.warmupTime.SecondsToTicks().TicksToSeconds();
            float accuracy = Mathf.Min((verb.forcedMissRadius > 0.5f) ?
                (float)((verb.CausesExplosion) ? GenRadial.NumCellsInRadius(projectile.explosionRadius) : 1) / GenRadial.NumCellsInRadius(verb.forcedMissRadius) :
                verb.GetHitChanceFactor(weapon, Dist) * ShotReport.HitFactorFromShooter(pawn, Dist), 1f);
            int burstCount = verb.burstShotCount;
            float burstShotDelay = verb.ticksBetweenBurstShots.TicksToSeconds();
            float projectileImpactDelay = GetProjectileImpactDelay(projectile.speed, Dist);
            float explosionDelay = projectile.explosionDelay.TicksToSeconds();
            int bCMinOne = burstCount - 1;

            if (req.Thing == null)
                weapon.Destroy();

            StringBuilder expBuilder = new StringBuilder();

            // Damage - Is the weapon burst-fire or single-shot?
            if (burstCount > 1)
                expBuilder.AppendLine($"{"M4_DamagePerBurst".Translate()}: {burstCount * damage} ({burstCount} x {damage})");
            else
                expBuilder.AppendLine($"{"Damage".Translate()}: {damage}");
            expBuilder.AppendLine($"{"Accuracy".Translate()}: {accuracy.ToStringPercent()}");
            expBuilder.AppendLine();

            // Cooldown and Warmup
            string singleUseText = (singleUse) ? " (" + "M4_WeaponSingleUse".Translate() + ")" : "";
            expBuilder.AppendLine($"{"CooldownTime".Translate()}: {cooldown.ToString("F2")} s{singleUseText}");
            expBuilder.AppendLine($"{"WarmupTime".Translate()}: {warmup.ToString("F2")} s");

            // - delay between burst shots
            if (bCMinOne > 0)
                expBuilder.AppendLine($"{"M4_BurstShotDelay".Translate()}: {(bCMinOne * burstShotDelay).ToString("F2")} s ({bCMinOne} x {burstShotDelay.ToString("F2")})");

            // Projectile 
            expBuilder.AppendLine($"{"M4_ProjectileTravelTime".Translate()}: {projectileImpactDelay.ToString("F2")} s");
            if (explosionDelay > 0f)
                expBuilder.AppendLine($"{"M4_ProjectileExplosionDelay".Translate()}: {explosionDelay.ToString("F2")} s");

            expBuilder.AppendLine();
            expBuilder.AppendLine($"DPS = ({burstCount * damage} x {accuracy.ToStringPercent()}) /" +
                $" {(cooldown + warmup + (bCMinOne * burstShotDelay) + projectileImpactDelay + explosionDelay).ToString("F2")}");

            return expBuilder.ToString();
        }

        private float GetRangedDamagePerSecond(StatRequest req)
        {
            // Probably not necessary but better safe than exceptional
            Pawn pawn = req.Thing as Pawn;
            if (pawn == null)
                return 0f;

            Thing weapon = pawn.equipment?.Primary;
            if (weapon == null || !weapon.def.IsRangedWeapon)
                return 0f;

            VerbProperties verb = weapon.def.Verbs[0];
            ProjectileProperties projectile = verb.defaultProjectile.projectile;
            bool singleUse = verb.verbClass == typeof(Verb_ShootOneUse);

            float damage = projectile.GetDamageAmount(weapon);
            float cooldown = (singleUse) ? 0f : weapon.GetStatValue(StatDefOf.RangedWeapon_Cooldown).SecondsToTicks().TicksToSeconds();
            float warmup = verb.warmupTime.SecondsToTicks().TicksToSeconds();
            float accuracy = Mathf.Min((verb.forcedMissRadius > 0.5f) ?
                (float)((verb.CausesExplosion) ? GenRadial.NumCellsInRadius(projectile.explosionRadius) : 1) / GenRadial.NumCellsInRadius(verb.forcedMissRadius) :
                verb.GetHitChanceFactor(weapon, Dist) * ShotReport.HitFactorFromShooter(pawn, Dist), 1f);
            int burstCount = verb.burstShotCount;
            float burstShotDelay = verb.ticksBetweenBurstShots.TicksToSeconds();
            float projectileImpactDelay = GetProjectileImpactDelay(projectile.speed, Dist);
            float explosionDelay = projectile.explosionDelay.TicksToSeconds();

            // Tackle any sort of bloat
            if (req.Thing == null)
                weapon.Destroy();

            return GetRangedDamagePerSecond(damage, cooldown, warmup, accuracy, burstCount, burstShotDelay, projectileImpactDelay, explosionDelay);
        }

        private float GetProjectileImpactDelay(float speed, float dist) =>
            Mathf.RoundToInt(Math.Max(dist / (speed / 100), 1)).TicksToSeconds();

        private float GetRangedDamagePerSecond(float damage, float cooldown, float warmup, float accuracy, int burstCount, float burstShotDelay, float projectileImpactDelay, float explosionDelay) =>
            (damage * burstCount * accuracy) / (cooldown + warmup + ((burstCount - 1) * burstShotDelay) + projectileImpactDelay + explosionDelay);

    }
}
