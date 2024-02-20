using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Annelitrice
{
    public class ITab_Pawn_Character : ITab
    {
		private Pawn PawnToShowInfoAbout
		{
			get
			{
				return SelThing.TryGetComp<CompContainPawn>().GetDirectlyHeldThings().First() as Pawn;
			}
		}
		public override bool IsVisible
		{
			get
			{
				return true;
			}
		}
		public ITab_Pawn_Character()
		{
			this.labelKey = "TabCharacter_Annelitrice".Translate();
			this.tutorTag = "Character_Annelitrice".Translate();
		}
		protected override void UpdateSize()
		{
			base.UpdateSize();
			this.size = CharacterCardUtility.PawnCardSize(this.PawnToShowInfoAbout) + new Vector2(17f, 17f) * 2f;
		}
		protected override void FillTab()
		{
			this.UpdateSize();
			Vector2 vector = CharacterCardUtility.PawnCardSize(this.PawnToShowInfoAbout);
			CharacterCardUtility.DrawCharacterCard(new Rect(17f, 17f, vector.x, vector.y), this.PawnToShowInfoAbout, null, default(Rect), true);
		}
	}
}
