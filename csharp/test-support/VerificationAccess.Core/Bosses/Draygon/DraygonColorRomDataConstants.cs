namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Cartridge values and helpers of <see cref="DraygonColorRomData"/> that only verification reads.</summary>
internal static class DraygonColorRomDataConstants
{
    /// <summary>Zero-based CGRAM color index 144 (<c>$90</c>), the first color of OBJ palette one; the 25-color introductory transfer ends at index 168.</summary>
    public const int IntroDestination = 144;
    /// <summary>Zero-based CGRAM color index 80 (<c>$50</c>), the beginning of BG palette five; normal and white-flash transfers replace indices 80..95.</summary>
    public const int BackgroundDestination = 80;
    /// <summary>Zero-based CGRAM color index 240 (<c>$F0</c>), the beginning of OBJ palette seven; normal and white-flash transfers replace indices 240..255.</summary>
    public const int SpriteDestination = 240;
    /// <summary>Zero-based CGRAM color index 89 (<c>$59</c>), BG palette-five pen nine; a health-band update replaces only indices 89..92.</summary>
    public const int HealthDestination = 89;
}
