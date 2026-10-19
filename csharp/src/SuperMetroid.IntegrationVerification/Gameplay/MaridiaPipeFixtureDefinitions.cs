namespace SuperMetroid.IntegrationVerification.Gameplay;

/// <summary>Cartridge identities used by the bounded #391 diagnostic.</summary>
internal static class MaridiaPipeFixtureDefinitions
{
    /// <summary>$8F:E2B6 pins Samus to the center of the authored tube opening, X=$0080.</summary>
    public const ushort TubeCenterX = 0x0080;
}
