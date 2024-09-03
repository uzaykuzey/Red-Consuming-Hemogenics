using Verse;
using RimWorld;
using UnityEngine;

namespace RedConsumingHemogenics
{
    public class Ability_RedConsumer : Ability
    {
        
        public Ability_RedConsumer(){}

        public Ability_RedConsumer(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public Ability_RedConsumer(Pawn pawn, Precept sourcePrecept)
        {
            this.pawn = pawn;
            this.sourcePrecept = sourcePrecept;
        }

        public Ability_RedConsumer(Pawn pawn, AbilityDef def)
        {
            this.pawn = pawn;
            this.def = def;
            Initialize();
            
        }

        public Ability_RedConsumer(Pawn pawn, Precept sourcePrecept, AbilityDef def)
        {
            this.pawn = pawn;
            this.def = def;
            this.sourcePrecept = sourcePrecept;
            Initialize();
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Activate(target, dest);

            return Activate(target, dest, pawn);
        }

        public static bool Activate(LocalTargetInfo target, LocalTargetInfo dest, Pawn pawn)
        {
            if (target.Thing is ThingWithComps item)
            {
                if (item is Corpse _)
                {
                    Messages.Message("This ability can't be used on corpses.", MessageTypeDefOf.RejectInput);
                    return false;
                }
                if (IsItemColorRed(item))
                {
                    float itemWeight = item.GetStatValue(StatDefOf.Mass);
                    float hemogenFromSingleItem = (itemWeight * ModSettings.Settings.hemogenConvergenceRate / 100.0f);
                    Pawn casterPawn = pawn;
                    if (casterPawn != null && casterPawn.health != null)
                    {
                        Gene_Hemogen hemogenGene = pawn.genes?.GetGene(GeneDefOf.Hemogenic) as Gene_Hemogen;
                        if (hemogenGene != null)
                        {
                            if (IsHemogenPack(item.def))
                            {
                                IngestedHemogenPack.GiveToughtsRelatedToPacifistHemogenicPrecept(pawn, "RCH_ConsumedHemogenPackColor");
                            }
                            else if (IngestedHemogenPack.DoPawnHavePreceptHemogenicsPacifist(casterPawn))
                            {
                                casterPawn.needs.mood?.thoughts.memories.TryGainMemory(ThoughtDef.Named("RCH_ConsumedRed"), null, casterPawn.Ideo.PreceptsListForReading.Find(precept => precept.def.defName == "RCH_Hemogenics_Pacifist"));
                            }
                            float requiredHemogenAmount = (hemogenGene.Max - hemogenGene.Value);
                            float hemogenGain;
                            int stackUse;
                            if (requiredHemogenAmount <= hemogenFromSingleItem * item.stackCount)
                            {
                                hemogenGain = requiredHemogenAmount;
                                stackUse = (int)Mathf.Ceil(requiredHemogenAmount / hemogenFromSingleItem);
                            }
                            else
                            {
                                hemogenGain = hemogenFromSingleItem * item.stackCount;
                                stackUse = item.stackCount;
                            }
                            hemogenGene.Value += hemogenGain;
                            int hemogenGainAmountInt = (int)Mathf.Floor(hemogenGain * 100);
                            Messages.Message($"The pawn's hemogen level increased by {hemogenGainAmountInt} due to consuming the color of {item.LabelNoCount} x{stackUse}.", MessageTypeDefOf.PositiveEvent);
                            if (stackUse >= item.stackCount)
                            {
                                item.Destroy();
                            }
                            else
                            {
                                item.stackCount -= stackUse;
                            }

                        }
                    }
                    return true;
                }
                else
                {
                    Messages.Message("The item is not red, so no hemogen can be gained.", MessageTypeDefOf.RejectInput);
                    return false;
                }
            }
            else
            {
                Messages.Message("Target is not an item.", MessageTypeDefOf.RejectInput);
                return false;
            }
        }

        public static bool IsHemogenPack(ThingDef thingDef)
        {
            return thingDef.defName == "HemogenPack" || thingDef.defName == "VRE_HemogenPack_Animal" ||
                thingDef.defName == "VRE_HemogenPack_Corpse" || thingDef.defName == "VRE_HemogenPack_Sanguophage";
        }

        public static bool IsItemColorRed(ThingDef thingDef)
        {
            if(thingDef==null)
            {
                return false;
            }

            if(thingDef.IsCorpse)
            {
                return false;
            }

            if(thingDef.comps.Any(comp => comp.compClass.Name == "RedConsumingHemogenics_RedItemThingComp"))
            {
                return ((RedConsumingHemogenics_RedItemProperty) thingDef.comps.Find(comp => comp.compClass.Name == "RedConsumingHemogenics_RedItemThingComp")).isRed;
            }

            if (thingDef.defName == "RawBerries" || IsHemogenPack(thingDef))
            {
                return true;
            }

            Texture2D originalTexture = GetTexture2D(thingDef);
            Texture2D texture = MakeTextureReadable(originalTexture);

            if (texture == null)
            {
                return false;
            }

            return TextureIsRed(texture);
        }

        public static Texture2D GetTexture2D(ThingDef thingDef)
        {
            if (thingDef.graphicData == null)
            {
                return null;
            }

            Graphic graphic = thingDef.graphic;

            if (graphic == null)
            {
                graphic = thingDef.graphicData.Graphic;
                if (graphic == null)
                {
                    return null;
                }
            }

            Material material = graphic.MatSingle;
            if (material == null)
            {
                return null;
            }

            Texture2D texture = material.mainTexture as Texture2D;
            if (texture == null)
            {
                return null;
            }

            return texture;
        }

        public static bool IsItemColorRed(Thing thing)
        {
            if(thing is Corpse _)
            {
                return false;
            }
            if (!(thing is ThingWithComps _))
            {
                Log.Error("Selected thing is not an item.");
                return false;
            }

            if (thing.DrawColor != Color.white)
            {
                return IsColorRed(thing.DrawColor);
            }

            if (thing.Stuff != null)
            {
                return IsColorRed(thing.Stuff.stuffProps.color);
            }

            return IsItemColorRed(thing.def);
        }

        public static Texture2D MakeTextureReadable(Texture2D originalTexture)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                originalTexture.width,
                originalTexture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(originalTexture, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;

            Texture2D readableTexture = new Texture2D(originalTexture.width, originalTexture.height);
            readableTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableTexture;
        }

        private static bool TextureIsRed(Texture2D texture)
        {
            Color[] pixels = texture.GetPixels();
            int totalNoOfRed = 0;
            int count = 0;

            foreach (Color pixel in pixels)
            {
                if (pixel.a > 0.86f)
                {
                    totalNoOfRed += IsColorRed(pixel) ? 1:0;
                    count++;
                }
            }
            return count == 0 ? false : ((totalNoOfRed*100) / count > 20);
        }


        public static bool IsColorRed(Color color)
        {
            return color.r>1.75f*color.b && color.r>1.75f*color.g;
        }
    }

    public class RedConsumingHemogenics_RedItemProperty: CompProperties 
    {
        public bool isRed;
        public RedConsumingHemogenics_RedItemProperty()
        {
            compClass = typeof(RedConsumingHemogenics_RedItemThingComp);
        }
    }

    public class RedConsumingHemogenics_RedItemThingComp: ThingComp 
    {
        public RedConsumingHemogenics_RedItemProperty Props => (RedConsumingHemogenics_RedItemProperty)props;
    }
}

