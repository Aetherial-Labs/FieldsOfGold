using FieldsOfGold.Blocks;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FieldsOfGold.Items
{
    class FOGDryGrass : Item
    {
        AssetLocation PileBlockCode => new AssetLocation("fieldsofgold", "haystack");

        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {


			if (blockSel is null) return;
			if (byEntity?.World is null) return;

			BlockPos position = blockSel.Position;
			IPlayer player = null;
		

            if (byEntity.Controls.Sneak && !byEntity.Controls.Sprint && !(api.World.BlockAccessor.GetBlock(blockSel.Position) is FOGHaystack))
            {
                IWorldAccessor world = byEntity.World;
                Block firepitBlock = world.GetBlock(new AssetLocation("firepit-construct1"));
                if (firepitBlock == null) return;

                BlockPos onPos = blockSel.DidOffset ? blockSel.Position : blockSel.Position.AddCopy(blockSel.Face);

                IPlayer byPlayer = byEntity.World.PlayerByUid((byEntity as EntityPlayer)?.PlayerUID);
                if (!byEntity.World.Claims.TryAccess(byPlayer, onPos, EnumBlockAccessFlags.BuildOrBreak))
                {
					return;
                }

				Block block = world.BlockAccessor.GetBlock(onPos.DownCopy());
                Block aimedBlock = world.BlockAccessor.GetBlock(blockSel.Position);
                if (aimedBlock is BlockGroundStorage)
                {
                    if (!(aimedBlock is BlockPitkiln))
                    {
                        BlockPitkiln blockpk = world.GetBlock(new AssetLocation("pitkiln")) as BlockPitkiln;
                        if (blockpk.TryCreateKiln(world, byPlayer, blockSel.Position))
                        {
                            handHandling = EnumHandHandling.PreventDefault;
                        }
                    }
                }
                else
                {
                    string useless = "";

                    if (!block.CanAttachBlockAt(byEntity.World.BlockAccessor, firepitBlock, onPos.DownCopy(), BlockFacing.UP)) return;
                    if (!firepitBlock.CanPlaceBlock(world, byPlayer, new BlockSelection() { Position = onPos, Face = BlockFacing.UP }, ref useless)) return;

                    world.BlockAccessor.SetBlock(firepitBlock.BlockId, onPos);

                    if (firepitBlock.Sounds != null) world.PlaySoundAt(firepitBlock.Sounds.Place, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);

                    itemslot.Itemstack.StackSize--;
                    handHandling = EnumHandHandling.PreventDefaultAction;
                }
            } else if (byEntity.Controls.Sprint && byEntity.Controls.Sneak)
            {
				if (byEntity is EntityPlayer)
				{
					player = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
				}
				if (player == null)
				{
					return;
				}
				if (!byEntity.World.Claims.TryAccess(player, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
				{
					return;
				}
				BlockEntity blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(position);
				if (blockEntity is BlockEntityLabeledChest || blockEntity is BlockEntitySignPost || blockEntity is BlockEntitySign || blockEntity is BlockEntityBloomery || blockEntity is BlockEntityFirepit || blockEntity is BlockEntityForge)
				{
					return;
				}
				if (blockEntity is IBlockEntityItemPile && ((IBlockEntityItemPile)blockEntity).OnPlayerInteract(player))
				{
					handHandling = EnumHandHandling.PreventDefaultAction;
					EntityPlayer entityPlayer = byEntity as EntityPlayer;
					IClientPlayer clientPlayer = ((entityPlayer != null) ? entityPlayer.Player : null) as IClientPlayer;
					if (clientPlayer == null)
					{
						return;
					}
					clientPlayer.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
					return;
				}
				else
				{
					blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(position.AddCopy(blockSel.Face));
					if (blockEntity is IBlockEntityItemPile && ((IBlockEntityItemPile)blockEntity).OnPlayerInteract(player))
					{
						handHandling = EnumHandHandling.PreventDefaultAction;
						EntityPlayer entityPlayer2 = byEntity as EntityPlayer;
						IClientPlayer clientPlayer2 = ((entityPlayer2 != null) ? entityPlayer2.Player : null) as IClientPlayer;
						if (clientPlayer2 == null)
						{
							return;
						}
						clientPlayer2.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
						return;
					}
					else
					{
						Block block = byEntity.World.GetBlock(this.PileBlockCode);
						if (block == null)
						{
							return;
						}
						BlockPos blockPos = position.Copy();
						if (byEntity.World.BlockAccessor.GetBlock(blockPos).Replaceable < 6000)
						{
							blockPos.Add(blockSel.Face, 1);
						}
						bool flag = ((IBlockItemPile)block).Construct(itemslot, byEntity.World, blockPos, player);
						Cuboidf[] collisionBoxes = byEntity.World.BlockAccessor.GetBlock(blockPos).GetCollisionBoxes(byEntity.World.BlockAccessor, blockPos);
						if (collisionBoxes != null && collisionBoxes.Length != 0 && CollisionTester.AabbIntersect(collisionBoxes[0], (double)blockPos.X, (double)blockPos.Y, (double)blockPos.Z, player.Entity.CollisionBox, player.Entity.SidedPos.XYZ))
						{
							player.Entity.SidedPos.Y += (double)collisionBoxes[0].Y2 - (player.Entity.SidedPos.Y - (double)((int)player.Entity.SidedPos.Y));
						}
						if (!flag)
						{
							base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
							return;
						}
						handHandling = EnumHandHandling.PreventDefaultAction;
						EntityPlayer entityPlayer3 = byEntity as EntityPlayer;
						IClientPlayer clientPlayer3 = ((entityPlayer3 != null) ? entityPlayer3.Player : null) as IClientPlayer;
						if (clientPlayer3 == null)
						{
							return;
						}
						clientPlayer3.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
						return;
					}
				}
			}
        }


        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return new WorldInteraction[] {
                new WorldInteraction()
                {
                    HotKeyCode = "sneak",
                    ActionLangCode = "heldhelp-createfirepit",
                    MouseButton = EnumMouseButton.Right,
                },
                new WorldInteraction()
                {
					HotKeyCodes = new string[]
					{
						"sprint",
						"sneak"
					},
					ActionLangCode = "fieldsofgold:handhelp-createhaystack",
                    MouseButton = EnumMouseButton.Right,
                }
            }.Append(base.GetHeldInteractionHelp(inSlot));
        }

    }
}
