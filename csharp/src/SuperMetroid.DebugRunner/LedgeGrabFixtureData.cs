/// <summary>Geometry and measured original-CPU witnesses for the ledge-grab comparison.</summary>
internal static class LedgeGrabFixtureData
{
    /// <summary>Top boundary of the single tested row-32 platform.</summary>
    public const int PlatformTop = 512;

    /// <summary>Top boundary of the lower row-48 floor, where missed grabs land.</summary>
    public const int FloorTop = 768;

    /// <summary>Native radius-21 landing center, including the downward-scan fraction.</summary>
    public const uint LandingCenter = 0x01ebffff;

    /// <summary>Live base speed retained when the seeded downward-aim jump expands.</summary>
    public const uint ExpansionBaseSpeed = 0x00014000;
}
