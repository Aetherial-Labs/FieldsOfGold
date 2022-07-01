using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FieldsOfGold.Items
{
    class FOGCattailRoot : ItemCattailRoot
    {       

        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            if (blockSel == null || ((byEntity != null) ? byEntity.World : null) == null || !byEntity.Controls.ShiftKey)
            {
                base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
                return;
            }
            bool flag = byEntity.World.BlockAccessor.GetLiquidBlock(blockSel.Position.AddCopy(blockSel.Face)).LiquidCode == "water";
            Block block;
            if (this.Code.Path.Contains("papyrus"))
            {
                block = byEntity.World.GetBlock(new AssetLocation(flag ? "fieldsofgold:tallplant-papyrus-water-growing-free" : "fieldsofgold:tallplant-papyrus-land-growing-free"));
            }
            else
            {
                block = byEntity.World.GetBlock(new AssetLocation(flag ? "fieldsofgold:tallplant-coopersreed-water-growing-free" : "fieldsofgold:tallplant-coopersreed-land-growing-free"));
            }
            if (block == null)
            {
                base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
                return;
            }
            IPlayer player = null;
            if (byEntity is EntityPlayer)
            {
                player = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
            }
            blockSel = blockSel.Clone();
            blockSel.Position.Add(blockSel.Face, 1);
            string text = "";
            if (block.TryPlaceBlock(byEntity.World, player, itemslot.Itemstack, blockSel, ref text))
            {
                byEntity.World.PlaySoundAt(block.Sounds.GetBreakSound(player), (double)blockSel.Position.X + 0.5, (double)blockSel.Position.Y + 0.5, (double)blockSel.Position.Z + 0.5, player, true, 32f, 1f);
                itemslot.TakeOut(1);
                itemslot.MarkDirty();
                handHandling = EnumHandHandling.PreventDefaultAction;
            }
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return new WorldInteraction[] {
                new WorldInteraction()
                {
                    HotKeyCode = "sneak",
                    ActionLangCode = "heldhelp-plant",
                    MouseButton = EnumMouseButton.Right,
                }
            }.Append(base.GetHeldInteractionHelp(inSlot));
        }

    }
}