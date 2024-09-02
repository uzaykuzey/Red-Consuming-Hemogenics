using RimWorld;
using System.Linq;
using Verse;

namespace RedConsumingHemogenics
{
    public class CompUseEffect_ImplantRedConsumer: CompUseEffect
    {
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if(p==null)
            {
                return false;
            }

            Gene_Hemogen hemogenGene = p.genes?.GetGene(GeneDefOf.Hemogenic) as Gene_Hemogen;

            if (hemogenGene==null)
            {
                return "The pawn is not hemogenic.";
            }

            if(p.genes.GenesListForReading.Any(gene => gene.def.defName == "RCH_Gene_RedConsumer"))
            {
                return "The pawn already had red consumer gene.";
            }

            return true;
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            GeneDef geneDef = DefDatabase<GeneDef>.GetNamed("RCH_Gene_RedConsumer");
            usedBy.genes.AddGene(geneDef, true);
        }
    }
}
