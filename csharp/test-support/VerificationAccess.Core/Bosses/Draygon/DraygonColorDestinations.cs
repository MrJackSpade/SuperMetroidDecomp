namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Where Draygon's colours land in the colour memory, which only verification reads.</summary>
internal static class DraygonColorDestinations
{
    /// <summary>Colour 144, the first of sprite palette one; the 25-colour introductory transfer ends at colour 168.</summary>
    public const int IntroDestination = 144;
    /// <summary>Colour 80, the start of background palette five; the normal and white-flash transfers replace 80..95.</summary>
    public const int BackgroundDestination = 80;
    /// <summary>Colour 240, the start of sprite palette seven; the normal and white-flash transfers replace 240..255.</summary>
    public const int SpriteDestination = 240;
    /// <summary>Colour 89, background palette five's colour nine; a health-band update replaces only 89..92.</summary>
    public const int HealthDestination = 89;
}
