using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FieldsOfGold.Blocks
{
    class FOGReeds : BlockReeds
    {
        WorldInteraction[] interactions = null;
        string climateColorMapInt;
        string seasonColorMapInt;

        private int habitat = EnumReedsHabitat.Land;

        public override void OnLoaded(ICoreAPI api)
        {

            string habitat = Variant["habitat"];
            if (habitat == "water") this.habitat = EnumReedsHabitat.Water;
            else if (habitat == "ice") this.habitat = EnumReedsHabitat.Ice;
            if (LastCodePart() == "harvested") return;

            interactions = ObjectCacheUtil.GetOrCreate(api, "reedsBlockInteractions", () =>
            {
                List<ItemStack> knifeStacklist = new();

                foreach (Item item in api.World.Items)
                {
                    if (item.Code == null) continue;

                    if (item.Tool == EnumTool.Knife)
                    {
                        knifeStacklist.Add(new ItemStack(item));
                    }
                }
                return new WorldInteraction[] {
                        new WorldInteraction()
                        {
                            ActionLangCode = "blockhelp-reeds-harvest",
                            MouseButton = EnumMouseButton.Left,
                            Itemstacks = knifeStacklist.ToArray()
                        }
                };
            });
        }


        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {

            if (!CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
            {
                return false;
            }

            Block block = world.BlockAccessor.GetBlock(blockSel.Position);
            Block blockToPlace = world.GetBlock(CodeWithVariant("state", "growing"));

            if (blockToPlace != null)
            {
                if (CanPlantStay(world.BlockAccessor, blockSel.Position))
                {
                    world.BlockAccessor.SetBlock(blockToPlace.BlockId, blockSel.Position);
                    return true;
                }
                else
                {
                    failureCode = "requirefertileground";
                    return false;
                }

            }

            return false;
        }



        public override bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            if (this.Variant.ContainsKey("side"))
            {
                BlockFacing blockFacing = BlockFacing.FromCode(this.Variant["side"]);
                BlockPos pos2 = pos.AddCopy(blockFacing);
                return blockAccessor.GetBlock(pos2).CanAttachBlockAt(blockAccessor, this, pos2, blockFacing.Opposite, null);
            }
            Block block = blockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z);
            if (block.Fertility <= 0)
            {
                return false;
            }
            block = blockAccessor.GetLiquidBlock(pos);
            return block.LiquidLevel < 7 && !block.SideSolid.Any();
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
 
            if (world.Side == EnumAppSide.Server && (byPlayer == null || byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative))
            {
                bool isReed = Variant["type"] == "coopersreed";
                ItemStack drop = null;
                if (Variant["state"] == "normal")
                {
                    drop = new ItemStack(world.GetItem(new AssetLocation(isReed ? "cattailtops" : "papyrustops")));
                }
                else if (Variant["state"] == "harvested")
                {
                    drop = new ItemStack(world.GetItem(new AssetLocation(isReed ? "cattailroot" : "papyrusroot")));
                }
                else if (Variant["state"] == "growing")
                {
                    drop = null;
                }
                if (drop != null)
                {
                    world.SpawnItemEntity(drop, new Vec3d(pos.X + 0.5, pos.Y + 0.5, pos.Z + 0.5), null);
                }

                world.PlaySoundAt(Sounds.GetBreakSound(byPlayer), pos.X, pos.Y, pos.Z, byPlayer);
            }

            if (byPlayer != null && Variant["state"] == "normal" && (byPlayer.InventoryManager.ActiveTool == EnumTool.Knife || byPlayer.InventoryManager.ActiveTool == EnumTool.Sickle || byPlayer.InventoryManager.ActiveTool == EnumTool.Scythe))
            {
                world.BlockAccessor.SetBlock(world.GetBlock(this.habitat == EnumReedsHabitat.Ice ? CodeWithVariants(new string[] { "habitat", "state" }, new string[] { "water", "harvested" }) : CodeWithVariant("state", "harvested")).BlockId, pos);
                return;
            }

            if (habitat != 0)
            {
                world.BlockAccessor.SetBlock(world.GetBlock(new AssetLocation("water-still-7")).BlockId, pos);
                world.BlockAccessor.GetBlock(pos).OnNeighbourBlockChange(world, pos, pos);
            }
            else
            {
                world.BlockAccessor.SetBlock(0, pos);
            }
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            if (this.Variant["state"] == "growing")
            {
                return new WorldInteraction[] {
                new WorldInteraction()
                {
                    ActionLangCode = "fieldsofgold:blockhelp-growing-reeds-harvest"
                }
            };

            }
            else
            {
               return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
            }
        }
    }

    public class EnumReedsHabitat
    {
        public const int Land = 0;
        public const int Water = 1;
        public const int Ice = 2;
    }

}
