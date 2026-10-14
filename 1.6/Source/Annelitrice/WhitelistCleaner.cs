using Verse;
using System.Collections.Generic;
using AlienRace;
using System.Linq;

namespace Annelitrice
{
	[StaticConstructorOnStartup]
	public static class WhitelistCleaner
	{
		static WhitelistCleaner()
		{
			ThingDef_AlienRace race = DefDatabase<ThingDef_AlienRace>.GetNamed("Annelitrice");

			if (race != null && race.alienRace?.raceRestriction != null)
			{
				var list = race.alienRace.raceRestriction;
				list.whiteApparelList = new List<ThingDef>(list.apparelList ?? new List<ThingDef>());

				AddIfFound(list.whiteApparelList, "Apparel_ShieldBelt");
				AddIfFound(list.whiteApparelList, "Apparel_SmokepopBelt");
				AddIfFound(list.whiteApparelList, "Apparel_FirefoampopPack");
				AddIfFound(list.whiteApparelList, "Apparel_PsychicShockLance");
				AddIfFound(list.whiteApparelList, "Apparel_PsychicInsanityLance");

				if (ModLister.RoyaltyInstalled)
				{
					AddIfFound(list.whiteApparelList, "Apparel_PackJump");
					AddIfFound(list.whiteApparelList, "Apparel_PackBroadshield");
				}

				if (ModLister.BiotechInstalled)
				{
					AddIfFound(list.whiteApparelList, "Apparel_PackControl");
					AddIfFound(list.whiteApparelList, "Apparel_PackBandwidth");
					AddIfFound(list.whiteApparelList, "Apparel_PackTox");
				}

				if (ModLister.AnomalyInstalled)
				{
					AddIfFound(list.whiteApparelList, "Apparel_ShardPsychicShockLance");
					AddIfFound(list.whiteApparelList, "Apparel_ShardPsychicInsanityLance");
					AddIfFound(list.whiteApparelList, "Apparel_BiomutationLance");
					AddIfFound(list.whiteApparelList, "Apparel_DisruptorFlarePack");
					AddIfFound(list.whiteApparelList, "Apparel_DeadlifePack");
					AddIfFound(list.whiteApparelList, "Apparel_PackTurret");
				}

				if (ModLister.OdysseyInstalled)
				{
					AddIfFound(list.whiteApparelList, "Apparel_PackHunter");
					AddIfFound(list.whiteApparelList, "Apparel_CerebrexNode");
				}
			}
		}

		private static void AddIfFound(List<ThingDef> list, string defName)
		{
			ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
			if (def != null)
			{
				list.Add(def);
			}
		}
	}
}