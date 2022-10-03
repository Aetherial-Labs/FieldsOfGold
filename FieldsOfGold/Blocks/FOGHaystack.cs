using FieldsOfGold.BlockEntities;
using FieldsOfGold.config;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FieldsOfGold.Blocks
{
    class FOGHaystack : Block, IBlockItemPile
    {
        Cuboidf[][] CollisionBoxesByFillLevel;
        //TODO: Analyze this code to figure out how to give haystacks a dynamic collision box size.
        public FOGHaystack()
        {
            this.CollisionBoxesByFillLevel = new Cuboidf[17][];
            for (int i = 0; i <= 16; i++)
            {
                this.CollisionBoxesByFillLevel[i] = new Cuboidf[]
                {
                    new Cuboidf(0f, 0f, 0f, 1f, (float)i * 0.0625f, 1f)
                };
            }
        }

        //// Token: 0x06000FCC RID: 4044 RVA: 0x000898E0 File Offset: 0x00087AE0
        public int FillLevel(IBlockAccessor blockAccessor, BlockPos pos)
        {
            BlockEntity blockEntity = blockAccessor.GetBlockEntity(pos);
            if (blockEntity is FOGBEHaystack)
            {
                return (int)Math.Ceiling((double)((FOGBEHaystack)blockEntity).OwnStackSize / (((FOGBEHaystack)blockEntity).MaxStackSize/16));
            }
            return 1;
        }

        //// Token: 0x06000FCD RID: 4045 RVA: 0x0000AD94 File Offset: 0x00008F94
        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return this.CollisionBoxesByFillLevel[this.FillLevel(blockAccessor, pos)];
        }

        //// Token: 0x06000FCE RID: 4046 RVA: 0x0000AD94 File Offset: 0x00008F94
        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return this.CollisionBoxesByFillLevel[this.FillLevel(blockAccessor, pos)];
        }

        //private Cuboidf[][] CollisionBoxesByFillLevel;

        public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
        {
            return new BlockDropItemStack[0];
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            // Handled by BlockEntityItemPile
            return new ItemStack[0];
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BlockEntity be = world.BlockAccessor.GetBlockEntity(blockSel.Position);
            if (be is FOGBEHaystack pile)
            {
                return pile.OnPlayerInteract(byPlayer);
            }

            return false;
        }     

        public bool Construct(ItemSlot slot, IWorldAccessor world, BlockPos pos, IPlayer byPlayer)
        {
            Block block = world.BlockAccessor.GetBlock(pos);
            if (!block.IsReplacableBy(this)) return false;
            Block belowBlock = world.BlockAccessor.GetBlock(pos.DownCopy());
            if (!belowBlock.CanAttachBlockAt(world.BlockAccessor, this, pos.DownCopy(), BlockFacing.UP) && belowBlock != this) return false;

            world.BlockAccessor.SetBlock(BlockId, pos);

            BlockEntity be = world.BlockAccessor.GetBlockEntity(pos);
            if (be is FOGBEHaystack pile)
            {
                if (byPlayer?.WorldData.CurrentGameMode != EnumGameMode.Creative)
                {
                    pile.inventory[0].Itemstack = (ItemStack)slot.TakeOut(byPlayer.Entity.Controls.Sprint ? pile.BulkTakeQuantity : pile.DefaultTakeQuantity);
                    slot.MarkDirty();
                }
                else
                {
                    pile.inventory[0].Itemstack = (ItemStack)slot.Itemstack.Clone();
                    pile.inventory[0].Itemstack.StackSize = Math.Min(pile.inventory[0].Itemstack.StackSize, pile.MaxStackSize);
                }

                pile.MarkDirty();
                world.BlockAccessor.MarkBlockDirty(pos);
                world.PlaySoundAt(pile.soundLocation, pos.X, pos.Y, pos.Z, byPlayer, true);
            }
            return true;
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            Block belowBlock = world.BlockAccessor.GetBlock(pos.DownCopy());
            if (!belowBlock.CanAttachBlockAt(world.BlockAccessor, this, pos.DownCopy(), BlockFacing.UP))
            {
                world.BlockAccessor.BreakBlock(pos, null);
            }
        }


        public override bool CanAttachBlockAt(IBlockAccessor blockAccessor, Block block, BlockPos pos, BlockFacing blockFace, Cuboidi attachmentArea = null)
        {
            /*
            BECoinsPile be = blockAccessor.GetBlockEntity(pos) as BECoinsPile;
            if (be != null)
            {
                return be.OwnStackSize == be.MaxStackSize;
            } 
            */
            return base.CanAttachBlockAt(blockAccessor, block, pos, blockFace, attachmentArea);
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return new WorldInteraction[]
            {
                new WorldInteraction
                {
                    ActionLangCode = "fieldsofgold:blockhelp-haystack-addgrass",
                    MouseButton = EnumMouseButton.Right,
                    HotKeyCode = "sneak",
                    Itemstacks = new ItemStack[]
                    {
                        new ItemStack(world.GetItem(new AssetLocation("drygrass")), FieldsOfGoldConfig.Current.dryGrassAddedPerInteract)
                    }
                },
                new WorldInteraction
                {
                    ActionLangCode = "fieldsofgold:blockhelp-haystack-removegrass",
                    MouseButton = EnumMouseButton.Right,
                    HotKeyCode = null
                },
                new WorldInteraction
                {
                    ActionLangCode = "fieldsofgold:blockhelp-haystack-64addgrass",
                    MouseButton = EnumMouseButton.Right,
                    HotKeyCodes = new string[]
                    {
                        "sprint",
                        "sneak"
                    },
                    Itemstacks = new ItemStack[]
                    {
                        new ItemStack(world.GetItem(new AssetLocation("drygrass")), FieldsOfGoldConfig.Current.dryGrassAddedPerInteractWithShiftSneak)
                    }
                },
                new WorldInteraction
                {
                    ActionLangCode = "fieldsofgold:blockhelp-haystack-64removegrass",
                    HotKeyCode = "sprint",
                    MouseButton = EnumMouseButton.Right
                },
                new WorldInteraction
                {
                    ActionLangCode = "fieldsofgold:blockhelp-haystack-makehaybale",
                    HotKeyCode = "sprint",
                    MouseButton = EnumMouseButton.Right,Itemstacks = new ItemStack[]
                    {
                        new ItemStack(world.GetItem(new AssetLocation("drygrass")), FieldsOfGoldConfig.Current.dryGrassPerHaystackBlock),
                        new ItemStack(world.GetItem(new AssetLocation("rope")), 1)
                    }
                },
                new WorldInteraction
                {
                    ActionLangCode = "fieldsofgold:blockhelp-haystack-makemat",
                    HotKeyCode = "sprint",
                    MouseButton = EnumMouseButton.Right,Itemstacks = new ItemStack[]
                    {
                        new ItemStack(world.GetItem(new AssetLocation("drygrass")), FieldsOfGoldConfig.Current.dryGrassPerMat),
                        new ItemStack(world.GetItem(new AssetLocation("cattailtops")), FieldsOfGoldConfig.Current.cattailPerMat)
                    }
                }
            }.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }

    }
}
