
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values of <see cref="BabyMetroidCutsceneColorRomData"/> that only verification reads.</summary>
internal static class BabyMetroidCutsceneColorRomDataConstants
{
    /// <summary>$AD:E8E2, seven fade-frame pointers; index zero is never displayed.</summary>
    public const int FadePointerTable = 0xade8e2;

    /// <summary>Provides the native fade table pointer used to verify the Baby Metroid cutscene color schedule.</summary>
    extension(BabyMetroidCutsceneColorRomData)
    {
        /// <inheritdoc cref="BabyMetroidCutsceneColorRomDataConstants.FadePointerTable"/>
        internal static int FadePointerTable => BabyMetroidCutsceneColorRomDataConstants.FadePointerTable;
    }
}
