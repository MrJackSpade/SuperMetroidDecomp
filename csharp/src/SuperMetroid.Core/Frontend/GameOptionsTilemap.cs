namespace SuperMetroid.Core.Frontend;

/// <summary>Mutually exclusive options tilemaps imported by native initialization.</summary>
public enum GameOptionsTilemap
{
    /// <summary>$82:EC66 selects $97:8DF4, the primary options page.</summary>
    Primary,
    /// <summary>$82:EC77 selects $97:8FCD, English controller settings.</summary>
    ControllerEnglish,
    /// <summary>$82:EC88 selects $97:91C4, Japanese controller settings.</summary>
    ControllerJapanese,
    /// <summary>$82:EC99 selects $97:938D, English special settings.</summary>
    SpecialEnglish,
    /// <summary>$82:ECAA selects $97:953A, Japanese special settings.</summary>
    SpecialJapanese,
}
