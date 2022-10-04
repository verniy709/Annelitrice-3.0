using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Annelitrice
{
	public class BaseHairDef : Def
	{
		//需要有底发的HairDef
		public HairDef hairDef;

		//底发贴图的路径
		public string baseTexPath;

		//底发渲染时的y轴偏移，无特殊情况不用修改
		public float yOffset = 0.001f;

		//当穿有以下装备时隐藏底发
		public List<ThingDef> apparelList = new List<ThingDef>();
	}
}
