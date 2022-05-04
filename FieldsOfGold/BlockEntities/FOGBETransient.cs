using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace FieldsOfGold.BlockEntities
{
    class FOGBETransient : BlockEntity
    {
		public virtual int CheckIntervalMs { get; set; } = 2000;

		// Token: 0x0600014B RID: 331 RVA: 0x0000F9F8 File Offset: 0x0000DBF8
		public override void Initialize(ICoreAPI api)
		{
			base.Initialize(api);
			JsonObject attributes = base.Block.Attributes;
			if (attributes == null || !attributes["transientProps"].Exists)
			{
				return;
			}
			this.props = base.Block.Attributes["transientProps"].AsObject<TransientProperties>(null);
			if (this.props == null)
			{
				return;
			}
			if (this.transitionHoursLeft <= 0.0)
			{
				this.transitionHoursLeft = (Api.World.Calendar.DaysPerMonth/30f) * (double)this.props.InGameHours;
			}
			if (api.Side == EnumAppSide.Server)
			{
				if (this.listenerId != 0L)
				{
					throw new InvalidOperationException("Initializing BETransient twice would create a memory and performance leak");
				}
				this.listenerId = this.RegisterGameTickListener(new Action<float>(this.CheckTransition), this.CheckIntervalMs, 0);
				if (this.transitionAtTotalDaysOld != null)
				{
					this.transitionHoursLeft = (this.transitionAtTotalDaysOld.Value - this.Api.World.Calendar.TotalDays) * (double)this.Api.World.Calendar.HoursPerDay;
					this.lastCheckAtTotalDays = this.Api.World.Calendar.TotalDays;
				}
			}
		}

		// Token: 0x0600014C RID: 332 RVA: 0x00002C63 File Offset: 0x00000E63
		public override void OnBlockPlaced(ItemStack byItemStack = null)
		{
			this.lastCheckAtTotalDays = this.Api.World.Calendar.TotalDays;
		}

		// Token: 0x0600014D RID: 333 RVA: 0x0000FB28 File Offset: 0x0000DD28
		public virtual void CheckTransition(float dt)
		{
			if (this.Api.World.BlockAccessor.GetBlock(this.Pos).Attributes == null)
			{
				this.Api.World.Logger.Error("BETransient exiting at {0} cannot find block attributes for {1}. Will stop transient timer", new object[]
				{
					this.Pos,
					base.Block.Code.ToShortString()
				});
				this.UnregisterGameTickListener(this.listenerId);
				return;
			}
			this.lastCheckAtTotalDays = Math.Min(this.lastCheckAtTotalDays, this.Api.World.Calendar.TotalDays);
			ClimateCondition climateAt = this.Api.World.BlockAccessor.GetClimateAt(this.Pos, EnumGetClimateMode.WorldGenValues, 0.0);
			if (climateAt == null)
			{
				return;
			}
			float temperature = climateAt.Temperature;
			float num = 1f / this.Api.World.Calendar.HoursPerDay;
			while (this.Api.World.Calendar.TotalDays - this.lastCheckAtTotalDays > (double)num)
			{
				this.lastCheckAtTotalDays += (double)num;
				this.transitionHoursLeft -= 1.0;
				climateAt.Temperature = temperature;
				ClimateCondition climateAt2 = this.Api.World.BlockAccessor.GetClimateAt(this.Pos, climateAt, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, this.lastCheckAtTotalDays);
				if (this.props.Condition == EnumTransientCondition.Temperature)
				{
					if (climateAt2.Temperature < this.props.WhenBelowTemperature || climateAt2.Temperature > this.props.WhenAboveTemperature)
					{
						this.tryTransition(this.props.ConvertTo);
					}
				}
				else
				{
					bool flag = climateAt2.Temperature < this.props.ResetBelowTemperature;
					if (climateAt2.Temperature < this.props.StopBelowTemperature || flag)
					{
						this.transitionHoursLeft += 1.0;
						if (flag)
						{
							this.transitionHoursLeft = (double)this.props.InGameHours;
						}
					}
					else if (this.transitionHoursLeft <= 0.0)
					{
						this.tryTransition(this.ConvertToOverride ?? this.props.ConvertTo);
						return;
					}
				}
			}
		}

		// Token: 0x0600014E RID: 334 RVA: 0x0000FD60 File Offset: 0x0000DF60
		public void tryTransition(string toCode)
		{
			Block block = this.Api.World.BlockAccessor.GetBlock(this.Pos);
			if (block.Attributes == null)
			{
				return;
			}
			string text = this.props.ConvertFrom;
			if (text == null || toCode == null)
			{
				return;
			}
			if (text.IndexOf(":") == -1)
			{
				text = block.Code.Domain + ":" + text;
			}
			if (toCode.IndexOf(":") == -1)
			{
				toCode = block.Code.Domain + ":" + toCode;
			}
			if (text == null || !toCode.Contains("*"))
			{
				Block block2 = this.Api.World.GetBlock(new AssetLocation(toCode));
				if (block2 == null)
				{
					return;
				}
				this.Api.World.BlockAccessor.SetBlock(block2.BlockId, this.Pos);
				return;
			}
			else
			{
				AssetLocation blockCode = block.WildCardReplace(new AssetLocation(text), new AssetLocation(toCode));
				Block block2 = this.Api.World.GetBlock(blockCode);
				if (block2 == null)
				{
					return;
				}
				this.Api.World.BlockAccessor.SetBlock(block2.BlockId, this.Pos);
				return;
			}
		}

		// Token: 0x0600014F RID: 335 RVA: 0x0000FE88 File Offset: 0x0000E088
		public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
		{
			base.FromTreeAttributes(tree, worldForResolving);
			this.transitionHoursLeft = tree.GetDouble("transitionHoursLeft", 0.0);
			if (tree.HasAttribute("transitionAtTotalDays"))
			{
				this.transitionAtTotalDaysOld = new double?(tree.GetDouble("transitionAtTotalDays", 0.0));
			}
			this.lastCheckAtTotalDays = tree.GetDouble("lastCheckAtTotalDays", 0.0);
			this.ConvertToOverride = tree.GetString("convertToOverride", null);
		}

		// Token: 0x06000150 RID: 336 RVA: 0x0000FF10 File Offset: 0x0000E110
		public override void ToTreeAttributes(ITreeAttribute tree)
		{
			base.ToTreeAttributes(tree);
			tree.SetDouble("transitionHoursLeft", this.transitionHoursLeft);
			tree.SetDouble("lastCheckAtTotalDays", this.lastCheckAtTotalDays);
			if (this.ConvertToOverride != null)
			{
				tree.SetString("convertToOverride", this.ConvertToOverride);
			}
		}

		// Token: 0x06000151 RID: 337 RVA: 0x0000FF60 File Offset: 0x0000E160
		public void SetPlaceTime(double totalHours)
		{
			float inGameHours = this.props.InGameHours;
			this.transitionHoursLeft = (double)inGameHours + totalHours - this.Api.World.Calendar.TotalHours;
		}

		// Token: 0x06000152 RID: 338 RVA: 0x00002C80 File Offset: 0x00000E80
		public bool IsDueTransition()
		{
			return this.transitionHoursLeft <= 0.0;
		}

		// Token: 0x04000187 RID: 391
		private double lastCheckAtTotalDays;

		// Token: 0x04000188 RID: 392
		private double transitionHoursLeft = -1.0;

		// Token: 0x04000189 RID: 393
		private TransientProperties props;

		// Token: 0x0400018B RID: 395
		private long listenerId;

		// Token: 0x0400018C RID: 396
		private double? transitionAtTotalDaysOld;

		// Token: 0x0400018D RID: 397
		public string ConvertToOverride;
	}
}