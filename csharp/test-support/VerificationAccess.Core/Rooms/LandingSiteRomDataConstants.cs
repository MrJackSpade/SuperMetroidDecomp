namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of <see cref="LandingSiteRomData"/> that only verification reads.</summary>
internal static class LandingSiteRomDataConstants
{
    /// <summary>Synthetic bank-$83 door used by the intro landing cutscene.</summary>
    public const ushort LandingCutsceneDoorPointer = 0x88fe;

    /// <summary>Provides the landing cutscene door pointer used to check Landing Site transitions.</summary>
    extension(LandingSiteRomData)
    {
        /// <inheritdoc cref="LandingSiteRomDataConstants.LandingCutsceneDoorPointer"/>
        internal static ushort LandingCutsceneDoorPointer => LandingSiteRomDataConstants.LandingCutsceneDoorPointer;
    }
}
