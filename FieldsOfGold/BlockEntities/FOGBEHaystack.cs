using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FieldsOfGold.BlockEntities
{
    class FOGBEHaystack : BlockEntityItemPile, IBlockEntityItemPile
    {
        internal AssetLocation soundLocation = new AssetLocation("tradeomat:sounds/coin");

        public override AssetLocation SoundLocation { get { return soundLocation; } }

        public override string BlockCode
        {
            get { return "foghaystack"; }
        }

        public override int DefaultTakeQuantity
        {
            get { return 16; }
        }

        public override int BulkTakeQuantity
        {
            get { return 64; }
        }

        public override int MaxStackSize { get { return 256; } }


        MeshData[] meshes
        {
            get
            {
                return ObjectCacheUtil.GetOrCreate(Api, "haystack-meshes", () =>
                {
                    MeshData[] meshes = new MeshData[17];

                    Block block = Api.World.BlockAccessor.GetBlock(Pos);

                    Shape shape = Api.Assets.TryGet("fieldsofgold:shapes/blocks/utility/haystack.json").ToObject<Shape>();

                    ITesselatorAPI mesher = ((ICoreClientAPI)Api).Tesselator;

                    for (int j = 0; j <= 16; j++)
                    {
                        mesher.TesselateShape(block, shape, out meshes[j], null, j * 9);
                    }

                    return meshes;
                });
            }
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            RandomizeSoundPitch = true;
        }

        public override bool OnTesselation(ITerrainMeshPool meshdata, ITesselatorAPI tesselator)
        {
            lock (inventoryLock)
            {
                int index = Math.Min(256, (int)Math.Ceiling(inventory[0].StackSize / 16.0));
                meshdata.AddMeshData(meshes[index]);
            }

            return true;
        }

        public override bool OnPlayerInteract(IPlayer byPlayer)
        {
            //BlockPos abovePos = Pos.UpCopy();

            //BlockEntity be = Api.World.BlockAccessor.GetBlockEntity(abovePos);
            //if (be is BlockEntityItemPile)
            //{
            //    return ((BlockEntityItemPile)be).OnPlayerInteract(byPlayer);
            //}

            bool sneaking = byPlayer.Entity.Controls.Sneak;

            ItemSlot hotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

                        bool equalStack = hotbarSlot.Itemstack != null && hotbarSlot.Itemstack.Equals(Api.World, inventory[0].Itemstack, GlobalConstants.IgnoredStackAttributes);
                        bool ropeStack = hotbarSlot.Itemstack != null && hotbarSlot.Itemstack.Item.FirstCodePart() == "rope";
                        bool fiberStack = hotbarSlot.Itemstack != null && (hotbarSlot.Itemstack.Item.FirstCodePart() == "cattailtops"|| hotbarSlot.Itemstack.Item.FirstCodePart() == "papyrustops");
            if (sneaking && !equalStack)
            {
                return false;
            }

            if (byPlayer.Entity.Controls.Sprint && ropeStack && inventory[0].Itemstack.StackSize >= 64)
            {
                inventory[0].Itemstack.StackSize = inventory[0].Itemstack.StackSize - 64;
                byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(Api.World.BlockAccessor.GetBlock(new AssetLocation("game:hay-normal"))));
                byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(1);
                if (inventory[0].StackSize <= 0)
                {
                    Api.World.BlockAccessor.SetBlock(0, Pos);
                }
                MarkDirty();
                return true;
            }

            if (byPlayer.Entity.Controls.Sprint && fiberStack && inventory[0].StackSize >= 8)
            {
                inventory[0].Itemstack.StackSize = inventory[0].Itemstack.StackSize - 8;
                byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(Api.World.BlockAccessor.GetBlock(new AssetLocation("fieldsofgold:strawmat-down"))));
                byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(4);
                if (inventory[0].StackSize <= 0)
                {
                    Api.World.BlockAccessor.SetBlock(0, Pos);
                }
                MarkDirty();
                return true;
            }

            if (sneaking && equalStack && OwnStackSize >= MaxStackSize)
            {
                return false;
            }

            lock (inventoryLock)
            {
                if (sneaking)
                {
                    return TryPutItem(byPlayer);
                }
                else
                {
                    return TryTakeItem(byPlayer);
                }
            }
        }
    }
}
