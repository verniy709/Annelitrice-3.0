using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Annelitrice
{
	public class CompContainPawn : ThingComp, IThingHolder
	{
		public CompContainPawn()
		{
			innerContainer = new ThingOwner<Pawn>(this, true, LookMode.Deep);
		}
		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}
        public override void PostExposeData()
        {
			Scribe_Deep.Look(ref innerContainer, "CompContainPawn_innerContainer", new object[] { this });
		}
		private ThingOwner<Pawn> innerContainer;
	}
}
