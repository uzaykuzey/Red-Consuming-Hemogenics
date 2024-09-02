using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RedConsumingHemogenics
{
    public class JobGiver_ConsumeRed: ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!ModsConfig.BiotechActive || !ModSettings.Settings.automaticRedConsume)
            {
                return null;
            }

            Gene_Hemogen gene_Hemogen = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            if (gene_Hemogen == null || !pawn.genes.GenesListForReading.Any(gene => gene.def.defName == "RCH_Gene_RedConsumer"))
            {
                return null;
            }

            if (!gene_Hemogen.ShouldConsumeHemogenNow())
            {
                return null;
            }

            Thing thing = GetRedItem(pawn);
            if (thing != null)
            {
                Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("RCH_ConsumeRed"), thing);
                job.ability = pawn.abilities.GetAbility(DefDatabase<AbilityDef>.GetNamed("RCH_RedConsumer"));
                //job.count = 1;

                return job;
            }
            

            return null;
        }

        public static Thing GetRedItem(Pawn pawn)
        {
            Thing thing = GetRedItem(pawn, ModSettings.Settings.SelectedItem1);
            if(thing==null)
            {
                thing = GetRedItem(pawn, ModSettings.Settings.SelectedItem2);
                if(thing==null)
                {
                    thing = GetRedItem(pawn, ModSettings.Settings.SelectedItem3);
                }
            }
            return thing;
        }

        public static Thing GetRedItem(Pawn pawn, ThingDef thingDef)
        {
            if(thingDef==null || pawn==null)
            {
                return null;
            }

            /*Thing carriedThing = pawn.carryTracker.CarriedThing;

            if (carriedThing != null && carriedThing.def == thingDef)
            {
                return carriedThing;
            }

            if(pawn.inventory != null && pawn.inventory.innerContainer!=null)
            {
                for (int i = 0; i < pawn.inventory.innerContainer.Count; i++)
                {
                    if (pawn.inventory.innerContainer[i].def == ThingDefOf.HemogenPack)
                    {
                        return pawn.inventory.innerContainer[i];
                    }
                }
            }*/



            return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerThings.ThingsOfDef(thingDef), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing t) => pawn.CanReserve(t) && !t.IsForbidden(pawn));
        }

        public override float GetPriority(Pawn pawn)
        {
            try
            {
                if (!ModsConfig.BiotechActive || !ModSettings.Settings.automaticRedConsume)
                {
                    return 0f;
                }

                if (pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>() == null || !pawn.genes.GenesListForReading.Any(gene => gene.def.defName == "RCH_Gene_RedConsumer"))
                {
                    return 0f;
                }

                return 9.2f;
            }
            catch
            {
                return 0f;
            }
        }
    }

    public class JobDriver_ConsumeRed : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            job.count = 1;

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);


            Toil toil = Toils_General.Wait(300);
            toil.WithProgressBarToilDelay(TargetIndex.A);
            toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            if (job.targetA.IsValid)
            {
                toil.FailOnDespawnedOrNull(TargetIndex.A);

            }
            yield return toil;
            Toil use = new Toil();

            use.initAction = delegate
            {
                job.ability.Activate(TargetA, TargetA);
            };
            use.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return use;
            yield break;
        }
    }
}
