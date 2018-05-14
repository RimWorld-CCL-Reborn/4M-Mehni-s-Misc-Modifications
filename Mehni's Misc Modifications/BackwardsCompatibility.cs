using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mehni.Misc.Modifications
{
    class BackwardsCompatibility
    {
        public static void DynamicFleeingPostLoadInit()
        {
            if (MeMiMoSettings.variableRaidRetreat)
            {
                if (MeMiMoSettings.retreatAtPercentageDefeated != 0.5f)
                {
                    MeMiMoSettings.retreatDefeatRange = new Verse.FloatRange(MeMiMoSettings.retreatAtPercentageDefeated, MeMiMoSettings.retreatAtPercentageDefeated);
                    MeMiMoSettings.retreatAtPercentageDefeated = 0.5f;
                }
                if (MeMiMoSettings.randomRaidRetreat)
                {
                    MeMiMoSettings.retreatDefeatRange = new Verse.FloatRange(0.3f, 0.7f);
                    MeMiMoSettings.randomRaidRetreat = false;
                }
            }
        }
    }
}
