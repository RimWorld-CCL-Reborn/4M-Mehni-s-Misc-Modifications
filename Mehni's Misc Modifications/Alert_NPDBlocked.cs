using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;

namespace Mehni.Misc.Modifications
{
    public class Alert_NPDBlocked : Alert
    {
        public Alert_NPDBlocked()
        {
            defaultExplanation = "M4_NPDNeedsSpace_Desc".Translate();
            defaultLabel = "M4_NPDNeedsSpace".Translate();
        }

        public override AlertReport GetReport() => AlertReport.CulpritsAre(BlockedNPDs.ToList());

        // ReSharper disable once InconsistentNaming
        private static IEnumerable<Thing> BlockedNPDs
        {
            get
            {
                List<Map> maps = Find.Maps;
                foreach (Map map in maps)
                {
                    IEnumerable<Building> npdBuildings = map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.NutrientPasteDispenser);
                    foreach (Building building in npdBuildings)
                    {
                        if (!building.InteractionCell.Standable(map))
                            yield return building;
                    }
                }
            }
        }
    }
}
