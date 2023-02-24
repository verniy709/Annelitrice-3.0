using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace Annelitrice
{
	public class CompApparelSecialTex : ThingComp
	{
		public CompProperties_ApparelSecialTex Props
		{
			get
			{
				return (CompProperties_ApparelSecialTex)props;			
			}
		}

		public List<SpApparelProperties> list = new List<SpApparelProperties>();

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			if (!Props.replaceTexPathList.NullOrEmpty())
			{
				list = Props.replaceTexPathList;
				list.Sort((x, y) => y.level.CompareTo(x.level));
			}
		}
	}


	public class CompProperties_ApparelSecialTex : CompProperties
	{
		public CompProperties_ApparelSecialTex()
		{
			compClass = typeof(CompApparelSecialTex);
		}

		//识别列表
		public List<SpApparelProperties> replaceTexPathList = new List<SpApparelProperties>();
	}


	public class SpApparelProperties : IComparer<SpApparelProperties>
	{
		//需要识别的服装Def
		public ThingDef apparelDef;

		//列表的优先级，数字越大优先级越高
		public int level = 0;

		//贴图路径，格式与服装Def中的wornGraphicPath一致
		public string path;

		//需要用的shader，无特殊要求就不用管
		public Shader shader = null;

		//从大到小排序接口
		public int Compare(SpApparelProperties x, SpApparelProperties y)
		{
			return y.level.CompareTo(x.level);
		}
	}
}
