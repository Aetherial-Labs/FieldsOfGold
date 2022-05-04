using FieldsOfGold.Blocks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FieldsOfGold.Items
{
	class FOGDryGrass : ItemDryGrass
	{
		AssetLocation PileBlockCode => new("fieldsofgold", "haystack");

		public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
		{
			if (blockSel == null || (byEntity?.World) == null)
			{
				return;
			}

			EntityPlayer asPlayer = byEntity as EntityPlayer;
			IPlayer byPlayer = byEntity.World.PlayerByUid((asPlayer)?.PlayerUID);
			BlockPos onPos = blockSel.DidOffset ? blockSel.Position : blockSel.Position.AddCopy(blockSel.Face);
			BlockPos position = blockSel.Position;
			

            if (!byEntity.World.Claims.TryAccess(byPlayer, onPos, EnumBlockAccessFlags.BuildOrBreak))
            {
                return;
            }

            
            if (byEntity.Controls.Sneak && byEntity.Controls.Sprint && !(api.World.BlockAccessor.GetBlock(position) is FOGHaystack))
			{
				if (!byEntity.World.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
				{
					return;
				}
				BlockEntity blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(onPos);
				if (blockEntity is BlockEntityLabeledChest || blockEntity is BlockEntitySignPost || blockEntity is BlockEntitySign || blockEntity is BlockEntityBloomery || blockEntity is BlockEntityFirepit || blockEntity is BlockEntityForge)
				{
					return;
				}
				if (blockEntity is IBlockEntityItemPile pile && pile.OnPlayerInteract(byPlayer))
				{
					handHandling = EnumHandHandling.PreventDefaultAction;
					IClientPlayer clientPlayer = ((byPlayer != null) ? (byEntity as EntityPlayer).Player : null) as IClientPlayer;
					clientPlayer.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
					return;
				}
				else
				{
					blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(onPos.AddCopy(blockSel.Face));
					if (blockEntity is IBlockEntityItemPile apile && apile.OnPlayerInteract(byPlayer))
					{
						handHandling = EnumHandHandling.PreventDefaultAction;
                        if (((byEntity is EntityPlayer entityPlayer2) ? entityPlayer2.Player : null) is not IClientPlayer clientPlayer2)
                        {
                            return;
                        }
                        clientPlayer2.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
						return;
					}
					else
					{
						Block blockPile = byEntity.World.GetBlock(this.PileBlockCode);
						if (blockPile == null)
						{
							return;
						}
						BlockPos blockPos = position.Copy();
						if (byEntity.World.BlockAccessor.GetBlock(blockPos).Replaceable < 6000)
						{
							blockPos.Add(blockSel.Face, 1);
						}
						bool flag = ((IBlockItemPile)blockPile).Construct(itemslot, byEntity.World, blockPos, byPlayer);
						Cuboidf[] collisionBoxes = byEntity.World.BlockAccessor.GetBlock(blockPos).GetCollisionBoxes(byEntity.World.BlockAccessor, blockPos);
						if (collisionBoxes != null && collisionBoxes.Length != 0 && CollisionTester.AabbIntersect(collisionBoxes[0], (double)blockPos.X, (double)blockPos.Y, (double)blockPos.Z, byPlayer.Entity.CollisionBox, byPlayer.Entity.SidedPos.XYZ))
						{
							byPlayer.Entity.SidedPos.Y += (double)collisionBoxes[0].Y2 - (byPlayer.Entity.SidedPos.Y - (double)((int)byPlayer.Entity.SidedPos.Y));
						}
						if (!flag)
						{
							base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
							return;
						}
						handHandling = EnumHandHandling.PreventDefaultAction;
                        if (((byEntity is EntityPlayer entityPlayer3) ? entityPlayer3.Player : null) is not IClientPlayer clientPlayer3)
                        {
                            return;
                        }
                        clientPlayer3.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
						return;
					}
				}
			}

            if (byEntity.Controls.Sneak && api.World.BlockAccessor.GetBlock(position) is FOGHaystack)
            {
                //If targeting a haystack while crouching, do not place fireplace
                return;
            }

			base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
		}
	}
}
