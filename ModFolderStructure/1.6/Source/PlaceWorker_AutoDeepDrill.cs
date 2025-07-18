using RimWorld;
using Verse;

namespace AutoDeepDrill
{
	public class PlaceWorker_AutoDeepDrill : PlaceWorker_ShowDeepResources
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			ThingDef thingDef = map.deepResourceGrid.ThingDefAt(loc);

			if (thingDef == null)
				return AcceptanceReport.WasAccepted;

			return thingDef.thingCategories.Contains(ThingCategoryDefOf.ResourcesRaw);	
		}
	}
}