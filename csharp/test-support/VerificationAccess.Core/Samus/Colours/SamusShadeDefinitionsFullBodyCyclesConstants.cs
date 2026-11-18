namespace SuperMetroid.Core.Samus.Colours;

/// <summary>Cartridge values and helpers of <see cref="SamusShadeDefinitions.FullBodyCycles"/> that only verification reads.</summary>
internal static class SamusShadeDefinitionsFullBodyCyclesConstants
{
        /// <summary>The steps in each suit's active Speed Booster cycle list.</summary>
        public const int SpeedBoosterSteps = 4;
        /// <summary>The Speed Booster step the cycle holds once reached: its list's last.</summary>
        public const int SpeedBoosterLastStep = SpeedBoosterSteps - 1;
        /// <summary>The steps in each suit's stored-shine cycle list.</summary>
        public const int StoredShineSteps = 6;
        /// <summary>The steps in each suit's active-shinespark cycle list.</summary>
        public const int ActiveShinesparkSteps = 4;
}
