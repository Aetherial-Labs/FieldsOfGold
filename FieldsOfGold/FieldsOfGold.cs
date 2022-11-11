using FieldsOfGold.BlockEntities;
using FieldsOfGold.Blocks;
using FieldsOfGold.CollectibleBehaviors;
using FieldsOfGold.config;
using FieldsOfGold.Items;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace FieldsOfGold
{
    public class FieldsOfGold : ModSystem
    {

        private readonly Harmony _harmony = new("harmoniousfog");

        internal static IServerNetworkChannel serverChannel;
        internal static IClientNetworkChannel clientChannel;
        public static double daysPerMonthMod;

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }
        //Test
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
           
            //Block Entities
            //api.RegisterBlockEntityClass("fogbeberrybush", typeof(FOGBEBerryBush));
            api.RegisterBlockEntityClass("fogbeehive", typeof(FOGBEBeehive));
            api.RegisterBlockEntityClass("fogbehaystack", typeof(FOGBEHaystack));
            api.RegisterBlockEntityClass("fogbetransient", typeof(FOGBETransient));

            //Items
            api.RegisterItemClass("fogreeditem", typeof(FOGCattailRoot));
            api.RegisterItemClass("fogdrygrass", typeof(FOGDryGrass));

            //Blocks
            api.RegisterBlockClass("foghaystack", typeof(FOGHaystack));
            api.RegisterBlockClass("foghaybale", typeof(FOGHaybale));
            api.RegisterBlockClass("fogstrawmat", typeof(FOGStrawMat));
            api.RegisterBlockClass("fogreeds", typeof(FOGReeds));

            //CollectibleBehaviors
            api.RegisterCollectibleBehaviorClass("haystackable", typeof(BehaviorHayStackable));

            PatchGame();


            try
            {
                var Config = api.LoadModConfig<FieldsOfGoldConfig>("fieldsofgold.json");
                if (Config != null)
                {
                    api.Logger.Notification("Mod Config successfully loaded.");
                    FieldsOfGoldConfig.Current = Config;
                }
                else
                {
                    api.Logger.Notification("No Mod Config specified. Falling back to default settings");
                    FieldsOfGoldConfig.Current = FieldsOfGoldConfig.GetDefault();
                }
            }
            catch
            {
                FieldsOfGoldConfig.Current = FieldsOfGoldConfig.GetDefault();
                api.Logger.Error("Failed to load custom mod configuration. Falling back to default settings!");
            }
            finally
            {
      
            api.StoreModConfig(FieldsOfGoldConfig.Current, "fieldsofgold.json");
        
            }
            daysPerMonthMod = (float)api.World.Config.GetAsInt("daysPerMonth") / 9f;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            clientChannel = api.Network.RegisterChannel("fieldsofgold");
            clientChannel.RegisterMessageType(typeof(FOGConfigSyncPacket));
            clientChannel.SetMessageHandler<FOGConfigSyncPacket>((packet) =>
            {
                FieldsOfGoldConfig.Current = SerializerUtil.Deserialize<FieldsOfGoldConfig>(packet.configData);
            });
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            serverChannel = api.Network.RegisterChannel("fieldsofgold");
            serverChannel.RegisterMessageType(typeof(FOGConfigSyncPacket));
            api.Event.PlayerNowPlaying += (byPlayer) =>
            {
                {
                    serverChannel.SendPacket(new FOGConfigSyncPacket()
                    {
                        
                        configData = SerializerUtil.Serialize(FieldsOfGoldConfig.Current)

                    }, byPlayer);
                };
            };
            
        }
        public override void Dispose()
        {
            var harmony = new Harmony("harmoniousfog");
            harmony.UnpatchAll("harmoniousfog");
        }

        private void PatchGame()
        {
            Mod.Logger.Event("Applying Harmony patches");
            var harmony = new Harmony("harmoniousfog");
            var original = typeof(BlockEntityFarmland).GetMethod("GetHoursForNextStage");
            var patches = (Harmony.GetPatchInfo(original));
            if (patches != null && patches.Owners.Contains("harmoniousfog"))
            {
                return;
            }
            harmony.PatchAll();
        }

#pragma warning disable IDE0051 // Remove unused private members
        private void UnPatchGame()
#pragma warning restore IDE0051 // Remove unused private members
        {
            Mod.Logger.Event("Unapplying Harmony patches");

            _harmony.UnpatchAll();
        }

    }

    [HarmonyPatch]
    [HarmonyPatch(typeof(BlockEntityFarmland), "GetHoursForNextStage")]
    //internal sealed class BlockEntityFarmlandPatches
    class BlockEntityFarmlandPatches
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BlockEntityPumpkinVine), "Initialize")]
        static void Patch_BlockEntityPumpkinVine_Initialize_Prefix(BlockEntityPumpkinVine __instance,
            ref float ___pumpkinHoursToGrow, ref float ___vineHoursToGrow, ref float ___vineHoursToGrowStage2)
        {
            /*
             * Get appropriate modifier for crops based on DaysPerMonth using a 30 day month as a base.
              Then modify growth rate from base to account for in-game hours per day.
             */
            float dayHours = __instance.Api.World.Calendar.HoursPerDay;
            ___pumpkinHoursToGrow = (float)(FieldsOfGold.daysPerMonthMod) * ((90f / 24) * dayHours);
            ___vineHoursToGrow = (float)(FieldsOfGold.daysPerMonthMod) * ((90f / 24) * dayHours);
            ___vineHoursToGrowStage2 = (float)(FieldsOfGold.daysPerMonthMod) * ((45f / 24) * dayHours);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(BlockEntityFarmland), "GetBlockInfo")]
#pragma warning disable IDE0051 // Remove unused private members
        static void Patch_BlockEntityFarmland_GetHoursForNextStage_Prefix(BlockEntityFarmland __instance, StringBuilder dsc)
#pragma warning restore IDE0051 // Remove unused private members
        {
            double daysTillNextStage;
            daysTillNextStage = Math.Round((__instance.TotalHoursForNextStage-__instance.Api.World.Calendar.TotalHours) / __instance.Api.World.Calendar.HoursPerDay);
            var block = (__instance.CallMethod<Block>("GetCrop"));

            if (!(block is BlockCrop) || (__instance.CallMethod<int>("GetCropStage", block)) >= block.CropProps.GrowthStages)
            {
                return;
            }
            if (block is BlockCrop & daysTillNextStage > 1)
            {
                dsc.AppendLine("Crop will reach next stage in " + daysTillNextStage + " day.");
                
            }
            else if (block is BlockCrop)
            {
                dsc.AppendLine("Crop will reach next stage in less than a day.");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BlockEntityBerryBush), "GetHoursForNextStage")]
        static bool Patch_BlockEntityBerryBush_GetHoursForNextStage_Prefix(BlockEntityBerryBush __instance, ref double __result)
        {
            double hoursPerDay = __instance.Api.World.Calendar.HoursPerDay;
            double emptyDays = __instance.Block.Attributes["growthProps"]["emptyDays"].AsDouble();
            double floweringDays = __instance.Block.Attributes["growthProps"]["floweringDays"].AsDouble();
            double ripeDays = __instance.Block.Attributes["growthProps"]["ripeDays"].AsDouble();
            string state = __instance.Api.World.BlockAccessor.GetBlock(__instance.Pos).LastCodePart();


            if (state == "ripe")
            {
                __result = ((ripeDays == 0.0 ? 4.2:ripeDays) * hoursPerDay) * FieldsOfGold.daysPerMonthMod; 
                return false;
            }
            else if (state == "flowering") { 
                __result = ((floweringDays == 0.0 ? 12.6:floweringDays) * hoursPerDay) * FieldsOfGold.daysPerMonthMod;
                return false;
            } 
            else if (state == "empty") {
                __result = ((emptyDays == 0?30:emptyDays) * hoursPerDay) * FieldsOfGold.daysPerMonthMod;
                return false;
            };

            return false;
        }

    }
    public static class HarmonyReflectionExtensions
    {
        /// <summary>
        ///     Calls a method within an instance of an object, via reflection. This can be an internal or private method within another assembly.
        /// </summary>
        /// <typeparam name="T">The return type, expected back from the method.</typeparam>
        /// <param name="instance">The instance to call the method from.</param>
        /// <param name="method">The name of the method to call.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <returns>The return value of the reflected method call.</returns>
        public static T CallMethod<T>(this object instance, string method, params object[] args)
        {
            return (T)AccessTools.Method(instance.GetType(), method).Invoke(instance, args);
        }

        public static bool IsDefaultValue<T>(this T @this)
        {
            return EqualityComparer<T>.Default.Equals(@this, default);
        }
    }

}
        

