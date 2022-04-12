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
	class FOGDryGrass : ItemDryGrass
	{
		AssetLocation PileBlockCode => new AssetLocation("fieldsofgold", "haystack");

		public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
		{
			EntityPlayer asPlayer = byEntity as EntityPlayer;
			IPlayer byPlayer = byEntity.World.PlayerByUid((asPlayer)?.PlayerUID);
			BlockPos onPos = blockSel.DidOffset ? blockSel.Position : blockSel.Position.AddCopy(blockSel.Face);
			BlockPos position = blockSel.Position;
			System.Diagnostics.Debug.WriteLine("Yo!");
			api.World.Logger.Chat("hitting here 1.");
			if (blockSel == null || ((byEntity != null) ? byEntity.World : null) == null)
            {
                return;
            }

            if (!byEntity.World.Claims.TryAccess(byPlayer, onPos, EnumBlockAccessFlags.BuildOrBreak))
            {
                return;
            }

            if (byEntity.Controls.Sneak && byEntity.Controls.Sprint)
			{
				System.Diagnostics.Debug.WriteLine("Yo!");
				if (!byEntity.World.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
				{
					return;
				}
				BlockEntity blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(onPos);
				if (blockEntity is BlockEntityLabeledChest || blockEntity is BlockEntitySignPost || blockEntity is BlockEntitySign || blockEntity is BlockEntityBloomery || blockEntity is BlockEntityFirepit || blockEntity is BlockEntityForge)
				{
					return;
				}
				if (blockEntity is IBlockEntityItemPile && ((IBlockEntityItemPile)blockEntity).OnPlayerInteract(byPlayer))
				{
					handHandling = EnumHandHandling.PreventDefaultAction;
					IClientPlayer clientPlayer = ((byPlayer != null) ? (byEntity as EntityPlayer).Player : null) as IClientPlayer;
					clientPlayer.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
					return;
				}
				else
				{
					blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(onPos.AddCopy(blockSel.Face));
					if (blockEntity is IBlockEntityItemPile && ((IBlockEntityItemPile)blockEntity).OnPlayerInteract(byPlayer))
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
			base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
		}
	}
}
