using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace AutoDeepDrill
{
	public class CompPropertiesAutoDeepDrill : CompProperties
	{
		public string saveKeysPrefix; // ? idk what this is but I'll leave it alone just to be safe

		public CompPropertiesAutoDeepDrill()
		{
			compClass = typeof(CompAutoDeepDrill);
		}
		
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			if (parentDef.specialDisplayRadius > 4)
			{
				yield return "specialDisplayRadius should be less than or equal to four (4) for AutoDrill to behave properly. Limited by hardcoded loop value in private void FlickOffExhaustedDrillsAroundParent()";
			}
		}
	}

	public class CompAutoDeepDrill : ThingComp
	{
		private bool PowerOn => parent.GetComp<CompPowerTrader>()?.PowerOn ?? false;
		private int ticksUntilSpawn;
		private IntRange stoneChunkQuantity { 
			get {
				switch (parent.def.defName)
				{
					case "autodrill_smart":
						return _settings.StoneChunkQuantitySmartDrill;
					case "autodrill_giant":
						return _settings.StoneChunkQuantityGiantDrill;
					default:
						return _settings.StoneChunkQuantityBasicDrill;
				}
			} 
		}

		private IntRange SpawnIntervalRange
		{
			get
			{
				int minRange, maxRange;
				switch (parent.def.defName)
				{
					case "autodrill_smart":
						minRange = _settings.ResourceSecondsSmartDrill.min;
						maxRange = _settings.ResourceSecondsSmartDrill.max;
						break;
					case "autodrill_giant":
						minRange = _settings.ResourceSecondsGiantDrill.min;
						maxRange = _settings.ResourceSecondsGiantDrill.max;
						break;
					default:
						minRange = _settings.ResourceSecondsBasicDrill.min;
						maxRange = _settings.ResourceSecondsBasicDrill.max;
						break;
				}
				return new IntRange(minRange * 60 , maxRange * 60);
			}
		}
		private float ResourceConsumptionMultiplier
		{ 
			get {
				switch (parent.def.defName)
				{
					case "autodrill_smart":
						return _settings.ResourceConsumptionMultiplierSmartDrill;
					case "autodrill_giant":
						return _settings.ResourceConsumptionMultiplierGiantDrill;
					default:
						return _settings.ResourceConsumptionMultiplierBasicDrill;
				}
			} 
		}
		private float ResourceOutputMultiplier
		{
			get
			{
				switch (parent.def.defName)
				{
					case "autodrill_smart":
						return _settings.ResourceOutputMultiplierSmartDrill;
					case "autodrill_giant":
						return _settings.ResourceOutputMultiplierGiantDrill;
					default:
						return _settings.ResourceOutputMultiplierBasicDrill;
				}
			}
		}

		private AutoDeepDrillSettings _settings = LoadedModManager.GetMod<AutoDeepDrillMod>().GetSettings<AutoDeepDrillSettings>();
		public CompPropertiesAutoDeepDrill _props => (CompPropertiesAutoDeepDrill)props;

		public override void Initialize(CompProperties props)
		{
			if (_settings.InfestationEnabled && !parent.def.defName.EqualsIgnoreCase("autodrill_smart"))
				parent.def.comps.Add(new CompProperties_CreatesInfestations());

			base.Initialize(props);
		}

		private bool GetNextResource(out ThingDef resDef, out int countPresent, out IntVec3 cell)
		{
			for (int i = 0; i < GenRadial.NumCellsInRadius((float)Math.Max(0, parent.def.specialDisplayRadius)); ++i)
			{
				IntVec3 intVec = parent.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(parent.Map))
				{
					ThingDef thingDef = parent.Map.deepResourceGrid.ThingDefAt(intVec);

					if (thingDef != null && thingDef.thingCategories.Contains(ThingCategoryDefOf.ResourcesRaw))
					{
						resDef = thingDef;
						countPresent = parent.Map.deepResourceGrid.CountAt(intVec);
						cell = intVec;
						return true;
					}
				}
			}

			resDef = DeepDrillUtility.GetBaseResource(parent.Map, parent.Position); // **** maybe 1.1 add position. 
			countPresent = int.MaxValue;
			cell = parent.Position;
			return false;
		}

		public override void PostDrawExtraSelectionOverlays()
		{
			if(_settings.ShowResourcesOnSelection)
				parent.Map.deepResourceGrid.MarkForDraw();
		}

		public bool ValuableResourcesPresent()
        {
            return GetNextResource(out _, out _, out _);
		}
		
		private void FlickOffExhaustedDrillsAroundParent(IntVec3 cell)
		{
			//parent.GetComp<CompFlickable>().SwitchIsOn = false; // this line would flick off parent directly, but not other drills which may have also been exhausted by parent.
			//replacing for(radial) with the above line would remove the 4 radius limit, but wont stop overlapping drills. Would be much simpler though...
			for (int i = 0; i < GenRadial.NumCellsInRadius(4 /*(float)Math.Max(1,parent.def.specialDisplayRadius)*/); ++i)
			{// for drills within radius
				IntVec3 c = cell + GenRadial.RadialPattern[i];
				if (c.InBounds(parent.Map))
				{
					ThingWithComps firstThingWithComp = c.GetFirstThingWithComp<CompAutoDeepDrill>(parent.Map);
					if (firstThingWithComp != null && !firstThingWithComp.GetComp<CompAutoDeepDrill>().ValuableResourcesPresent())
					{// exhausted, no more valuable (non-chunk) resources
						firstThingWithComp.GetComp<CompFlickable>().SwitchIsOn = false; // flick since can't forbid
					}
				}
			}
		}
		
		private void FlickCheckResourceExhaustion(IntVec3 cell)
		{
			if (!ValuableResourcesPresent())
			{ // if resource tiles are depleted
				if (DeepDrillUtility.GetBaseResource(parent.Map, cell) == null)
				{ // no fallback resource (like rock chunks)
					Messages.Message("DeepDrillExhaustedNoFallback".Translate(), parent, MessageTypeDefOf.TaskCompletion);
				}
				else
				{
					Messages.Message("DeepDrillExhausted".Translate(Find.ActiveLanguageWorker.Pluralize(DeepDrillUtility.GetBaseResource(parent.Map, cell).label)), parent, MessageTypeDefOf.TaskCompletion);
				}
				FlickOffExhaustedDrillsAroundParent(cell);
			}
		}
		
		private void SpawnStoneChunks(ThingDef resDef)
		{
			int numberToSpawn = stoneChunkQuantity.RandomInRange;
			if (numberToSpawn < 1)
			{
				Messages.Message("DeepDrillExhausted".Translate(), parent, MessageTypeDefOf.TaskCompletion);
				return;
			}
			for (int i = 0; i < numberToSpawn; ++i)
			{ // try to spawn stone chunks
				Thing thingToSpawn = ThingMaker.MakeThing(resDef);
				thingToSpawn.stackCount = 1;
				GenPlace.TryPlaceThing(thingToSpawn, parent.Position, parent.Map, ThingPlaceMode.Near);
			}
		}
		
		private void SpawnValuableResource(ThingDef resDef, int countPresent, IntVec3 cell)
		{
			int resPortion = Math.Min(countPresent, resDef.deepCountPerPortion);
			if (_settings.ConsumesResources)
			{ // decrease remaining tile resource quantity
				parent.Map.deepResourceGrid.SetAt(cell, resDef, Math.Max(0, countPresent - GenMath.RoundRandom(resPortion * ResourceConsumptionMultiplier)));
			}
			Thing thingToSpawn = ThingMaker.MakeThing(resDef); // make thing determined by deep resource definition
			thingToSpawn.stackCount = Math.Max(1, GenMath.RoundRandom((float)resPortion * Find.Storyteller.difficulty.mineYieldFactor * ResourceOutputMultiplier)); // set spawn quantity
			GenPlace.TryPlaceThing(thingToSpawn, parent.Position, parent.Map, ThingPlaceMode.Near); // spawn
		}
		
		public void TrySpawn()
		{
			var nonStoneChunkResource = GetNextResource(out ThingDef resDef, out int countPresent, out IntVec3 cell);
			if (resDef == null) return; // no deep resource found, TODO should probably error
			if (nonStoneChunkResource)
			{
				SpawnValuableResource(resDef, countPresent, cell);
				FlickCheckResourceExhaustion(cell);
			}
			else SpawnStoneChunks(resDef);
			return;
		}
		
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad) ResetTimer();
		}
		
		public override void PostExposeData()
		{
			string str = (!_props.saveKeysPrefix.NullOrEmpty()) ? (_props.saveKeysPrefix + "_") : null; // I  have no idea what this is for
			Scribe_Values.Look(ref ticksUntilSpawn, str + "ticksUntilSpawn", 0); // save state
		}
		
		public void ResetTimer()
		{
			ticksUntilSpawn = SpawnIntervalRange.RandomInRange;
		}
		
		
		private bool CanDrillNow()
		{
			if (!parent.Spawned || !PowerOn) return false;
			if (DeepDrillUtility.GetBaseResource(parent.Map, parent.Position) != null) return true; // this makes many future checks redundant; why, tynan?
			return ValuableResourcesPresent();
            // ****     I think it's just a confusing but efficient way to avoid some check. 
            // ****     the right way is ValuableResourcesPresent() || (DeepDrillUtility.GetBaseResource(parent.Map, parent.Position) != null),
            // **** which checks the resources and then checks the chunks. however, now that it can spawn chunks, it can definitely drill now.
            // ****     so if check the chunks first and it will ignore to check the resources. but I don't think it's an efficiency bottleneck.
        }

        public override void CompTick() => TickInterval(1);

		public override void CompTickRare() => TickInterval(250);

		private void TickInterval(int interval)
		{
			if (CanDrillNow())
			{
				ticksUntilSpawn -= interval;
				if (ticksUntilSpawn <= 0)
				{
					TrySpawn();
					ResetTimer();
				}
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			/*if (!Prefs.DevMode)
				yield return null;
			*/
			if (Prefs.DevMode)
			{
                ThingDef resDef;
                int count;
                IntVec3 cell;
				GetNextResource(out resDef, out count, out cell);
				if (resDef == null)
				{
					yield return new Command_Action
					{
						defaultLabel = "DEBUG: " + "DeepDrillNoResources".Translate(),
						action = delegate {} // nothing to do
					};
				}
				else
				{
					yield return new Command_Action
					{
						defaultLabel = "DEBUG: Drill " + resDef.label,
						action = delegate
						{
							TrySpawn();
							ResetTimer();
						}
					};
				}
			}
		}
		
		public override string CompInspectStringExtra()
		{
			if (parent.Spawned && PowerOn)
            {
                ThingDef resDef;
                int count;
                IntVec3 cell;
                GetNextResource(out resDef, out count, out cell);
				if (resDef == null)
				{
					return "DeepDrillNoResources".Translate();
				}
				else return "ResourceBelow".Translate() + ": " + resDef.LabelCap + "\n" + "NextSpawnedItemIn".Translate(resDef.label) + " " + ticksUntilSpawn.ToStringTicksToPeriod();
			}
			else return null;
		}
	}
}
