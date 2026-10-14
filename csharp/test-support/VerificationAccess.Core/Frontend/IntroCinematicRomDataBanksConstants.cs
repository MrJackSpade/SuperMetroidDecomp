using SuperMetroid.Core.Frontend;

/// <summary>Cartridge values of <see cref="IntroCinematicRomData.Banks"/> that only verification reads.</summary>
internal static class IntroCinematicRomDataBanksConstants
{
    /// <summary>Bank-$8B base for executable intro cinematic instruction streams.</summary>
    public const int CinematicCode = 0x8b0000;

    /// <summary>Provides the cinematic code bank used to resolve intro script pointers.</summary>
    extension(IntroCinematicRomData.Banks)
    {
        /// <inheritdoc cref="IntroCinematicRomDataBanksConstants.CinematicCode"/>
        internal static int CinematicCode => IntroCinematicRomDataBanksConstants.CinematicCode;
    }
}
