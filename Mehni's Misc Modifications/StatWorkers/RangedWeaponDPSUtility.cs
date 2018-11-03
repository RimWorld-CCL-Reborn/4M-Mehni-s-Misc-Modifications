using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace Mehni.Misc.Modifications
{
    public static class RangedWeaponDPSUtility
    {

        public static float GetDPS(Thing weapon, float dist, Pawn pawn = null)
        {
            VerbProperties verb = weapon.def.Verbs[0];
            ProjectileProperties projectile = verb.defaultProjectile.projectile;
            bool singleUse = verb.verbClass == typeof(Verb_ShootOneUse);

            float damage = GetDamage(projectile, weapon);
            float cooldown = GetCooldown(weapon, singleUse);
            float warmup = GetWarmup(verb, pawn);
            float accuracy = GetAccuracy(weapon, verb, projectile, dist, pawn);
            int burstCount = GetBurstCount(verb);
            float burstShotDelay = GetBurstShotDelay(verb);
            float projectileTravelTime = GetProjectileTravelTime(projectile, dist);
            float explosionDelay = GetExplosionDelay(projectile);

            return GetDPS(damage, cooldown, warmup, accuracy, burstCount, burstShotDelay, projectileTravelTime, explosionDelay);
        }

        public static float GetDPS(float damage, float cooldown, float warmup, float accuracy, int burstCount, float burstShotDelay, float projectileTravelTime, float explosionDelay) =>
            (damage * burstCount * accuracy) / (cooldown + warmup + ((burstCount - 1) * burstShotDelay) + projectileTravelTime + explosionDelay);

        public static string GetExplanation(Thing weapon, float dist, Pawn pawn = null)
        {
            VerbProperties verb = weapon.def.Verbs[0];
            ProjectileProperties projectile = verb.defaultProjectile.projectile;
            bool singleUse = verb.verbClass == typeof(Verb_ShootOneUse);

            float damage = GetDamage(projectile, weapon);
            float cooldown = GetCooldown(weapon, singleUse);
            float warmup = GetWarmup(verb, pawn);
            float accuracy = GetAccuracy(weapon, verb, projectile, dist, pawn);
            int burstCount = GetBurstCount(verb);
            float burstShotDelay = GetBurstShotDelay(verb);
            float projectileTravelTime = GetProjectileTravelTime(projectile, dist);
            float explosionDelay = GetExplosionDelay(projectile);
            int bCMinOne = burstCount - 1;

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
            if (pawn != null)
                expBuilder.AppendLine($"{"WarmupTime".Translate()}: {warmup.ToString("F2")} s ({verb.warmupTime.ToString("F2")} x {pawn.GetStatValue(StatDefOf.AimingDelayFactor).ToStringPercent()})");
            else
                expBuilder.AppendLine($"{"WarmupTime".Translate()}: {warmup.ToString("F2")} s");

            // - delay between burst shots
            if (bCMinOne > 0)
                expBuilder.AppendLine($"{"M4_BurstShotDelay".Translate()}: {(bCMinOne * burstShotDelay).ToString("F2")} s ({bCMinOne} x {burstShotDelay.ToString("F2")})");

            // Projectile 
            expBuilder.AppendLine($"{"M4_ProjectileTravelTime".Translate()}: {projectileTravelTime.ToString("F2")} s");
            if (explosionDelay > 0f)
                expBuilder.AppendLine($"{"M4_ProjectileExplosionDelay".Translate()}: {explosionDelay.ToString("F2")} s");

            expBuilder.AppendLine();
            expBuilder.AppendLine($"DPS = ({burstCount * damage} x {accuracy.ToStringPercent()}) /" +
                $" {(cooldown + warmup + (bCMinOne * burstShotDelay) + projectileTravelTime + explosionDelay).ToString("F2")}");

            return expBuilder.ToString();
        }

        #region Value Getters
        private static float GetDamage(ProjectileProperties projectile, Thing weapon) =>
            projectile.GetDamageAmount(weapon);

        private static float GetCooldown(Thing weapon, bool singleUse) =>
            (singleUse) ? 0f : weapon.GetStatValue(StatDefOf.RangedWeapon_Cooldown).SecondsToTicks().TicksToSeconds();

        private static float GetWarmup(VerbProperties verb, Pawn pawn = null)
        {
            float warmup = verb.warmupTime;
            if (pawn != null)
                warmup *= pawn.GetStatValue(StatDefOf.AimingDelayFactor);
            return warmup.SecondsToTicks().TicksToSeconds();
        }

        private static float GetAccuracy(Thing weapon, VerbProperties verb, ProjectileProperties projectile, float dist, Pawn pawn = null)
        {
            float forcedMissRadius = CalculateAdjustedForcedMissDist(verb.forcedMissRadius, dist);
            float baseAimOn = verb.GetHitChanceFactor(weapon, dist);
            if (pawn != null)
                baseAimOn *= ShotReport.HitFactorFromShooter(pawn, dist);
            int affectedCellCount = (verb.CausesExplosion) ? GenRadial.NumCellsInRadius(projectile.explosionRadius) : 1;

            float accuracy = 0f;
            if (forcedMissRadius > 0.5f)
            {
                int affectableCellCount = GenRadial.NumCellsInRadius(forcedMissRadius);
                accuracy = (float)affectedCellCount / affectableCellCount;
            }
            else
            {
                float medianToWildRadius = ShootTuning.MissDistanceFromAimOnChanceCurves.Evaluate(baseAimOn, 0.5f);
                float indirectHitChance = (float)(affectedCellCount - 1) / GenRadial.NumCellsInRadius(medianToWildRadius);
                accuracy = baseAimOn + (1f - baseAimOn) * indirectHitChance;
            }
            return Mathf.Clamp01(accuracy);
        }

        private static float CalculateAdjustedForcedMissDist(float forcedMiss, float dist)
        {
            if (dist < 9f)
                return 0f;
            if (dist < 25f)
                return forcedMiss * 0.5f;
            if (dist < 49f)
                return forcedMiss * 0.8f;
            return forcedMiss;
        }

        private static int GetBurstCount(VerbProperties verb) =>
            verb.burstShotCount;

        private static float GetBurstShotDelay(VerbProperties verb) =>
            verb.ticksBetweenBurstShots.TicksToSeconds();

        // 100 from Projectile.StartingTicksToImpact
        private static float GetProjectileTravelTime(ProjectileProperties projectile, float dist) =>
            Mathf.RoundToInt(Math.Max(dist / (projectile.speed / 100), 1)).TicksToSeconds();

        private static float GetExplosionDelay(ProjectileProperties projectile) =>
            projectile.explosionDelay.TicksToSeconds();
        #endregion

    }
}
