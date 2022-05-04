using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace FieldsOfGold.Items
{
    class FOGHaybale : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
			if (!slot.Empty && slot.Itemstack.Collectible.FirstCodePart() == "knife")
			{
				api.World.BlockAccessor.SetBlock(0, blockSel.Position);

				int haybaleYield = (256/8);
				for (int i = haybaleYield; i > 0; i--)
				{
					api.World.SpawnItemEntity(new ItemStack(api.World.GetItem(new AssetLocation("drygrass")),8), blockSel.Position.ToVec3d() +
						new Vec3d(0, 0.1, 0));
				}              
                slot.Itemstack.Item.DamageItem(world, byPlayer.Entity, slot);
                return true;
			}
			
			return base.OnBlockInteractStart(world, byPlayer, blockSel);
		}

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            List<ItemStack> canAddKnifeStacks = new();
            foreach (CollectibleObject obj in api.World.Collectibles)
            {         
                ICoreClientAPI capi = api as ICoreClientAPI;

                if (obj is Item && obj.FirstCodePart() == "knife")
                {
                    List<ItemStack> stacks = obj.GetHandBookStacks(capi);
                    if (stacks != null) canAddKnifeStacks.AddRange(stacks);
                }
            }

            return new WorldInteraction[]
            {
             

                new WorldInteraction
                {
                  ActionLangCode = "fieldsofgold:blockhelp-haybale",
                  MouseButton = EnumMouseButton.Right,
                  Itemstacks = canAddKnifeStacks.ToArray()
                }
            }.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
    }
}
