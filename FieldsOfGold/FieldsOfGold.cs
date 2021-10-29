using FieldsOfGold.BlockEntities;
using FieldsOfGold.Blocks;
using FieldsOfGold.Items;
using FromGoldenCombs.config;
using Vintagestory.API.Common;

namespace FieldsOfGold
{
    public class FieldsOfGold : ModSystem
    {
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("fogreeds", typeof(FOGReeds));
            api.RegisterItemClass("fogreeditem", typeof(FOGCattailRoot));
            api.RegisterBlockEntityClass("fogbeberrybush", typeof(FOGBerryBush));
            api.RegisterBlockEntityClass("fogbeehive", typeof(FOGBeehive));
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
    }
}

