using System;
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

	public sealed class SavedPawnRace : IExposable
	{
		public string defName;

		public SavedPawnRace()
		{
		}
		public SavedPawnRace(string defName)
		{
			this.defName = defName;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref defName, "defName", null, false);
		}
	}

	[HarmonyPatch(typeof(CompStatue), "CreateSnapshotOfPawn_HookForMods")]
	public static class Patch_CompStatue_CreateSnapshotOfPawn_HookForMods
	{
		public static void Postfix(Pawn p, Dictionary<string, object> dictToStoreDataIn)
		{
			if (p == null || p.def == null || dictToStoreDataIn == null) return;

			dictToStoreDataIn["SourcePawnRaceExposable"] = new SavedPawnRace(p.def.defName);
		}
	}

	[HarmonyPatch(typeof(CompStatue), nameof(CompStatue.PostExposeData))]
	public static class Patch_CompStatue_PostExposeData_Migrate
	{
		private static readonly FieldInfo FI_AdditionalSaved =
			AccessTools.Field(typeof(CompStatue), "additionalSavedPawnDataForMods");

		public static void Prefix(CompStatue __instance)
		{
			var dict = (Dictionary<string, object>)FI_AdditionalSaved?.GetValue(__instance);
			if (dict == null) return;

			if (dict.TryGetValue("SourcePawnDefName", out var legacy) && legacy is string s && !string.IsNullOrEmpty(s))
			{
				dict["SourcePawnRaceExposable"] = new SavedPawnRace(s);
				dict.Remove("SourcePawnDefName");
			}

			var badKeys = new List<string>();
			foreach (var kv in dict)
			{
				var v = kv.Value;
				if (v != null && !(v is IExposable))
					badKeys.Add(kv.Key);
			}
			for (int i = 0; i < badKeys.Count; i++)
				dict.Remove(badKeys[i]);
		}
	}

	[HarmonyPatch(typeof(CompStatue), nameof(CompStatue.DrawAt))]
	public static class Patch_CompStatue_DrawAt_Transpiler
	{
		private static readonly FieldInfo FI_AdditionalSaved =
			AccessTools.Field(typeof(CompStatue), "additionalSavedPawnDataForMods");

		private static readonly FieldInfo FI_AltIncVect =
			AccessTools.Field(typeof(Altitudes), "AltIncVect");

		public static void ApplyZOffset(ref Vector3 drawPos, CompStatue __instance)
		{
			try
			{
				var ext = __instance?.parent?.def?.GetModExtension<StatueZOffsetExtension>();
				if (ext == null || Math.Abs(ext.pawnZOffset) < 0.0001f) return;

				var dict = FI_AdditionalSaved != null
					? (Dictionary<string, object>)FI_AdditionalSaved.GetValue(__instance)
					: null;
				if (dict == null) return;

				string sourceDefName = null;
				if (dict.TryGetValue("SourcePawnRaceExposable", out var boxed) && boxed is SavedPawnRace spr && !string.IsNullOrEmpty(spr.defName))
				{
					sourceDefName = spr.defName;
				}
				else
				{
					return;
				}

				if (sourceDefName == AnnelitriceDefOf.Annelitrice.defName)
				{
					drawPos += Vector3.forward * ext.pawnZOffset;
				}
			}
			catch
			{
			}
		}
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
		{
			var codes = new List<CodeInstruction>(instructions);
			var miApply = AccessTools.Method(typeof(Patch_CompStatue_DrawAt_Transpiler), nameof(ApplyZOffset));

			bool injected = false;

			for (int i = 0; i < codes.Count; i++)
			{
				var ins = codes[i];
				yield return ins;

				if (!injected)
				{
					if (ins.opcode == OpCodes.Starg || ins.opcode == OpCodes.Starg_S)
					{
						const int window = 8;
						bool justDidAltIncSub = false;
						for (int j = Math.Max(0, i - window); j < i; j++)
						{
							var prev = codes[j];
							if (prev.opcode == OpCodes.Ldsfld && prev.operand is FieldInfo fi && fi == FI_AltIncVect)
							{
								justDidAltIncSub = true;
								break;
							}
						}

						if (justDidAltIncSub)
						{
							yield return new CodeInstruction(OpCodes.Ldarga_S, (byte)1); 
							yield return new CodeInstruction(OpCodes.Ldarg_0);          
							yield return new CodeInstruction(OpCodes.Call, miApply);

							injected = true;
						}
					}
				}
			}
		}
	}
}
