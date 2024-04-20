using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Xml;

namespace Annelitrice
{
	public class AnnelitriceSettings : ModSettings
	{
		public static bool censorBool = false;
		public static bool removeRaceRestrictionBool = false;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref censorBool, "censorBool");
			Scribe_Values.Look(ref removeRaceRestrictionBool, "removeRaceRestrictionBool");
			base.ExposeData();
		}
	}

	public class AnnelitriceMod : Mod
	{
		AnnelitriceSettings settings;

		public AnnelitriceMod(ModContentPack content) : base(content)
		{
			this.settings = GetSettings<AnnelitriceSettings>();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.Begin(inRect);
			listingStandard.Label("Anneli.SettingLabel".Translate());
			listingStandard.CheckboxLabeled("Anneli.CensoredBodytypeCheckbox".Translate(), ref AnnelitriceSettings.censorBool);
			listingStandard.CheckboxLabeled("Anneli.RemoveRaceRestrictionCheckbox".Translate(), ref AnnelitriceSettings.removeRaceRestrictionBool);
			listingStandard.End();
			base.DoSettingsWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "Anneli.SettingTitle".Translate();
		}
	}

	public class PatchOperationSettingA : PatchOperation
	{
		private List<PatchOperation> operations = new List<PatchOperation>();

		private PatchOperation lastFailedOperation;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			if (AnnelitriceSettings.censorBool)
			{
				foreach (PatchOperation operation in operations)
				{
					if (!operation.Apply(xml))
					{
						lastFailedOperation = operation;
						return false;
					}
				}
			}
			return true;
		}
	}

	public class PatchOperationSettingB : PatchOperation
	{
		private List<PatchOperation> operations = new List<PatchOperation>();

		private PatchOperation lastFailedOperation;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			if (AnnelitriceSettings.removeRaceRestrictionBool)
			{
				foreach (PatchOperation operation in operations)
				{
					if (!operation.Apply(xml))
					{
						lastFailedOperation = operation;
						return false;
					}
				}
			}
			return true;
		}
	}
}
