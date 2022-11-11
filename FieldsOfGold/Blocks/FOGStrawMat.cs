using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FieldsOfGold.Blocks
{
    class FOGStrawMat : BlockSimpleCoating
    {
		public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
		{
				bool flag = true;
				bool flag2 = false;
				foreach (BlockBehavior blockBehavior in this.BlockBehaviors)
				{
					EnumHandling enumHandling = EnumHandling.PassThrough;
					bool flag3 = blockBehavior.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref enumHandling, ref failureCode);
					if (enumHandling != EnumHandling.PassThrough)
					{
						flag = (flag && flag3);
						flag2 = true;
					}
					if (enumHandling == EnumHandling.PreventSubsequent)
					{
						return flag;
					}
				}
				if (flag2)
				{
					return flag;
				}
				if (!this.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
				{
					return false;
				}
				if (this.TryAttachTo(world, blockSel.Position, blockSel.Face))
				{
					return true;
				}
				if (this.TryAttachTo(world, blockSel.Position, BlockFacing.DOWN))
				{
					return true;
				}

				failureCode = "requireattachable";
				return false;
		}

		private bool TryAttachTo(IWorldAccessor world, BlockPos blockpos, BlockFacing onBlockFace)
		{
			BlockFacing opposite = onBlockFace.Opposite;
			BlockPos pos = blockpos.AddCopy(opposite);
			if (world.BlockAccessor.GetBlock(pos).CanAttachBlockAt(world.BlockAccessor, this, pos, onBlockFace, null))
			{
				int blockId = world.BlockAccessor.GetBlock(base.CodeWithParts(new string[]
				{
					opposite.Code
				})).BlockId;
				world.BlockAccessor.SetBlock(blockId, blockpos);
				return true;
			}
			return false;
		}
	}
}
