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

		public override void ExposeData()
		{
			Scribe_Values.Look(ref censorBool, "censorBool");
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
			listingStandard.Label("Require restarting to take effect".Translate());
			listingStandard.CheckboxLabeled("Censor NSFW Bodytype".Translate(), ref AnnelitriceSettings.censorBool);
			listingStandard.End();
			base.DoSettingsWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "Annelitrice 3.0".Translate();
		}
	}

	public class PatchOperationSetting : PatchOperation
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
}
