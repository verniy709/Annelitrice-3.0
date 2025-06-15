using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.Planet;
using AlienRace;
using AlienRace.ExtendedGraphics;

namespace Annelitrice
{
	public class AnnelitriceConditionIsDead : Condition
	{
		public new const string XmlNameParseKey = "AnnelitriceIsDead";

		public bool isDead;

		public override bool Satisfied(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data) =>
			this.isDead == pawn.WrappedPawn.Dead && !pawn.WrappedPawn.IsShambler && !pawn.WrappedPawn.IsGhoul;
	}
}
