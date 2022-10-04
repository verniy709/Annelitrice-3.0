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
    [StaticConstructorOnStartup]
    public static class Textures
    {
        //public static readonly Texture2D BackgroundMenu = ContentFinder<Texture2D>.Get("Anneli_UI/UI_Background");
        //public static Dictionary<BodyPartDef, Dictionary<HediffDef, Texture2D>> leftPairedAppendanges = new Dictionary<BodyPartDef, Dictionary<HediffDef, Texture2D>>();
        //public static Dictionary<BodyPartDef, Dictionary<HediffDef, Texture2D>> rightPairedAppendanges = new Dictionary<BodyPartDef, Dictionary<HediffDef, Texture2D>>();
        //public static Dictionary<BodyPartDef, Dictionary<HediffDef, Texture2D>> pairedAppendanges = new Dictionary<BodyPartDef, Dictionary<HediffDef, Texture2D>>();
        //static Textures()
        //{
        //    var leftArmAppendages = new Dictionary<HediffDef, Texture2D>();
        //    leftArmAppendages[AnnelitriceDefOf.Annely_BoneBlade] = Arm_Left_BoneBlade_UI;
        //    leftArmAppendages[AnnelitriceDefOf.Annely_BoneSpike] = Arm_Left_BoneSpike_UI;
        //    leftArmAppendages[AnnelitriceDefOf.Annely_CrushingArm] = Arm_Left_CrushingArm_UI;
        //    leftArmAppendages[AnnelitriceDefOf.Annely_Tentacle] = Arm_Left_Tentacle_UI;
        //    leftPairedAppendanges[BodyPartDefOf.Arm] = leftArmAppendages;
        //
        //    var leftLegAppendages = new Dictionary<HediffDef, Texture2D>();
        //    leftLegAppendages[AnnelitriceDefOf.Annely_Arachno] = Leg_Left_Arachno_UI;
        //    leftLegAppendages[AnnelitriceDefOf.Annely_DollJoint] = Leg_Left_DollJoint_UI;
        //    leftLegAppendages[AnnelitriceDefOf.Annely_Lumps] = Leg_Left_Lumps_UI;
        //    leftLegAppendages[AnnelitriceDefOf.Annely_VenomGland] = Leg_Left_VenomGland_UI;
        //    leftPairedAppendanges[BodyPartDefOf.Leg] = leftLegAppendages;
        //
        //    var rightArmAppendages = new Dictionary<HediffDef, Texture2D>();
        //    rightArmAppendages[AnnelitriceDefOf.Annely_BoneBlade] = Arm_Right_BoneBlade_UI;
        //    rightArmAppendages[AnnelitriceDefOf.Annely_BoneSpike] = Arm_Right_BoneSpike_UI;
        //    rightArmAppendages[AnnelitriceDefOf.Annely_CrushingArm] = Arm_Right_CrushingArm_UI;
        //    rightArmAppendages[AnnelitriceDefOf.Annely_Tentacle] = Arm_Right_Tentacle_UI;
        //    rightPairedAppendanges[BodyPartDefOf.Arm] = rightArmAppendages;
        //
        //    var rightLegAppendages = new Dictionary<HediffDef, Texture2D>();
        //    rightLegAppendages[AnnelitriceDefOf.Annely_Arachno] = Leg_Right_Arachno_UI;
        //    rightLegAppendages[AnnelitriceDefOf.Annely_DollJoint] = Leg_Right_DollJoint_UI;
        //    rightLegAppendages[AnnelitriceDefOf.Annely_Lumps] = Leg_Right_Lumps_UI;
        //    rightLegAppendages[AnnelitriceDefOf.Annely_VenomGland] = Leg_Right_VenomGland_UI;
        //    rightPairedAppendanges[BodyPartDefOf.Leg] = rightLegAppendages;
        //
        //    var backAppendages = new Dictionary<HediffDef, Texture2D>();
        //    backAppendages[AnnelitriceDefOf.Annely_BoneWing] = Back_BoneWing_UI;
        //    backAppendages[AnnelitriceDefOf.Annely_Lump] = Back_Lump_UI;
        //    backAppendages[AnnelitriceDefOf.Annely_PatagiumWing] = Back_PatagiumWing_UI;
        //    backAppendages[AnnelitriceDefOf.Annely_BackTentacle] = Back_Tentacle_UI;
        //    pairedAppendanges[AnnelitriceDefOf.Spine] = backAppendages;
        //
        //    var headAppendages = new Dictionary<HediffDef, Texture2D>();
        //    headAppendages[AnnelitriceDefOf.Annely_Kuchisake] = Head_Kuchisake_UI;
        //    headAppendages[AnnelitriceDefOf.Annely_Piercing] = Head_Piercing_UI;
        //    headAppendages[AnnelitriceDefOf.Annely_ProliferatingBrain] = Head_ProliferatingBrain_UI;
        //    headAppendages[AnnelitriceDefOf.Annely_ThirdEye] = Head_ThirdEye_UI;
        //    pairedAppendanges[BodyPartDefOf.Head] = headAppendages;
        //
        //    var peritoneumAppendages = new Dictionary<HediffDef, Texture2D>();
        //    peritoneumAppendages[AnnelitriceDefOf.Annely_Banelings] = Torso_Banelings_UI;
        //    peritoneumAppendages[AnnelitriceDefOf.Annely_Boomalope] = Torso_Boomalope_UI;
        //    peritoneumAppendages[AnnelitriceDefOf.Annely_GastrovascularCavity] = Torso_GastrovascularCavity_UI;
        //    peritoneumAppendages[AnnelitriceDefOf.Annely_Piercing] = Torso_Piercing_UI;
        //    pairedAppendanges[BodyPartDefOf.Torso] = peritoneumAppendages;
        //}

        //public static readonly Texture2D Back_BoneWing_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Back/BoneWing_UI");
        //public static readonly Texture2D Back_Lump_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Back/Lump_UI");
        //public static readonly Texture2D Back_PatagiumWing_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Back/PatagiumWing_UI");
        //public static readonly Texture2D Back_Tentacle_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Back/Tentacle_UI");
        //public static readonly Texture2D Head_Kuchisake_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Head/Kuchisake_UI");
        //public static readonly Texture2D Head_Piercing_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Head/Piercing_UI");
        //public static readonly Texture2D Head_ProliferatingBrain_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Head/ProliferatingBrain_UI");
        //public static readonly Texture2D Head_ThirdEye_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Head/ThirdEye_UI");
        //public static readonly Texture2D Torso_Banelings_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Peritoneum/Banelings_UI");
        //public static readonly Texture2D Torso_Boomalope_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Peritoneum/Boomalope_UI");
        //public static readonly Texture2D Torso_GastrovascularCavity_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Peritoneum/GastrovascularCavity_UI");
        //public static readonly Texture2D Torso_Piercing_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Peritoneum/Piercing_UI");
        //
        //public static readonly Texture2D Arm_Left_BoneBlade_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Arm_Left/BoneBlade_UI");
        //public static readonly Texture2D Arm_Left_BoneSpike_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Arm_Left/BoneSpike_UI");
        //public static readonly Texture2D Arm_Left_CrushingArm_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Arm_Left/CrushingArm_UI");
        //public static readonly Texture2D Arm_Left_Tentacle_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Arm_Left/Tentacle_UI");
        //public static readonly Texture2D Leg_Left_Arachno_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Leg_Left/Arachno_UI");
        //public static readonly Texture2D Leg_Left_DollJoint_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Leg_Left/DollJoint_UI");
        //public static readonly Texture2D Leg_Left_Lumps_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Leg_Left/Lumps_UI");
        //public static readonly Texture2D Leg_Left_VenomGland_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Leg_Left/VenomGland_UI");
        //public static readonly Texture2D Leg_Right_Arachno_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Leg_Right/Arachno_UI");
        //public static readonly Texture2D Leg_Right_DollJoint_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Leg_Right/DollJoint_UI");
        //public static readonly Texture2D Leg_Right_Lumps_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Leg_Right/Lumps_UI");
        //public static readonly Texture2D Leg_Right_VenomGland_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Leg_Right/VenomGland_UI");
        //public static readonly Texture2D Arm_Right_BoneBlade_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Arm_Right/BoneBlade_UI");
        //public static readonly Texture2D Arm_Right_BoneSpike_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Arm_Right/BoneSpike_UI");
        //public static readonly Texture2D Arm_Right_CrushingArm_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Arm_Right/CrushingArm_UI");
        //public static readonly Texture2D Arm_Right_Tentacle_UI = ContentFinder<Texture2D>.Get("Anneli_UI/Appendages_UI/Arm_Right/Tentacle_UI");

        //public static readonly Texture2D Blue_Minus = ContentFinder<Texture2D>.Get("Anneli_UI/Blue_Minus");
        //public static readonly Texture2D Blue_Plus = ContentFinder<Texture2D>.Get("Anneli_UI/Blue_Plus");
        //public static readonly Texture2D Green_Minus = ContentFinder<Texture2D>.Get("Anneli_UI/Green_Minus");
        //public static readonly Texture2D Green_Plus = ContentFinder<Texture2D>.Get("Anneli_UI/Green_Plus");
        //public static readonly Texture2D Heal_Button = ContentFinder<Texture2D>.Get("Anneli_UI/Heal_Button");
        //public static readonly Texture2D Red_Minus = ContentFinder<Texture2D>.Get("Anneli_UI/Red_Minus");
        //public static readonly Texture2D Red_Plus = ContentFinder<Texture2D>.Get("Anneli_UI/Red_Plus");
        //public static readonly Texture2D UI_Evolution_Points = ContentFinder<Texture2D>.Get("Anneli_UI/UI_Evolution_Points");

        //public static readonly Texture2D Red_Plus_Hovering = ContentFinder<Texture2D>.Get("Anneli_UI/Red_Plus_Hovering");
        //public static readonly Texture2D Red_Minus_Hovering = ContentFinder<Texture2D>.Get("Anneli_UI/Red_Minus_Hovering");
        //public static readonly Texture2D Green_Plus_Hovering = ContentFinder<Texture2D>.Get("Anneli_UI/Green_Plus_Hovering");
        //public static readonly Texture2D Green_Minus_Hovering = ContentFinder<Texture2D>.Get("Anneli_UI/Green_Minus_Hovering");
        //public static readonly Texture2D Blue_Plus_Hovering = ContentFinder<Texture2D>.Get("Anneli_UI/Blue_Plus_Hovering");
        //public static readonly Texture2D Blue_Minus_Hovering = ContentFinder<Texture2D>.Get("Anneli_UI/Blue_Minus_Hovering");
        //public static readonly Texture2D Heal_Button_Hovering = ContentFinder<Texture2D>.Get("Anneli_UI/Heal_Button_Hovering");
        //public static readonly Texture2D Empty_Slot_Hovering = ContentFinder<Texture2D>.Get("Anneli_UI/Empty_Slot_Hovering");

    }
}
