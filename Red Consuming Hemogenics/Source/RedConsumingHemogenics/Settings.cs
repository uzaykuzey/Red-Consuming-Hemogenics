using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RedConsumingHemogenics
{
    public class SettingsData : Verse.ModSettings
    {
        public float hemogenConvergenceRate = 30f;
        public string selectedItem1DefName;
        public string selectedItem2DefName;
        public string selectedItem3DefName;
        public bool automaticRedConsume = true;
        public bool dontApplyHemogenPackDebuffWhenItsMedical = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref hemogenConvergenceRate, "hemogenConvergenceRate", 30f);
            Scribe_Values.Look(ref selectedItem1DefName, "selectedItem1DefName");
            Scribe_Values.Look(ref selectedItem2DefName, "selectedItem2DefName");
            Scribe_Values.Look(ref selectedItem3DefName, "selectedItem3DefName");
            Scribe_Values.Look(ref automaticRedConsume, "automaticRedConsume", true);
            Scribe_Values.Look(ref dontApplyHemogenPackDebuffWhenItsMedical, "dontApplyHemogenPackDebuffWhenItsMedical", false);
        }

        public ThingDef SelectedItem1
        {
            get => !string.IsNullOrEmpty(selectedItem1DefName) ? DefDatabase<ThingDef>.GetNamed(selectedItem1DefName, false) : null;
            set => selectedItem1DefName = value?.defName;
        }

        public ThingDef SelectedItem2
        {
            get => !string.IsNullOrEmpty(selectedItem2DefName) ? DefDatabase<ThingDef>.GetNamed(selectedItem2DefName, false) : null;
            set => selectedItem2DefName = value?.defName;
        }

        public ThingDef SelectedItem3
        {
            get => !string.IsNullOrEmpty(selectedItem3DefName) ? DefDatabase<ThingDef>.GetNamed(selectedItem3DefName, false) : null;
            set => selectedItem3DefName = value?.defName;
        }
    }

    public class ModSettings : Mod
    {
        public static SettingsData Settings;
        private string userInput;

        public ModSettings(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SettingsData>();
            userInput = Settings.hemogenConvergenceRate.ToString();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            // Hemogen Convergence Rate
            listingStandard.Label("1 kg of item equals to how much hemogen (default: 30):");
            userInput = listingStandard.TextEntry(userInput);

            if (float.TryParse(userInput, out float parsedValue))
            {
                Settings.hemogenConvergenceRate = parsedValue;
            }
            else
            {
                Settings.hemogenConvergenceRate = 30f;
            }
            if (ModsConfig.IdeologyActive)
            {
                listingStandard.CheckboxLabeled("Don't apply hemogen pack ingestion debuff when it's medical (default: false):", ref Settings.dontApplyHemogenPackDebuffWhenItsMedical, "Deletes the mood and social debuffs a pacifist hemogenic gets when they ingest a hemogen pack because of medical conditions. Only applies to prisoners and colonists in medical beds.");
            }
            listingStandard.CheckboxLabeled("Automatically consume red items (default: true):", ref Settings.automaticRedConsume, "Makes it so pawns will automatically consume red items when their hemogen levels fall below the threshold.");

            listingStandard.Label("Select three items to be consumed when a pawn is below hemogen threshold. 1 is highest priority, 3 is lowest priority:");

            if (listingStandard.ButtonTextLabeled("Item 1: ", Settings.SelectedItem1?.label ?? "None"))
            {
                Find.WindowStack.Add(new FloatMenu(MakeItemOptions(1)));
            }

            if (listingStandard.ButtonTextLabeled("Item 2: ", Settings.SelectedItem2?.label ?? "None"))
            {
                Find.WindowStack.Add(new FloatMenu(MakeItemOptions(2)));
            }

            if (listingStandard.ButtonTextLabeled("Item 3: ", Settings.SelectedItem3?.label ?? "None"))
            {
                Find.WindowStack.Add(new FloatMenu(MakeItemOptions(3)));
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }


        private List<FloatMenuOption> MakeItemOptions(int itemNumber)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                try
                {
                    if (thingDef.category == ThingCategory.Item && Ability_RedConsumer.IsItemColorRed(thingDef) && thingDef.label != "elemental dust")
                    {
                        options.Add(new FloatMenuOption(thingDef.label, delegate
                        {
                            if (itemNumber == 1)
                            {
                                Settings.SelectedItem1 = thingDef;
                            }
                            else if (itemNumber == 2)
                            {
                                Settings.SelectedItem2 = thingDef;
                            }
                            else if (itemNumber == 3)
                            {
                                Settings.SelectedItem3 = thingDef;
                            }
                        }));
                    }
                }
                catch{}
            }

            options.Sort((option1, option2) => -string.Compare(option1.Label, option2.Label, StringComparison.Ordinal));

            options.Add(new FloatMenuOption("None", delegate
            {
                if (itemNumber == 1)
                {
                    Settings.SelectedItem1 = null;
                }
                else if (itemNumber == 2)
                {
                    Settings.SelectedItem2 = null;
                }
                else if (itemNumber == 3)
                {
                    Settings.SelectedItem3 = null;
                }
            }));

            options.Reverse();

            return options;
        }

        public override string SettingsCategory()
        {
            return "Red Consuming Hemogenics";
        }
    }

}