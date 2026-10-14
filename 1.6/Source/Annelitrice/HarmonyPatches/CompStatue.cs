using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Annelitrice
{
	public class StatueZOffsetExtension : DefModExtension
	{
		public float pawnZOffset = 0f;
	}

	public class SavedPawnRace : IExposable
	{
		public string defName;
		public void ExposeData() => Scribe_Values.Look(ref defName, "defName");
	}

	[HarmonyPatch(typeof(CompStatue), "CreateSnapshotOfPawn_HookForMods")]
	public static class Patch_CompStatue_SaveRace
	{
		public static void Postfix(Pawn p, Dictionary<string, object> dictToStoreDataIn)
		{
			if (p?.def != null && dictToStoreDataIn != null)
				dictToStoreDataIn["Annelitrice_RaceData"] = new SavedPawnRace { defName = p.def.defName };
		}
	}

	[HarmonyPatch(typeof(CompStatue), nameof(CompStatue.DrawAt))]
	public static class Patch_CompStatue_DrawAt_Transpiler
	{
		private static readonly MethodInfo MI_ApplyOffset =
			AccessTools.Method(typeof(Patch_CompStatue_DrawAt_Transpiler), nameof(ModifyPawnVector));

		public static Vector3 ModifyPawnVector(Vector3 pawnPos, CompStatue comp)
		{
			var ext = comp.parent.def.GetModExtension<StatueZOffsetExtension>();
			if (ext == null || ext.pawnZOffset == 0f) return pawnPos;

			var dict = Traverse.Create(comp).Field("additionalSavedPawnDataForMods").GetValue<Dictionary<string, object>>();

			if (dict != null && dict.TryGetValue("Annelitrice_RaceData", out var obj) && obj is SavedPawnRace saved)
			{
				if (saved.defName == AnnelitriceDefOf.Annelitrice.defName) 
				{
					pawnPos.z += ext.pawnZOffset;
				}
			}
			return pawnPos;
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			MethodInfo m_Addition = AccessTools.Method(typeof(Vector3), "op_Addition", new[] { typeof(Vector3), typeof(Vector3) });

			for (int i = 0; i < codes.Count; i++)
			{
				yield return codes[i];

				if (codes[i].Calls(m_Addition))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0); 
					yield return new CodeInstruction(OpCodes.Call, MI_ApplyOffset);

				}
			}
		}
	}
}