﻿using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FieldsOfGold.Items
{
    class FOGHaybale : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            System.Diagnostics.Debug.WriteLine("Click");
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
			if (!slot.Empty && slot.Itemstack.Item.FirstCodePart() == "knife")
			{
				api.World.BlockAccessor.SetBlock(0, blockSel.Position);

				int haybaleYield = 64;
				for (int i = haybaleYield; i > 0; i--)
				{
					api.World.SpawnItemEntity(new ItemStack(api.World.GetItem(new AssetLocation("drygrass"))), blockSel.Position.ToVec3d() +
						new Vec3d(0, .1, 0));
				}

				if (byPlayer is EntityPlayer player)
					this.DamageItem(api.World, byPlayer.Entity, player.RightHandItemSlot, 1);
			}
            return true;
        }
    }
}