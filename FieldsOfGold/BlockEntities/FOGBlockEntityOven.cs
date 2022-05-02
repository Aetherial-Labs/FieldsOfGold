using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FieldsOfGold.BlockEntities
{
    class FOGBlockEntityOven : BlockEntityDisplay, IHeatSource
    {
		// Token: 0x17000097 RID: 151
		// (get) Token: 0x06000651 RID: 1617 RVA: 0x00005C42 File Offset: 0x00003E42
		public virtual float maxTemperature
		{
			get
			{
				return 300f;
			}
		}

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x06000652 RID: 1618 RVA: 0x00005C49 File Offset: 0x00003E49
		public virtual int itemCapacity
		{
			get
			{
				return 4;
			}
		}

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x06000653 RID: 1619 RVA: 0x00005C4C File Offset: 0x00003E4C
		public virtual int fuelitemCapacity
		{
			get
			{
				return 6;
			}
		}

		// Token: 0x06000654 RID: 1620 RVA: 0x00049E4C File Offset: 0x0004804C
		public FOGBlockEntityOven()
		{
			this.bakingData = new OvenItemData[this.itemCapacity];
			for (int i = 0; i < this.itemCapacity; i++)
			{
				this.bakingData[i] = new OvenItemData();
			}
			this.woodrand = new int[this.fuelitemCapacity];
			this.ovenInv = new InventoryOven("oven-0", this.itemCapacity, this.fuelitemCapacity);
			this.meshes = new MeshData[this.itemCapacity + this.fuelitemCapacity];
		}

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x06000655 RID: 1621 RVA: 0x00005C4F File Offset: 0x00003E4F
		public override InventoryBase Inventory
		{
			get
			{
				return this.ovenInv;
			}
		}

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x06000656 RID: 1622 RVA: 0x00005C57 File Offset: 0x00003E57
		public override string InventoryClassName
		{
			get
			{
				return "oven";
			}
		}

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x06000657 RID: 1623 RVA: 0x00005C5E File Offset: 0x00003E5E
		public ItemSlot FuelSlot
		{
			get
			{
				return this.ovenInv[this.itemCapacity];
			}
		}

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x06000658 RID: 1624 RVA: 0x00005C71 File Offset: 0x00003E71
		public bool IsBurning
		{
			get
			{
				return this.burning;
			}
		}

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x06000659 RID: 1625 RVA: 0x00049EEC File Offset: 0x000480EC
		public bool HasItems
		{
			get
			{
				for (int i = 0; i < this.itemCapacity; i++)
				{
					if (!this.ovenInv[i].Empty)
					{
						return true;
					}
				}
				return false;
			}
		}

		// Token: 0x0600065A RID: 1626 RVA: 0x00049F20 File Offset: 0x00048120
		public override void Initialize(ICoreAPI api)
		{
			this.capi = (api as ICoreClientAPI);
			base.Initialize(api);
			InventoryBase inventoryBase = this.ovenInv;
			string inventoryClassName = this.InventoryClassName;
			string str = "-";
			BlockPos pos = this.Pos;
			inventoryBase.LateInitialize(inventoryClassName + str + ((pos != null) ? pos.ToString() : null), api);
			this.RegisterGameTickListener(new Action<float>(this.OnBurnTick), 100, 0);
			this.prng = new Random(this.Pos.GetHashCode());
			this.SetRotation();
		}

		// Token: 0x0600065B RID: 1627 RVA: 0x00049FA4 File Offset: 0x000481A4
		private void SetRotation()
		{
			string a = base.Block.Variant["side"];
			if (a == "south")
			{
				this.rotation = 270;
				return;
			}
			if (a == "west")
			{
				this.rotation = 180;
				return;
			}
			if (!(a == "east"))
			{
				this.rotation = 90;
				return;
			}
			this.rotation = 0;
		}

		// Token: 0x0600065C RID: 1628 RVA: 0x00005C79 File Offset: 0x00003E79
		public float GetHeatStrength(IWorldAccessor world, BlockPos heatSourcePos, BlockPos heatReceiverPos)
		{
			return Math.Max((this.ovenTemperature - 20f) / (this.maxTemperature - 20f) * 8f, 0f);
		}

		// Token: 0x0600065D RID: 1629 RVA: 0x0004A018 File Offset: 0x00048218
		public virtual bool OnInteract(IPlayer byPlayer, BlockSelection bs)
		{
			ItemSlot activeHotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
			if (activeHotbarSlot.Empty)
			{
				if (this.TryTake(byPlayer))
				{
					byPlayer.InventoryManager.BroadcastHotbarSlot();
					return true;
				}
				return false;
			}
			else
			{
				CollectibleObject collectible = activeHotbarSlot.Itemstack.Collectible;
				JsonObject attributes = collectible.Attributes;
				if (attributes == null || !attributes.IsTrue("isFirewood"))
				{
					JsonObject attributes2 = collectible.Attributes;
					if (((attributes2 != null) ? attributes2["bakingProperties"] : null) == null)
					{
						CombustibleProperties combustibleProps = collectible.CombustibleProps;
						if (combustibleProps == null || combustibleProps.SmeltingType != EnumSmeltType.Bake || collectible.CombustibleProps.MeltingPoint >= 260)
						{
							if (this.TryTake(byPlayer))
							{
								byPlayer.InventoryManager.BroadcastHotbarSlot();
								return true;
							}
							return false;
						}
					}
					if (activeHotbarSlot.Itemstack.Equals(this.Api.World, this.lastRemoved, GlobalConstants.IgnoredStackAttributes) && !this.ovenInv[0].Empty)
					{
						if (this.TryTake(byPlayer))
						{
							byPlayer.InventoryManager.BroadcastHotbarSlot();
							return true;
						}
					}
					else
					{
						if (this.TryPut(activeHotbarSlot))
						{
							ItemStack itemstack = activeHotbarSlot.Itemstack;
							AssetLocation assetLocation;
							if (itemstack == null)
							{
								assetLocation = null;
							}
							else
							{
								Block block = itemstack.Block;
								if (block == null)
								{
									assetLocation = null;
								}
								else
								{
									BlockSounds sounds = block.Sounds;
									assetLocation = ((sounds != null) ? sounds.Place : null);
								}
							}
							AssetLocation assetLocation2 = assetLocation;
							this.Api.World.PlaySoundAt((assetLocation2 != null) ? assetLocation2 : new AssetLocation("sounds/player/buildhigh"), byPlayer.Entity, byPlayer, true, 16f, 1f);
							byPlayer.InventoryManager.BroadcastHotbarSlot();
							return true;
						}
						Block block2 = activeHotbarSlot.Itemstack.Block;
						if (((block2 != null) ? block2.GetBehavior<BlockBehaviorCanIgnite>() : null) == null)
						{
							ICoreClientAPI coreClientAPI = this.Api as ICoreClientAPI;
							if (coreClientAPI != null && (activeHotbarSlot.Empty || !activeHotbarSlot.Itemstack.Attributes.GetBool("bakeable", true)))
							{
								coreClientAPI.TriggerIngameError(this, "notbakeable", Lang.Get("This item is not bakeable.", Array.Empty<object>()));
							}
							else if (coreClientAPI != null && !activeHotbarSlot.Empty)
							{
								coreClientAPI.TriggerIngameError(this, "notbakeable", this.burning ? Lang.Get("Wait until the fire is out", Array.Empty<object>()) : Lang.Get("Oven is full", Array.Empty<object>()));
							}
							return true;
						}
					}
					return false;
				}
				if (this.TryFuel(activeHotbarSlot))
				{
					ItemStack itemstack2 = activeHotbarSlot.Itemstack;
					AssetLocation assetLocation3;
					if (itemstack2 == null)
					{
						assetLocation3 = null;
					}
					else
					{
						Block block3 = itemstack2.Block;
						if (block3 == null)
						{
							assetLocation3 = null;
						}
						else
						{
							BlockSounds sounds2 = block3.Sounds;
							assetLocation3 = ((sounds2 != null) ? sounds2.Place : null);
						}
					}
					AssetLocation assetLocation4 = assetLocation3;
					this.Api.World.PlaySoundAt((assetLocation4 != null) ? assetLocation4 : new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16f, 1f);
					byPlayer.InventoryManager.BroadcastHotbarSlot();
					IClientPlayer clientPlayer = byPlayer as IClientPlayer;
					if (clientPlayer != null)
					{
						clientPlayer.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
					}
					return true;
				}
				return false;
			}
		}

		// Token: 0x0600065E RID: 1630 RVA: 0x0004A2D4 File Offset: 0x000484D4
		protected virtual bool TryFuel(ItemSlot slot)
		{
			if (this.IsBurning || this.HasItems)
			{
				return false;
			}
			if (this.FuelSlot.Empty || this.FuelSlot.Itemstack.StackSize < this.fuelitemCapacity)
			{
				int num = slot.TryPutInto(this.Api.World, this.FuelSlot, 1);
				if (num > 0)
				{
					base.MarkDirty(true, null);
					this.lastRemoved = null;
				}
				return num > 0;
			}
			return false;
		}

		// Token: 0x0600065F RID: 1631 RVA: 0x0004A348 File Offset: 0x00048548
		protected virtual bool TryPut(ItemSlot slot)
		{
			if (this.IsBurning || !this.FuelSlot.Empty)
			{
				return false;
			}
			BakingProperties bakingProperties = BakingProperties.ReadFrom(slot.Itemstack);
			if (bakingProperties == null)
			{
				return false;
			}
			if (!slot.Itemstack.Attributes.GetBool("bakeable", true))
			{
				return false;
			}
			if (bakingProperties.LargeItem)
			{
				for (int i = 0; i < this.itemCapacity; i++)
				{
					if (!this.ovenInv[i].Empty)
					{
						return false;
					}
				}
			}
			for (int j = 0; j < this.itemCapacity; j++)
			{
				if (this.ovenInv[j].Empty)
				{
					int num = slot.TryPutInto(this.Api.World, this.ovenInv[j], 1);
					if (num > 0)
					{
						this.bakingData[j] = new OvenItemData(this.ovenInv[j].Itemstack);
						this.updateMesh(j);
						base.MarkDirty(true, null);
						this.lastRemoved = null;
					}
					return num > 0;
				}
				if (j == 0)
				{
					BakingProperties bakingProperties2 = BakingProperties.ReadFrom(this.ovenInv[j].Itemstack);
					if (bakingProperties2 != null && bakingProperties2.LargeItem)
					{
						return false;
					}
				}
			}
			return false;
		}

		// Token: 0x06000660 RID: 1632 RVA: 0x0004A474 File Offset: 0x00048674
		protected virtual bool TryTake(IPlayer byPlayer)
		{
			int i = this.itemCapacity;
			while (i >= 0)
			{
				if (i != this.itemCapacity || this.FuelSlot.Empty)
				{
					goto IL_4D;
				}
				JsonObject attributes = this.FuelSlot.Itemstack.Collectible.Attributes;
				if (attributes == null || !attributes.IsTrue("isFirewood"))
				{
					goto IL_4D;
				}
			IL_152:
				i--;
				continue;
			IL_4D:
				if (!this.ovenInv[i].Empty)
				{
					ItemStack itemStack = this.ovenInv[i].TakeOut(1);
					this.lastRemoved = ((itemStack == null) ? null : itemStack.Clone());
					if (byPlayer.InventoryManager.TryGiveItemstack(itemStack, false))
					{
						Block block = itemStack.Block;
						AssetLocation assetLocation;
						if (block == null)
						{
							assetLocation = null;
						}
						else
						{
							BlockSounds sounds = block.Sounds;
							assetLocation = ((sounds != null) ? sounds.Place : null);
						}
						AssetLocation assetLocation2 = assetLocation;
						this.Api.World.PlaySoundAt((assetLocation2 != null) ? assetLocation2 : new AssetLocation("sounds/player/throw"), byPlayer.Entity, byPlayer, true, 16f, 1f);
					}
					if (itemStack.StackSize > 0)
					{
						this.Api.World.SpawnItemEntity(itemStack, this.Pos.ToVec3d().Add(0.5, 0.5, 0.5), null);
					}
					this.bakingData[i].CurHeightMul = 1f;
					this.updateMesh(i);
					base.MarkDirty(true, null);
					return true;
				}
				goto IL_152;
			}
			return false;
		}

		// Token: 0x06000661 RID: 1633 RVA: 0x0004A5E0 File Offset: 0x000487E0
		public virtual ItemStack[] CanAdd(ItemStack[] itemstacks)
		{
			if (this.IsBurning)
			{
				return null;
			}
			if (!this.FuelSlot.Empty)
			{
				return null;
			}
			if (this.ovenTemperature <= (float)(this.EnvironmentTemperature() + 25))
			{
				return null;
			}
			for (int i = 0; i < this.itemCapacity; i++)
			{
				if (this.ovenInv[i].Empty)
				{
					return itemstacks;
				}
			}
			return null;
		}

		// Token: 0x06000662 RID: 1634 RVA: 0x0004A644 File Offset: 0x00048844
		public virtual ItemStack[] CanAddAsFuel(ItemStack[] itemstacks)
		{
			if (this.IsBurning)
			{
				return null;
			}
			for (int i = 0; i < this.itemCapacity; i++)
			{
				if (!this.ovenInv[i].Empty)
				{
					return null;
				}
			}
			if (this.FuelSlot.StackSize >= this.fuelitemCapacity)
			{
				return null;
			}
			return itemstacks;
		}

		// Token: 0x06000663 RID: 1635 RVA: 0x0004A698 File Offset: 0x00048898
		public bool TryIgnite()
		{
			if (!this.CanIgnite())
			{
				return false;
			}
			this.burning = true;
			this.fuelBurnTime = (float)(45 + this.FuelSlot.StackSize * 5);
			base.MarkDirty(false, null);
			ILoadedSound loadedSound = this.ambientSound;
			if (loadedSound != null)
			{
				loadedSound.Start();
			}
			return true;
		}

		// Token: 0x06000664 RID: 1636 RVA: 0x00005CA4 File Offset: 0x00003EA4
		public bool CanIgnite()
		{
			return !this.FuelSlot.Empty && !this.burning;
		}

		// Token: 0x06000665 RID: 1637 RVA: 0x0004A6E8 File Offset: 0x000488E8
		protected virtual void OnBurnTick(float dt)
		{
			dt *= 1.25f;
			if (this.Api is ICoreClientAPI)
			{
				return;
			}
			if (this.fuelBurnTime > 0f)
			{
				this.fuelBurnTime -= dt;
				if (this.fuelBurnTime <= 0f)
				{
					this.fuelBurnTime = 0f;
					this.burning = false;
					CombustibleProperties combustibleProps = this.FuelSlot.Itemstack.Collectible.CombustibleProps;
					if (((combustibleProps != null) ? combustibleProps.SmeltedStack : null) == null)
					{
						this.FuelSlot.Itemstack = null;
						for (int i = 0; i < this.itemCapacity; i++)
						{
							this.bakingData[i].CurHeightMul = 1f;
						}
					}
					else
					{
						int stackSize = this.FuelSlot.StackSize;
						this.FuelSlot.Itemstack = combustibleProps.SmeltedStack.ResolvedItemstack.Clone();
						this.FuelSlot.Itemstack.StackSize = stackSize * combustibleProps.SmeltedRatio;
					}
					base.MarkDirty(true, null);
				}
			}
			if (this.IsBurning)
			{
				this.ovenTemperature = this.ChangeTemperature(this.ovenTemperature, this.maxTemperature, dt * (float)this.FuelSlot.StackSize / (float)this.fuelitemCapacity);
			}
			else
			{
				int num = this.EnvironmentTemperature();
				if (this.ovenTemperature > (float)num)
				{
					this.HeatInput(dt);
					//TODO Change made here. Original value was 24f
					this.ovenTemperature = this.ChangeTemperature(this.ovenTemperature, (float)num, dt / 240f);

				}
			}
			int num2 = this.syncCount + 1;
			this.syncCount = num2;
			if (num2 % 5 == 0 && (this.IsBurning || this.prevOvenTemperature != this.ovenTemperature || !this.Inventory.Empty))
			{
				base.MarkDirty(false, null);
				this.prevOvenTemperature = this.ovenTemperature;
			}
		}

		// Token: 0x06000666 RID: 1638 RVA: 0x0004A8A4 File Offset: 0x00048AA4
		protected virtual void HeatInput(float dt)
		{
			for (int i = 0; i < this.itemCapacity; i++)
			{
				ItemStack itemstack = this.ovenInv[i].Itemstack;
				if (itemstack != null && this.HeatStack(itemstack, dt, i) >= 100f)
				{
					this.IncrementallyBake(dt * 1.2f, i);
				}
			}
		}

		// Token: 0x06000667 RID: 1639 RVA: 0x0004A8F8 File Offset: 0x00048AF8
		protected virtual float HeatStack(ItemStack stack, float dt, int i)
		{
			float temp = this.bakingData[i].temp;
			float num = temp;
			if (temp < this.ovenTemperature)
			{
				float dt2 = (1f + GameMath.Clamp((this.ovenTemperature - temp) / 28f, 0f, 1.6f)) * dt;
				num = this.ChangeTemperature(temp, this.ovenTemperature, dt2);
				CombustibleProperties combustibleProps = stack.Collectible.CombustibleProps;
				int val = (combustibleProps != null) ? combustibleProps.MaxTemperature : 0;
				JsonObject itemAttributes = stack.ItemAttributes;
				int num2 = Math.Max(val, (itemAttributes != null) ? itemAttributes["maxTemperature"].AsInt(0) : 0);
				if (num2 > 0)
				{
					num = Math.Min((float)num2, num);
				}
			}
			else if (temp > this.ovenTemperature)
			{
				float dt3 = (1f + GameMath.Clamp((temp - this.ovenTemperature) / 28f, 0f, 1.6f)) * dt;
				num = this.ChangeTemperature(temp, this.ovenTemperature, dt3);
			}
			if (temp != num)
			{
				this.bakingData[i].temp = num;
			}
			return num;
		}

		// Token: 0x06000668 RID: 1640 RVA: 0x0004A9F0 File Offset: 0x00048BF0
		protected virtual void IncrementallyBake(float dt, int slotIndex)
		{
			ItemSlot itemSlot = this.Inventory[slotIndex];
			OvenItemData ovenItemData = this.bakingData[slotIndex];
			float num = ovenItemData.BrowningPoint;
			if (num == 0f)
			{
				num = 160f;
			}
			int num2 = (int)(ovenItemData.temp / num);
			float num3 = ovenItemData.TimeToBake;
			if (num3 == 0f)
			{
				num3 = 1f;
			}
			float num4 = (float)GameMath.Clamp(num2, 1, 30) * dt / num3;
			float num5 = ovenItemData.BakedLevel;
			if (ovenItemData.temp > num)
			{
				num5 = ovenItemData.BakedLevel + num4;
				ovenItemData.BakedLevel = num5;
			}
			BakingProperties bakingProperties = BakingProperties.ReadFrom(itemSlot.Itemstack);
			float num6 = (bakingProperties != null) ? bakingProperties.LevelFrom : 0f;
			float num7 = (bakingProperties != null) ? bakingProperties.LevelTo : 1f;
			float v = (bakingProperties != null) ? bakingProperties.StartScaleY : 1f;
			float v2 = (bakingProperties != null) ? bakingProperties.EndScaleY : 1f;
			float t = GameMath.Clamp((num5 - num6) / (num7 - num6), 0f, 1f);
			float num8 = (float)((int)(GameMath.Mix(v, v2, t) * (float)FOGBlockEntityOven.BakingStageThreshold)) / (float)FOGBlockEntityOven.BakingStageThreshold;
			bool flag = num8 != ovenItemData.CurHeightMul;
			ovenItemData.CurHeightMul = num8;
			if (num5 > num7)
			{
				float temp = ovenItemData.temp;
				string text = (bakingProperties != null) ? bakingProperties.ResultCode : null;
				if (text != null)
				{
					ItemStack itemStack = null;
					if (itemSlot.Itemstack.Class == EnumItemClass.Block)
					{
						Block block = this.Api.World.GetBlock(new AssetLocation(text));
						if (block != null)
						{
							itemStack = new ItemStack(block, 1);
						}
					}
					else
					{
						Item item = this.Api.World.GetItem(new AssetLocation(text));
						if (item != null)
						{
							itemStack = new ItemStack(item, 1);
						}
					}
					if (itemStack != null)
					{
						IBakeableCallback bakeableCallback = this.ovenInv[slotIndex].Itemstack.Collectible as IBakeableCallback;
						if (bakeableCallback != null)
						{
							bakeableCallback.OnBaked(this.ovenInv[slotIndex].Itemstack, itemStack);
						}
						this.ovenInv[slotIndex].Itemstack = itemStack;
						this.bakingData[slotIndex] = new OvenItemData(itemStack);
						this.bakingData[slotIndex].temp = temp;
						flag = true;
					}
				}
				else
				{
					ItemSlot itemSlot2 = new DummySlot(null);
					if (itemSlot.Itemstack.Collectible.CanSmelt(this.Api.World, this.ovenInv, itemSlot.Itemstack, null))
					{
						itemSlot.Itemstack.Collectible.DoSmelt(this.Api.World, this.ovenInv, this.ovenInv[slotIndex], itemSlot2);
						if (!itemSlot2.Empty)
						{
							this.ovenInv[slotIndex].Itemstack = itemSlot2.Itemstack;
							this.bakingData[slotIndex] = new OvenItemData(itemSlot2.Itemstack);
							this.bakingData[slotIndex].temp = temp;
							flag = true;
						}
					}
				}
			}
			if (flag)
			{
				this.updateMesh(slotIndex);
				base.MarkDirty(true, null);
			}
		}

		// Token: 0x06000669 RID: 1641 RVA: 0x0004ACE4 File Offset: 0x00048EE4
		protected virtual int EnvironmentTemperature()
		{
			ClimateCondition climateAt = this.Api.World.BlockAccessor.GetClimateAt(this.Pos, EnumGetClimateMode.NowValues, 0.0);
			if (climateAt != null)
			{
				return (int)climateAt.Temperature;
			}
			return 20;
		}

		// Token: 0x0600066A RID: 1642 RVA: 0x0004AD24 File Offset: 0x00048F24
		public virtual float ChangeTemperature(float fromTemp, float toTemp, float dt)
		{
			float num = Math.Abs(fromTemp - toTemp);
			num *= GameMath.Sqrt(num);
			dt += dt * (num / 480f);
			if (num < dt)
			{
				return toTemp;
			}
			if (fromTemp > toTemp)
			{
				dt = -dt / 2f;
			}
			if (Math.Abs(fromTemp - toTemp) < 1f)
			{
				return toTemp;
			}
			return fromTemp + dt;
		}

		// Token: 0x0600066B RID: 1643 RVA: 0x0004AD78 File Offset: 0x00048F78
		public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
		{
			base.FromTreeAttributes(tree, worldForResolving);
			this.ovenInv.FromTreeAttributes(tree);
			this.burning = (tree.GetInt("burn", 0) > 0);
			this.rotation = tree.GetInt("rota", 0);
			this.ovenTemperature = tree.GetFloat("temp", 0f);
			this.fuelBurnTime = tree.GetFloat("tfuel", 0f);
			for (int i = 0; i < this.itemCapacity; i++)
			{
				this.bakingData[i] = OvenItemData.ReadFromTree(tree, i);
			}
			ICoreAPI api = this.Api;
			if (api != null && api.Side == EnumAppSide.Client)
			{
				this.updateMeshes();
				if (this.clientSidePrevBurning != this.IsBurning)
				{
					this.ToggleAmbientSounds(this.IsBurning);
					this.clientSidePrevBurning = this.IsBurning;
					base.MarkDirty(true, null);
				}
			}
		}

		// Token: 0x0600066C RID: 1644 RVA: 0x0004AE58 File Offset: 0x00049058
		public override void ToTreeAttributes(ITreeAttribute tree)
		{
			base.ToTreeAttributes(tree);
			this.ovenInv.ToTreeAttributes(tree);
			tree.SetInt("burn", this.burning ? 1 : 0);
			tree.SetInt("rota", this.rotation);
			tree.SetFloat("temp", this.ovenTemperature);
			tree.SetFloat("tfuel", this.fuelBurnTime);
			for (int i = 0; i < this.itemCapacity; i++)
			{
				this.bakingData[i].WriteToTree(tree, i);
			}
		}

		// Token: 0x0600066D RID: 1645 RVA: 0x0004AEE4 File Offset: 0x000490E4
		public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
		{
			if (this.ovenTemperature <= 25f)
			{
				sb.AppendLine(Lang.Get("Temperature: {0}", new object[]
				{
					Lang.Get("Cold", Array.Empty<object>())
				}));
			}
			else
			{
				sb.AppendLine(Lang.Get("Temperature: {0}°C", new object[]
				{
					(int)this.ovenTemperature
				}));
				if (this.ovenTemperature < 100f && !this.IsBurning)
				{
					sb.AppendLine(Lang.Get("Reheat to continue baking", Array.Empty<object>()));
				}
			}
			sb.AppendLine();
			for (int i = 0; i < this.itemCapacity; i++)
			{
				if (!this.ovenInv[i].Empty)
				{
					ItemStack itemstack = this.ovenInv[i].Itemstack;
					sb.Append(itemstack.GetName());
					sb.AppendLine(string.Format(" ({0}°C)", (int)this.bakingData[i].temp));
				}
			}
		}

		// Token: 0x0600066E RID: 1646 RVA: 0x0004AFE8 File Offset: 0x000491E8
		public virtual void ToggleAmbientSounds(bool on)
		{
			if (this.Api.Side != EnumAppSide.Client)
			{
				return;
			}
			if (on)
			{
				if (this.ambientSound == null || !this.ambientSound.IsPlaying)
				{
					this.ambientSound = ((IClientWorldAccessor)this.Api.World).LoadSound(new SoundParams
					{
						Location = new AssetLocation("sounds/environment/fireplace.ogg"),
						ShouldLoop = true,
						Position = this.Pos.ToVec3f().Add(0.5f, 0.1f, 0.5f),
						DisposeOnFinish = false,
						Volume = 0.66f
					});
					this.ambientSound.Start();
					return;
				}
			}
			else
			{
				this.ambientSound.Stop();
				this.ambientSound.Dispose();
				this.ambientSound = null;
			}
		}

		// Token: 0x0600066F RID: 1647 RVA: 0x00005CBE File Offset: 0x00003EBE
		public override void OnBlockRemoved()
		{
			base.OnBlockRemoved();
			if (this.ambientSound != null)
			{
				this.ambientSound.Stop();
				this.ambientSound.Dispose();
			}
		}

		// Token: 0x06000670 RID: 1648 RVA: 0x0004B0BC File Offset: 0x000492BC
		protected override void updateMesh(int index)
		{
			if (this.Api == null || this.Api.Side == EnumAppSide.Server)
			{
				return;
			}
			bool isWood = false;
			float scaleY = 0f;
			ItemStack itemStack;
			if (index < this.itemCapacity)
			{
				if (this.Inventory[index].Empty)
				{
					this.meshes[index] = null;
					return;
				}
				itemStack = this.Inventory[index].Itemstack;
				scaleY = this.bakingData[index].CurHeightMul;
			}
			else
			{
				if ((this.FuelSlot.Empty ? 0 : this.FuelSlot.Itemstack.StackSize) <= index - this.itemCapacity)
				{
					this.meshes[index] = null;
					return;
				}
				itemStack = this.FuelSlot.Itemstack.Clone();
				itemStack.StackSize = 1;
				JsonObject attributes = itemStack.Collectible.Attributes;
				isWood = (attributes != null && attributes.IsTrue("isFirewood"));
			}
			bool isLargeItem = false;
			if (index == 0)
			{
				BakingProperties bakingProperties = BakingProperties.ReadFrom(itemStack);
				if (bakingProperties == null)
				{
					return;
				}
				isLargeItem = bakingProperties.LargeItem;
			}
			MeshData meshData = this.genMesh(itemStack);
			if (meshData != null)
			{
				this.translateMesh(meshData, index, isWood, isLargeItem, scaleY);
				this.meshes[index] = meshData;
			}
		}

		// Token: 0x06000671 RID: 1649 RVA: 0x0004B1D8 File Offset: 0x000493D8
		protected void translateMesh(MeshData mesh, int index, bool isWood, bool isLargeItem, float scaleY)
		{
			float num;
			float num4;
			float y;
			float num5;
			if (isWood)
			{
				if (!this.woodrandDone)
				{
					this.WoodRandomiserSetup();
				}
				num = 0.46f;
				scaleY = 1f;
				float num2 = (float)(this.woodrand[index - this.itemCapacity] - 4) * 0.6f;
				float num3 = (float)(this.woodrand[this.fuelitemCapacity - 1 - index + this.itemCapacity] - 4) / 256f;
				if (index < this.itemCapacity + 3)
				{
					num4 = 0.40625f + num3;
					y = -0.093125f;
					num5 = (float)((index - 5) * 3 + 8) / 16f;
					num2 += 90f;
					mesh.Rotate(FOGBlockEntityOven.centre, 0f, num2 * 0.017453292f, 0f);
				}
				else
				{
					num4 = (float)((8 - index) * 3 + 7) / 16f;
					y = 0.019375f;
					num5 = 0.5f + num3;
					mesh.Rotate(FOGBlockEntityOven.centre, 0f, num2 * 0.017453292f, 0f);
				}
			}
			else
			{
				this.woodrandDone = false;
				num4 = ((index % 2 == 0) ? 0.65625f : 0.34375f);
				y = 0.063125f;
				num5 = ((index > 1) ? 0.65625f : 0.34375f);
				if (isLargeItem)
				{
					num4 = 0.5f;
					num5 = 0.5f;
				}
				num = 0.78f;
			}
			mesh.Scale(FOGBlockEntityOven.centre, num, num * scaleY, num);
			mesh.Translate(num4 - 0.5f, y, num5 - 0.5f);
			if (this.rotation > 0)
			{
				mesh.Rotate(FOGBlockEntityOven.centre, 0f, (float)this.rotation * 0.017453292f, 0f);
			}
		}

		// Token: 0x06000672 RID: 1650 RVA: 0x0004B374 File Offset: 0x00049574
		protected virtual void WoodRandomiserSetup()
		{
			Random random = new Random(this.Pos.GetHashCode());
			for (int i = 0; i < this.fuelitemCapacity; i++)
			{
				this.woodrand[i] = random.Next(0, 9);
			}
			this.woodrandDone = true;
		}

		// Token: 0x06000673 RID: 1651 RVA: 0x0004B3BC File Offset: 0x000495BC
		public virtual void RenderParticleTick(IAsyncParticleManager manager, BlockPos pos, float windAffectednessAtPos, float secondsTicking, AdvancedParticleProperties[] particles)
		{
			if (this.fuelBurnTime < 3f)
			{
				return;
			}
			int stackSize = this.FuelSlot.StackSize;
			bool flag = stackSize > 3;
			double[] array = new double[4];
			float[] array2 = new float[4];
			for (int i = 0; i < particles.Length; i++)
			{
				if ((i < 12 || (float)this.prng.Next(0, 90) <= this.fuelBurnTime) && (i < 8 || i >= 12 || (float)this.prng.Next(0, 12) <= this.fuelBurnTime) && (i < 4 || i >= 4 || this.prng.Next(0, 6) != 0))
				{
					if (i >= 4 && stackSize < 3)
					{
						bool flag2 = this.rotation >= 180;
						if ((!flag2 && array2[i % 2] > (float)stackSize * 0.2f + 0.14f) || (flag2 && array2[i % 2] < (float)(3 - stackSize) * 0.2f + 0.14f))
						{
							goto IL_2EF;
						}
					}
					AdvancedParticleProperties advancedParticleProperties = particles[i];
					advancedParticleProperties.WindAffectednesAtPos = 0f;
					advancedParticleProperties.basePos.X = (double)pos.X;
					advancedParticleProperties.basePos.Y = (double)((float)pos.Y + (flag ? 0.09375f : 0.03125f));
					advancedParticleProperties.basePos.Z = (double)pos.Z;
					if (i >= 4)
					{
						bool flag3 = this.rotation % 180 > 0;
						if (flag)
						{
							flag3 = !flag3;
						}
						advancedParticleProperties.basePos.Z += (flag3 ? array[i % 2] : ((double)array2[i % 2]));
						advancedParticleProperties.basePos.X += (flag3 ? ((double)array2[i % 2]) : array[i % 2]);
						advancedParticleProperties.basePos.Y += (double)((float)(flag ? 4 : 3) / 32f);
						int num = this.rotation;
						if (num != 0)
						{
							if (num != 90)
							{
								if (num != 180)
								{
									advancedParticleProperties.basePos.Z -= (double)(flag ? 0.08f : 0.12f);
								}
								else
								{
									advancedParticleProperties.basePos.X += (double)(flag ? 0.08f : 0.12f);
								}
							}
							else
							{
								advancedParticleProperties.basePos.Z += (double)(flag ? 0.08f : 0.12f);
							}
						}
						else
						{
							advancedParticleProperties.basePos.X -= (double)(flag ? 0.08f : 0.12f);
						}
					}
					else
					{
						array[i] = this.prng.NextDouble() * 0.4000000059604645 + 0.33000001311302185;
						array2[i] = 0.26f + (float)this.prng.Next(0, 3) * 0.2f + (float)this.prng.NextDouble() * 0.08f;
					}
					manager.Spawn(advancedParticleProperties);
				}
			IL_2EF:;
			}
		}

		// Token: 0x04000618 RID: 1560
		private static readonly Vec3f centre = new Vec3f(0.5f, 0f, 0.5f);

		// Token: 0x04000619 RID: 1561
		public static int BakingStageThreshold = 100;

		// Token: 0x0400061A RID: 1562
		public const int maxBakingTemperatureAccepted = 260;

		// Token: 0x0400061B RID: 1563
		private bool burning;

		// Token: 0x0400061C RID: 1564
		private bool clientSidePrevBurning;

		// Token: 0x0400061D RID: 1565
		public float prevOvenTemperature = 20f;

		// Token: 0x0400061E RID: 1566
		public float ovenTemperature = 20f;

		// Token: 0x0400061F RID: 1567
		private float fuelBurnTime;

		// Token: 0x04000620 RID: 1568
		private readonly OvenItemData[] bakingData;

		// Token: 0x04000621 RID: 1569
		private int[] woodrand;

		// Token: 0x04000622 RID: 1570
		private bool woodrandDone;

		// Token: 0x04000623 RID: 1571
		private ItemStack lastRemoved;

		// Token: 0x04000624 RID: 1572
		private int rotation;

		// Token: 0x04000625 RID: 1573
		private Random prng;

		// Token: 0x04000626 RID: 1574
		private int syncCount;

		// Token: 0x04000627 RID: 1575
		private ILoadedSound ambientSound;

		// Token: 0x04000628 RID: 1576
		internal InventoryOven ovenInv;
	}
}
