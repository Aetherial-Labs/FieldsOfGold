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
            
            bool sneaking = byPlayer.Entity.Controls.Sneak;
            ItemSlot hotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            ItemStack hotbarStack = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack;

                        bool equalStack = hotbarStack != null && hotbarStack.Equals(Api.World, inventory[0].Itemstack, GlobalConstants.IgnoredStackAttributes);
                        bool ropeStack = hotbarStack != null && hotbarStack.Collectible.FirstCodePart() == "rope";
                        bool fiberStack = hotbarStack != null && (hotbarStack.Collectible.FirstCodePart() == "cattailtops"|| hotbarStack.Collectible.FirstCodePart() == "papyrustops");
            
            if (sneaking && !equalStack)
            {
                return false;
            }

            if (byPlayer.Entity.Controls.Sprint && ropeStack && inventory[0].Itemstack.StackSize >= 64)
            {
                ItemStack haystack = new ItemStack(Api.World.BlockAccessor.GetBlock(new AssetLocation("game:hay-normal")));
                inventory[0].Itemstack.StackSize = inventory[0].Itemstack.StackSize - 64;
                if (!byPlayer.InventoryManager.TryGiveItemstack(haystack))
                {
                    byPlayer.Entity.World.SpawnItemEntity(haystack, byPlayer.Entity.Pos.XYZ.AddCopy(0, 0.5, 0));
                }

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
                ItemStack strawmat = new ItemStack(Api.World.BlockAccessor.GetBlock(new AssetLocation("fieldsofgold:strawmat-down")));

                if (hotbarStack.StackSize < 4)
                {
                    (byPlayer.Entity.World.Api as ICoreClientAPI)?.TriggerIngameError(this, "notenoughfiber", Lang.Get("fieldsofgold:haystacktoofewfibers"));
                    return false;
                }

                inventory[0].Itemstack.StackSize = inventory[0].Itemstack.StackSize - 8;
                if (!byPlayer.InventoryManager.TryGiveItemstack(strawmat))
                {
                   byPlayer.Entity.World.SpawnItemEntity(strawmat, byPlayer.Entity.Pos.XYZ.AddCopy(0, 0.5, 0));
                }

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
