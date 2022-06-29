using FieldsOfGold.config;
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
        internal AssetLocation soundLocation = new("game:sounds/blocks/plant.ogg");

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


        MeshData[] Meshes
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
                meshdata.AddMeshData(Meshes[index]);
            }

            return true;
        }

        public override bool OnPlayerInteract(IPlayer byPlayer)
        {
            
            bool sneaking = byPlayer.Entity.Controls.Sneak;
            ItemStack hotbarStack = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack;

                        bool equalStack = hotbarStack != null && hotbarStack.Equals(Api.World, inventory[0].Itemstack, GlobalConstants.IgnoredStackAttributes);
                        bool ropeStack = hotbarStack != null && hotbarStack.Collectible.FirstCodePart() == "rope";
                        bool fiberStack = hotbarStack != null && (hotbarStack.Collectible.FirstCodePart() == "cattailtops"|| hotbarStack.Collectible.FirstCodePart() == "papyrustops");
            
            if (sneaking && !equalStack)
            {
                return false;
            }

            if (byPlayer.Entity.Controls.Sprint && ropeStack && inventory[0].Itemstack.StackSize >= FieldsOfGoldConfig.Current.dryGrassPerHaystackBlock)
            {
                ItemStack haystack = new(Api.World.BlockAccessor.GetBlock(new AssetLocation("game:hay-normal")));
                
                
                if (byPlayer.InventoryManager.TryGiveItemstack(haystack))
                {
                    inventory[0].Itemstack.StackSize = inventory[0].Itemstack.StackSize - FieldsOfGoldConfig.Current.dryGrassPerHaystackBlock;
                    byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(1);
                    byPlayer.Entity.World.SpawnItemEntity(haystack, byPlayer.Entity.Pos.XYZ.AddCopy(0, 0.5, 0));
                }

                
                if (inventory[0].StackSize <= 0)
                {
                    Api.World.BlockAccessor.SetBlock(0, Pos);
                }
                MarkDirty();
                return true;
            }

            if (byPlayer.Entity.Controls.Sprint && fiberStack && inventory[0].StackSize >= FieldsOfGoldConfig.Current.cattailPerMat)
            {
                ItemStack strawmat = new(Api.World.BlockAccessor.GetBlock(new AssetLocation("fieldsofgold:strawmat-down")));

                if (hotbarStack.StackSize < FieldsOfGoldConfig.Current.cattailPerMat)
                {
                    (byPlayer.Entity.World.Api as ICoreClientAPI)?.TriggerIngameError(this, "notenoughfiber", Lang.Get("fieldsofgold:haystacktoofewfibers"));
                    return false;
                }

                inventory[0].Itemstack.StackSize = inventory[0].Itemstack.StackSize - FieldsOfGoldConfig.Current.dryGrassPerMat;
                if (!byPlayer.InventoryManager.TryGiveItemstack(strawmat))
                {
                   byPlayer.Entity.World.SpawnItemEntity(strawmat, byPlayer.Entity.Pos.XYZ.AddCopy(0, 0.5, 0));
                }

                byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(FieldsOfGoldConfig.Current.cattailPerMat);
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
