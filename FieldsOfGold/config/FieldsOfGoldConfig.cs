using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;

namespace FieldsOfGold.config
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    class FieldsOfGoldConfig
    {
        public int hiveHoursToHarvest = 1488;
        public int DaysBerryEmptyToFlower = 60;
        public int DaysBerryFlowerToRipe = 28;
        public int DaysBerryRipeToEmpty = 14;
        public int dryGrassPerHaystackBlock = 256;
        public int dryGrassPerMat = 8;
        public int dryGrassAddedPerInteract = 16;
        public int dryGrassAddedPerInteractWithShiftSneak = 64;
        public int cattailPerMat = 4;
        public int maxShownStageLengthDays = 1500;

        public FieldsOfGoldConfig()
        {}

        public static FieldsOfGoldConfig Current { get; set; }

        public static FieldsOfGoldConfig GetDefault()
        {
            FieldsOfGoldConfig defaultConfig = new();

            defaultConfig.hiveHoursToHarvest = 1488;
            defaultConfig.DaysBerryEmptyToFlower = 60;
            defaultConfig.DaysBerryFlowerToRipe = 28;
            defaultConfig.DaysBerryRipeToEmpty = 14;
            defaultConfig.dryGrassPerHaystackBlock = 32;
            defaultConfig.dryGrassPerMat = 8;
            defaultConfig.dryGrassAddedPerInteract = 16;
            defaultConfig.dryGrassAddedPerInteractWithShiftSneak = 64;
            defaultConfig.cattailPerMat = 4;
            defaultConfig.maxShownStageLengthDays = 1500;

            //defaultConfig.HiveSeasons = 
            return defaultConfig;
        }

    }
}
