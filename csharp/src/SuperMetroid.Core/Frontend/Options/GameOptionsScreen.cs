namespace SuperMetroid.Core.Frontend.Options;

/// <summary>The options menu's screens, one shown at a time: the primary page and each settings page in each language.</summary>
public enum GameOptionsScreen
{
    /// <summary>The primary options page.</summary>
    Primary,
    /// <summary>English controller settings.</summary>
    ControllerEnglish,
    /// <summary>Japanese controller settings.</summary>
    ControllerJapanese,
    /// <summary>English special settings.</summary>
    SpecialEnglish,
    /// <summary>Japanese special settings.</summary>
    SpecialJapanese,
}
