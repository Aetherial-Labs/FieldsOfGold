using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;

namespace FieldsOfGold.config
{
    class FieldsOfGoldConfig
    {
        public int hiveHoursToHarvest = 1488;

        public FieldsOfGoldConfig()
        {}

        public static FieldsOfGoldConfig Current { get; set; }

        public static FieldsOfGoldConfig GetDefault()
        {
            FieldsOfGoldConfig defaultConfig = new();

            defaultConfig.hiveHoursToHarvest = 1488;
            //defaultConfig.HiveSeasons = 
            return defaultConfig;
        }

    }
}
