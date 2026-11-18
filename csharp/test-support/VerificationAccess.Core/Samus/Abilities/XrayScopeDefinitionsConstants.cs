using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Cartridge values and helpers of <see cref="XrayScopeDefinitions"/> that only verification reads.</summary>
internal static class XrayScopeDefinitionsConstants
{
    /// <summary>The visor shades X-ray shows while it is active.</summary>
    public static class VisorShades
    {
        /// <summary>The shades of the steady cycle the full beam shows.</summary>
        public const int FullCycleShades = SamusShadeDefinitions.Visor.CycleShades;
        /// <summary>Frames between visor shade advances.</summary>
        public const int FrameDelay = SamusShadeDefinitionsVisorConstants.FrameDelay;
    }
}
