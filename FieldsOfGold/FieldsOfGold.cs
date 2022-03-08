using FieldsOfGold.BlockEntities;
using FieldsOfGold.Blocks;
using FieldsOfGold.config;
using FieldsOfGold.Items;
using HarmonyLib;
using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace FieldsOfGold
{
    public class FieldsOfGold : ModSystem
    {

        private Harmony _harmony = new Harmony("harmoniousFOG");
        
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("fogreeds", typeof(FOGReeds));
            api.RegisterBlockEntityClass("fogbeberrybush", typeof(FOGBEBerryBush));
            api.RegisterBlockEntityClass("fogbeehive", typeof(FOGBEBeehive));
            api.RegisterBlockEntityClass("fogbehaystack", typeof(FOGBEHaystack));
            api.RegisterItemClass("fogreeditem", typeof(FOGCattailRoot));
            api.RegisterItemClass("fogdrygrass", typeof(FOGDryGrass));
            api.RegisterBlockClass("foghaystack", typeof(FOGHaystack));
            api.RegisterBlockClass("foghaybale", typeof(FOGHaybale));
            api.RegisterBlockClass("fogstrawmat", typeof(FOGStrawMat));
            PatchGame();

            System.Diagnostics.Debug.WriteLine("Fields of Gold initializing");
            
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
                if (FieldsOfGoldConfig.Current.hiveHoursToHarvest <= 0)
                    FieldsOfGoldConfig.Current.hiveHoursToHarvest = 1488;
                api.StoreModConfig(FieldsOfGoldConfig.Current, "fieldsofgold.json");
            }
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
            var patches = Harmony.GetPatchInfo(original);
            if (patches != null && patches.Owners.Contains("harmoniousfog"))
            {
                return;
            }
            harmony.PatchAll();
        }

        private void UnPatchGame()
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
        [HarmonyPrefix]
        static bool Patch_BlockEntityFarmland_GetHoursForNextStage_Prefix(
            BlockEntityFarmland __instance,
            ref double __result,
            Random ___rand)
        {
            var block = __instance.CallMethod<Block>("GetCrop");
            if (block == null)
            {
                __result = 99999999;
                return false;
            }
            System.Diagnostics.Debug.WriteLine("Harmony Patch 1 Run");
            __result = __instance.Api.World.Calendar.HoursPerDay * block.CropProps.TotalGrowthDays
                            * (__instance.Api.World.Calendar.DaysPerMonth / 30.0) / block.CropProps.GrowthStages
                            * 1 / __instance.GetGrowthRate(block!.CropProps.RequiredNutrient)
                            * (float)(0.9 + 0.2 * ___rand.NextDouble());
            System.Diagnostics.Debug.WriteLine("Harmony Patch 1 Results = " + __result);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BlockEntityFarmland), "GetBlockInfo")]
        static void Patch_BlockEntityFarmland_GetHoursForNextStage_Postfix(BlockEntityFarmland __instance, StringBuilder dsc)
        {
            double daysTillNextStage;
            daysTillNextStage = Math.Round((__instance.TotalHoursForNextStage-__instance.Api.World.Calendar.TotalHours) / __instance.Api.World.Calendar.HoursPerDay);
            if (daysTillNextStage > 1)
            {
                dsc.AppendLine("Crop will reach next stage in " + daysTillNextStage + " days.");
                
            }
            else
            {
                dsc.AppendLine("Crop will reach next stage in less than a day.");
            }
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
    }
}

