using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RedConsumingHemogenics
{
    public class IngestedHemogenPack: IngestionOutcomeDoer
    {
        public string thoughtDefSelfMood;
        public string thoughtDefWitnessMood;
        public string thoughtDefWitnessSocial;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            if(!ModsConfig.IdeologyActive)
            {
                return;
            }
            if(ModSettings.Settings.dontApplyHemogenPackDebuffWhenItsMedical && ((pawn.InBed() && pawn.CurrentBed()?.Medical == true) || (pawn.IsPrisoner && pawn.CurrentBed() != null && pawn.CurrentBed().ForPrisoners) ))
            {
                return;
            }

            GiveToughtsRelatedToPacifistHemogenicPrecept(pawn, thoughtDefSelfMood, thoughtDefWitnessMood, thoughtDefWitnessSocial, true, false);
        }

        public static void GiveToughtsRelatedToPacifistHemogenicPrecept(Pawn pawn, string thoughtDefSelfMood)
        {
            GiveToughtsRelatedToPacifistHemogenicPrecept(pawn, thoughtDefSelfMood, null, null, false, false);
        }

        public static void GiveToughtsRelatedToPacifistHemogenicPrecept(Pawn pawn, string thoughtDefSelfMood, string thoughtDefWitnessMood, string thoughtDefWitnessSocial, bool removeHemogenPackThoughts, bool removeBloodfeedThoughts)
        {
            if (DoPawnHavePreceptHemogenicsPacifist(pawn))
            {
                pawn.needs.mood?.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("RCH_ConsumedRed"));

                if (removeHemogenPackThoughts)
                {
                    pawn.needs.mood?.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("RCH_ConsumedHemogenPack"));
                    pawn.needs.mood?.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("RCH_ConsumedAnimalHemogenPack"));
                    pawn.needs.mood?.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("RCH_ConsumedCorpseHemogenPack"));
                }

                if(removeBloodfeedThoughts)
                {
                    pawn.needs.mood?.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("RCH_BloodfedOnHuman"));
                    pawn.needs.mood?.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("RCH_BloodfedOnAnimal"));
                    pawn.needs.mood?.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("RCH_BloodfedOnCorpse"));
                }

                Precept source = pawn.Ideo.PreceptsListForReading.Find(precept => precept.def.defName == "RCH_Hemogenics_Pacifist");
                pawn.needs.mood?.thoughts.memories.TryGainMemory(ThoughtDef.Named(thoughtDefSelfMood), null, source);
            }

            List<Pawn> allPawns = pawn.Map.mapPawns.AllPawns;

            foreach (Pawn p in allPawns)
            {
                if (!p.Equals(pawn) && p.RaceProps.Humanlike && DoPawnHavePreceptHemogenicsPacifist(p))
                {
                    Precept source = p.Ideo.PreceptsListForReading.Find(precept => precept.def.defName == "RCH_Hemogenics_Pacifist");
                    if (thoughtDefWitnessSocial != null)
                    {
                        p.needs.mood?.thoughts.memories.TryGainMemory(ThoughtDef.Named(thoughtDefWitnessSocial), pawn, source);
                    }
                    if (thoughtDefWitnessMood != null && p.Faction!=null && pawn.Faction!=null && p.Faction.Name.Equals(pawn.Faction.Name))
                    {
                        p.needs.mood?.thoughts.memories.TryGainMemory(ThoughtDef.Named(thoughtDefWitnessMood), null, source);
                    }

                }
            }

        }

        public static bool DoPawnHavePreceptHemogenicsPacifist(Pawn pawn)
        {
            if(pawn==null || pawn.Ideo==null)
            {
                return false;
            }
            return pawn.Ideo.PreceptsListForReading.Any(precept => precept.def.defName == "RCH_Hemogenics_Pacifist");
        }
    }

    public class CompProperties_BloodfeedAbilityMood: CompProperties_AbilityEffect
    {
        public string thoughtDefSelfMood;
        public string thoughtDefWitnessMood;
        public string thoughtDefWitnessSocial;

        public CompProperties_BloodfeedAbilityMood()
        {
            compClass = typeof(CompAbilityEffect_BloodfeedAbilityMoodEffect);
        }
    }

    public class CompAbilityEffect_BloodfeedAbilityMoodEffect : CompAbilityEffect
    {
        public new CompProperties_BloodfeedAbilityMood Props => (CompProperties_BloodfeedAbilityMood)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (!ModsConfig.IdeologyActive)
            {
                return;
            }
            IngestedHemogenPack.GiveToughtsRelatedToPacifistHemogenicPrecept(parent.pawn, Props.thoughtDefSelfMood, Props.thoughtDefWitnessMood, Props.thoughtDefWitnessSocial, false, Props.thoughtDefSelfMood != "RCH_UsedHemosmosis");
        }
    }
}
