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
			Scribe_Values.Look(ref defName, "defName", null);
		}
	}

	internal static class StatueSavedDataKeys
	{
		public const string RaceKey = "SourcePawnRaceExposable";
		public const string LegacyKey = "SourcePawnDefName";
	}

	[HarmonyPatch(typeof(CompStatue), "CreateSnapshotOfPawn_HookForMods")]
	public static class Patch_CompStatue_CreateSnapshotOfPawn_HookForMods
	{
		public static void Postfix(Pawn p, Dictionary<string, object> dictToStoreDataIn)
		{
			if (p == null || p.def == null || dictToStoreDataIn == null)
				return;

			dictToStoreDataIn[StatueSavedDataKeys.RaceKey] = new SavedPawnRace(p.def.defName);
		}
	}

	[HarmonyPatch(typeof(CompStatue), nameof(CompStatue.PostExposeData))]
	public static class Patch_CompStatue_PostExposeData_Migrate
	{
		private static readonly FieldInfo FI_AdditionalSaved =
			AccessTools.Field(typeof(CompStatue), "additionalSavedPawnDataForMods");

		public static void Prefix(CompStatue __instance)
		{
			if (FI_AdditionalSaved == null || __instance == null)
				return;

			var dict = FI_AdditionalSaved.GetValue(__instance) as Dictionary<string, object>;
			if (dict == null || dict.Count == 0)
				return;

			object legacy;
			string legacyStr;
			if (dict.TryGetValue(StatueSavedDataKeys.LegacyKey, out legacy)
				&& (legacyStr = legacy as string) != null
				&& !string.IsNullOrEmpty(legacyStr))
			{
				dict[StatueSavedDataKeys.RaceKey] = new SavedPawnRace(legacyStr);
				dict.Remove(StatueSavedDataKeys.LegacyKey);
			}

			List<string> keysToRemove = null;
			foreach (KeyValuePair<string, object> kv in dict)
			{
				object value = kv.Value;
				if (value != null && !(value is IExposable))
				{
					if (keysToRemove == null)
						keysToRemove = new List<string>();
					keysToRemove.Add(kv.Key);
				}
			}

			if (keysToRemove != null)
			{
				for (int i = 0; i < keysToRemove.Count; i++)
					dict.Remove(keysToRemove[i]);
			}
		}
	}

	[HarmonyPatch(typeof(CompStatue), nameof(CompStatue.DrawAt))]
	public static class Patch_CompStatue_DrawAt_Transpiler
	{
		private static readonly FieldInfo FI_AdditionalSaved =
			AccessTools.Field(typeof(CompStatue), "additionalSavedPawnDataForMods");

		private static readonly FieldInfo FI_AltIncVect =
			AccessTools.Field(typeof(Altitudes), "AltIncVect");

		private static readonly MethodInfo MI_ApplyZOffset =
			AccessTools.Method(typeof(Patch_CompStatue_DrawAt_Transpiler), nameof(ApplyZOffset));

		public static void ApplyZOffset(ref Vector3 drawPos, CompStatue compStatue)
		{
			if (compStatue == null)
				return;

			Thing parent = compStatue.parent;
			if (parent == null)
				return;

			ThingDef def = parent.def;
			if (def == null)
				return;

			StatueZOffsetExtension ext = def.GetModExtension<StatueZOffsetExtension>();
			if (ext == null)
				return;

			float offset = ext.pawnZOffset;
			if (Math.Abs(offset) < 0.0001f)
				return;

			if (FI_AdditionalSaved == null)
				return;

			var dict = FI_AdditionalSaved.GetValue(compStatue) as Dictionary<string, object>;
			if (dict == null)
				return;

			object boxed;
			if (!dict.TryGetValue(StatueSavedDataKeys.RaceKey, out boxed))
				return;

			var savedRace = boxed as SavedPawnRace;
			if (savedRace == null || string.IsNullOrEmpty(savedRace.defName))
				return;

			ThingDef annDef = AnnelitriceDefOf.Annelitrice;
			if (annDef == null)
				return;

			if (!string.Equals(savedRace.defName, annDef.defName, StringComparison.Ordinal))
				return;

			drawPos.z += offset;
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
		{
			var codes = new List<CodeInstruction>(instructions);
			bool injected = false;

			for (int i = 0; i < codes.Count; i++)
			{
				CodeInstruction ins = codes[i];
				yield return ins;

				if (injected)
					continue;

				if (ins.opcode == OpCodes.Starg || ins.opcode == OpCodes.Starg_S)
				{
					const int window = 8;
					bool justDidAltIncSub = false;

					for (int j = Math.Max(0, i - window); j < i; j++)
					{
						CodeInstruction prev = codes[j];
						FieldInfo fi = prev.operand as FieldInfo;
						if (prev.opcode == OpCodes.Ldsfld && fi == FI_AltIncVect)
						{
							justDidAltIncSub = true;
							break;
						}
					}

					if (justDidAltIncSub && MI_ApplyZOffset != null)
					{
						yield return new CodeInstruction(OpCodes.Ldarga_S, (byte)1); // ref Vector3 drawLoc
						yield return new CodeInstruction(OpCodes.Ldarg_0);          // this (CompStatue)
						yield return new CodeInstruction(OpCodes.Call, MI_ApplyZOffset);

						injected = true;
					}
				}
			}
		}
	}
}
