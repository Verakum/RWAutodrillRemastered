using RimWorld;
using UnityEngine;
using Verse;

namespace AutoDeepDrill
{
	public class AutoDeepDrillSettings : ModSettings
	{
		public bool InfestationEnabled = true;
		public bool ShowResourcesOnSelection = false;
		public bool ConsumesResources = true;

		#region Basic Drill Config Variables
		public IntRange StoneChunkQuantityBasicDrill = new IntRange(1, 1);
		public IntRange ResourceSecondsBasicDrill = new IntRange(210, 252);
		public float ResourceConsumptionMultiplierBasicDrill = 0.8f;
		public float ResourceOutputMultiplierBasicDrill = 0.8f;
		#endregion

		#region Giant Drill Config Variables
		public IntRange StoneChunkQuantityGiantDrill = new IntRange(2, 5);
		public IntRange ResourceSecondsGiantDrill = new IntRange(252, 336);
		public float ResourceConsumptionMultiplierGiantDrill = 3f;
		public float ResourceOutputMultiplierGiantDrill = 2f;

		#endregion

		#region Smart Drill Config Variables
		public IntRange StoneChunkQuantitySmartDrill = new IntRange(0, 0);
		public IntRange ResourceSecondsSmartDrill = new IntRange(420, 588);
		public float ResourceConsumptionMultiplierSmartDrill = 0.3f;
		public float ResourceOutputMultiplierSmartDrill = 1f;
		#endregion
		public override void ExposeData()
		{
			Scribe_Values.Look(ref InfestationEnabled, "InfestationEnabled", true);
			Scribe_Values.Look(ref ShowResourcesOnSelection, "ShowResourcesOnSelection", false);
			Scribe_Values.Look(ref ConsumesResources, "ConsumesResources", true);

			Scribe_Values.Look(ref StoneChunkQuantityBasicDrill, "StoneChunkQuantityBasicDrill", new IntRange(1, 1));
			Scribe_Values.Look(ref ResourceSecondsBasicDrill, "ResourceSecondsBasicDrill", new IntRange(210, 252));
			Scribe_Values.Look(ref ResourceConsumptionMultiplierBasicDrill, "ResourceConsumptionMultiplierBasicDrill", 0.8f);
			Scribe_Values.Look(ref ResourceOutputMultiplierBasicDrill, "ResourceOutputMultiplierBasicDrill", 0.8f);

			Scribe_Values.Look(ref StoneChunkQuantityGiantDrill, "StoneChunkQuantityGiantDrill", new IntRange(2, 5));
			Scribe_Values.Look(ref ResourceSecondsGiantDrill, "ResourceSecondsGiantDrill", new IntRange(252, 336));
			Scribe_Values.Look(ref ResourceConsumptionMultiplierGiantDrill, "ResourceConsumptionMultiplierGiantDrill", 3f);
			Scribe_Values.Look(ref ResourceOutputMultiplierGiantDrill, "ResourceOutputMultiplierGiantDrill", 2f);

			Scribe_Values.Look(ref StoneChunkQuantitySmartDrill, "StoneChunkQuantitySmartDrill", new IntRange(0, 0));
			Scribe_Values.Look(ref ResourceSecondsSmartDrill, "ResourceSecondsSmartDrill", new IntRange(420, 588));
			Scribe_Values.Look(ref ResourceConsumptionMultiplierSmartDrill, "ResourceConsumptionMultiplierSmartDrill", 0.3f);
			Scribe_Values.Look(ref ResourceOutputMultiplierSmartDrill, "ResourceOutputMultiplierSmartDrill", 1f);

			base.ExposeData();
		}
	}

	public class AutoDeepDrillMod : Mod
	{
		private AutoDeepDrillSettings _settings;

		private Vector2 _position = new Vector2(0, 0);


		public AutoDeepDrillMod(ModContentPack content) : base(content)
		{
			_settings = GetSettings<AutoDeepDrillSettings>();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{	
			var listingStandard = new Listing_Standard();
			
			var listingRect = new Rect(inRect);
			listingRect.height += 2000f;
			listingRect.width -= 100f;
			listingStandard.Begin(listingRect);

			var viewRec = new Rect(inRect) { y = 0f };
			viewRec.height += 400f;
			viewRec.width -= 400f;
			var outRect = new Rect(inRect) { y = 0f };
			outRect.width += 16f;

			Widgets.BeginScrollView(outRect, ref _position, viewRec);

			listingStandard.Label("GlobalLabel".Translate());
			listingStandard.CheckboxLabeled("InfestationCheckboxLabel".Translate(), ref _settings.InfestationEnabled, "InfestationCheckboxTooltip".Translate());
			listingStandard.SubLabel("InfestationCheckboxSubLabel".Translate(), 100f);
			listingStandard.CheckboxLabeled("ShowResourceCheckboxLabel".Translate(), ref _settings.ShowResourcesOnSelection, "ShowResourceCheckboxTooltip".Translate());
			listingStandard.CheckboxLabeled("ConsumeResourcesCheckboxLabel".Translate(), ref _settings.ConsumesResources, "ConsumeResourcesCheckboxTooltip".Translate());

			listingStandard.Gap();

			#region Basic Drill Config Settings
			var section = listingStandard.BeginSection(250f);
			section.Label("BasicAutoDrillConfigTitle".Translate());
			section.Label("StoneChunksQuantityLabel".Translate());
			section.IntRange(ref _settings.StoneChunkQuantityBasicDrill, 0, 10);
			section.Label("ResourceFrequencyLabel".Translate());
			section.IntRange(ref _settings.ResourceSecondsBasicDrill, 42, 984);
			section.SubLabel("ResourceMinimumFrequencySubLabel".Translate(), 50f);
			section.SubLabel("ResourceMaximumFrequencySubLabel".Translate(), 50f);

			var bufferConsumptionBasic = _settings.ResourceConsumptionMultiplierBasicDrill.ToString();
			section.TextFieldNumericLabeled("ResourceConsumptionQuantity".Translate(), ref _settings.ResourceConsumptionMultiplierBasicDrill, ref bufferConsumptionBasic, 1f);

			var bufferOutputBasic = _settings.ResourceOutputMultiplierBasicDrill.ToString();
			section.TextFieldNumericLabeled("ResourceOutputQuantity".Translate(), ref _settings.ResourceOutputMultiplierBasicDrill, ref bufferOutputBasic, 1f);
			listingStandard.EndSection(section);
			#endregion

			listingStandard.Gap();

			#region Giant Drill Config Settings
			section = listingStandard.BeginSection(250f);
			section.Label("GiantAutoDrillConfigTitle".Translate());
			section.Label("StoneChunksQuantityLabel".Translate());
			section.IntRange(ref _settings.StoneChunkQuantityGiantDrill, 0, 10);
			section.Label("ResourceFrequencyLabel".Translate());
			section.IntRange(ref _settings.ResourceSecondsGiantDrill, 42, 984);
			section.SubLabel("ResourceMinimumFrequencySubLabel".Translate(), 50f);
			section.SubLabel("ResourceMaximumFrequencySubLabel".Translate(), 50f);

			var bufferConsumptionGiant = _settings.ResourceConsumptionMultiplierGiantDrill.ToString();
			section.TextFieldNumericLabeled("ResourceConsumptionQuantity".Translate(), ref _settings.ResourceConsumptionMultiplierGiantDrill, ref bufferConsumptionGiant, 1f);

			var bufferOutputGiant = _settings.ResourceOutputMultiplierGiantDrill.ToString();
			section.TextFieldNumericLabeled("ResourceOutputQuantity".Translate(), ref _settings.ResourceOutputMultiplierGiantDrill, ref bufferOutputGiant, 1f);
			listingStandard.EndSection(section);
			#endregion

			listingStandard.Gap();

			#region Smart Drill Config Settings
			section = listingStandard.BeginSection(250f);
			section.Label("SmartAutoDrillConfigTitle".Translate());
			section.Label("StoneChunksQuantityLabel".Translate());
			section.IntRange(ref _settings.StoneChunkQuantitySmartDrill, 0, 10);
			section.Label("ResourceFrequencyLabel".Translate());
			section.IntRange(ref _settings.ResourceSecondsSmartDrill, 42, 984);
			section.SubLabel("ResourceMinimumFrequencySubLabel".Translate(), 50f);
			section.SubLabel("ResourceMaximumFrequencySubLabel".Translate(), 50f);

			var bufferConsumptionSmart = _settings.ResourceConsumptionMultiplierSmartDrill.ToString();
			section.TextFieldNumericLabeled("ResourceConsumptionQuantity".Translate(), ref _settings.ResourceConsumptionMultiplierSmartDrill, ref bufferConsumptionSmart, 1f);

			var bufferOutputSmart = _settings.ResourceOutputMultiplierSmartDrill.ToString();
			section.TextFieldNumericLabeled("ResourceOutputQuantity".Translate(), ref _settings.ResourceOutputMultiplierSmartDrill, ref bufferOutputSmart, 1f);
			listingStandard.EndSection(section);
			#endregion

			Widgets.EndScrollView();
			listingStandard.End();
			base.DoSettingsWindowContents(inRect);
		}

		/// <summary>
		/// Override SettingsCategory to show up in the list of settings.
		/// Using .Translate() is optional, but does allow for localisation.
		/// </summary>
		/// <returns>The (translated) mod name.</returns>
		public override string SettingsCategory()
		{
			return "ModSettingTitle".Translate();
		}
	}

}
