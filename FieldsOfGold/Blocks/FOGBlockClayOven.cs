using FieldsOfGold.BlockEntities;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FieldsOfGold.Blocks
{
    class FOGBlockClayOven : BlockClayOven
    {
		public override void OnLoaded(ICoreAPI api)
		{
			base.OnLoaded(api);
		}

		// Token: 0x060009F8 RID: 2552 RVA: 0x00002464 File Offset: 0x00000664
		public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos)
		{
			return true;
		}

		// Token: 0x060009F9 RID: 2553 RVA: 0x0006470C File Offset: 0x0006290C
		public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection bs)
		{
			FOGBlockEntityOven blockEntityOven = world.BlockAccessor.GetBlockEntity(bs.Position) as FOGBlockEntityOven;
			if (blockEntityOven != null)
			{
				return blockEntityOven.OnInteract(byPlayer, bs);
			}
			return base.OnBlockInteractStart(world, byPlayer, bs);
		}

		// Token: 0x060009FA RID: 2554 RVA: 0x00064748 File Offset: 0x00062948
		public override EnumIgniteState OnTryIgniteBlock(EntityAgent byEntity, BlockPos pos, float secondsIgniting)
		{
			FOGBlockEntityOven blockEntityOven = byEntity.World.BlockAccessor.GetBlockEntity(pos) as FOGBlockEntityOven;
			if (blockEntityOven == null || !blockEntityOven.CanIgnite())
			{
				return EnumIgniteState.NotIgnitablePreventDefault;
			}
			if (secondsIgniting <= 4f)
			{
				return EnumIgniteState.Ignitable;
			}
			return EnumIgniteState.IgniteNow;
		}

		// Token: 0x060009FB RID: 2555 RVA: 0x00007BC2 File Offset: 0x00005DC2
		public override void OnTryIgniteBlockOver(EntityAgent byEntity, BlockPos pos, float secondsIgniting, ref EnumHandling handling)
		{
			handling = EnumHandling.PreventDefault;
			FOGBlockEntityOven blockEntityOven = byEntity.World.BlockAccessor.GetBlockEntity(pos) as FOGBlockEntityOven;
			if (blockEntityOven == null)
			{
				return;
			}
			blockEntityOven.TryIgnite();
		}

		// Token: 0x060009FC RID: 2556 RVA: 0x00007BE9 File Offset: 0x00005DE9
		public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
		{
			return this.interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
		}

		// Token: 0x060009FD RID: 2557 RVA: 0x00064784 File Offset: 0x00062984
		public override void OnAsyncClientParticleTick(IAsyncParticleManager manager, BlockPos pos, float windAffectednessAtPos, float secondsTicking)
		{
			FOGBlockEntityOven blockEntityOven = manager.BlockAccess.GetBlockEntity(pos) as FOGBlockEntityOven;
			if (blockEntityOven != null && blockEntityOven.IsBurning)
			{
				blockEntityOven.RenderParticleTick(manager, pos, windAffectednessAtPos, secondsTicking, this.particles);
			}
			base.OnAsyncClientParticleTick(manager, pos, windAffectednessAtPos, secondsTicking);
		}

		// Token: 0x060009FE RID: 2558 RVA: 0x000647CC File Offset: 0x000629CC
		private void InitializeParticles()
		{
			this.particles = new AdvancedParticleProperties[16];
			this.basePos = new Vec3f[this.particles.Length];
			Cuboidf[] array = new Cuboidf[]
			{
				new Cuboidf(0.125f, 0f, 0.125f, 0.3125f, 0.5f, 0.875f),
				new Cuboidf(0.7125f, 0f, 0.125f, 0.875f, 0.5f, 0.875f),
				new Cuboidf(0.125f, 0f, 0.125f, 0.875f, 0.5f, 0.3125f),
				new Cuboidf(0.125f, 0f, 0.7125f, 0.875f, 0.5f, 0.875f)
			};
			for (int i = 0; i < 4; i++)
			{
				AdvancedParticleProperties advancedParticleProperties = this.ParticleProperties[0].Clone();
				Cuboidf cuboidf = array[i];
				this.basePos[i] = new Vec3f(0f, 0f, 0f);
				advancedParticleProperties.PosOffset[0].avg = cuboidf.MidX;
				advancedParticleProperties.PosOffset[0].var = cuboidf.Width / 2f;
				advancedParticleProperties.PosOffset[1].avg = 0.3f;
				advancedParticleProperties.PosOffset[1].var = 0.05f;
				advancedParticleProperties.PosOffset[2].avg = cuboidf.MidZ;
				advancedParticleProperties.PosOffset[2].var = cuboidf.Length / 2f;
				advancedParticleProperties.Quantity.avg = 0.5f;
				advancedParticleProperties.Quantity.var = 0.2f;
				advancedParticleProperties.LifeLength.avg = 0.8f;
				this.particles[i] = advancedParticleProperties;
			}
			for (int j = 4; j < 8; j++)
			{
				AdvancedParticleProperties advancedParticleProperties2 = this.ParticleProperties[1].Clone();
				advancedParticleProperties2.PosOffset[1].avg = 0.06f;
				advancedParticleProperties2.PosOffset[1].var = 0.02f;
				advancedParticleProperties2.Quantity.avg = 0.5f;
				advancedParticleProperties2.Quantity.var = 0.2f;
				advancedParticleProperties2.LifeLength.avg = 0.3f;
				advancedParticleProperties2.VertexFlags = 128;
				this.particles[j] = advancedParticleProperties2;
			}
			for (int k = 8; k < 12; k++)
			{
				AdvancedParticleProperties advancedParticleProperties3 = this.ParticleProperties[2].Clone();
				advancedParticleProperties3.PosOffset[1].avg = 0.09f;
				advancedParticleProperties3.PosOffset[1].var = 0.02f;
				advancedParticleProperties3.Quantity.avg = 0.5f;
				advancedParticleProperties3.Quantity.var = 0.2f;
				advancedParticleProperties3.LifeLength.avg = 0.18f;
				advancedParticleProperties3.VertexFlags = 192;
				this.particles[k] = advancedParticleProperties3;
			}
			for (int l = 12; l < 16; l++)
			{
				AdvancedParticleProperties advancedParticleProperties4 = this.ParticleProperties[3].Clone();
				advancedParticleProperties4.PosOffset[1].avg = 0.12f;
				advancedParticleProperties4.PosOffset[1].var = 0.03f;
				advancedParticleProperties4.Quantity.avg = 0.2f;
				advancedParticleProperties4.Quantity.var = 0.1f;
				advancedParticleProperties4.LifeLength.avg = 0.12f;
				advancedParticleProperties4.VertexFlags = 255;
				this.particles[l] = advancedParticleProperties4;
			}
		}

		// Token: 0x040007DD RID: 2013
		private WorldInteraction[] interactions;

		// Token: 0x040007DE RID: 2014
		private AdvancedParticleProperties[] particles;

		// Token: 0x040007DF RID: 2015
		private Vec3f[] basePos;
	}
}
