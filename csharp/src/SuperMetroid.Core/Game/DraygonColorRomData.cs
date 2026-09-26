namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A5 Draygon visual palette sources and native CGRAM transfer geometry.</summary>
public static class DraygonColorRomData
{
    /// <summary>
    /// Body initializer at $A5:8687 copies 25 words from $A5:A217: all of sprite palette
    /// one and the first nine words of sprite palette two, into target colors 144..168.
    /// </summary>
    public const int IntroSource = 0xa5a217;
    public const int IntroCount = 25;
    public const int IntroDestination = 144;

    /// <summary>Normal BG2 body palette at $A5:A277, copied by hurt AI $A5:954D.</summary>
    public const int BackgroundSource = 0xa5a277;
    public const int BackgroundCount = 16;
    public const int BackgroundDestination = 80;

    /// <summary>Normal sprite palette seven at $A5:A1F7, copied by hurt AI $A5:954D.</summary>
    public const int SpriteSource = 0xa5a1f7;
    public const int SpriteCount = 16;
    public const int SpriteDestination = 240;

    /// <summary>White-flash palette at $A5:A297, copied to both BG2 and sprite colors.</summary>
    public const int WhiteFlashSource = 0xa5a297;
    public const int WhiteFlashCount = 16;

    /// <summary>Eight four-color health bands at $A5:96AF; thresholds remain compiled.</summary>
    public const int HealthBandsSource = 0xa596af;
    public const int HealthBandCount = 8;
    public const int HealthBandColorCount = 4;
    public const int HealthDestination = 89;
}
