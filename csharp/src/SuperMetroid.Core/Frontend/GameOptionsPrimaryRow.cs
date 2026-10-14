namespace SuperMetroid.Core.Frontend;

/// <summary>The five rows of the primary options page, valued by their cursor index.</summary>
public enum GameOptionsPrimaryRow
{
    /// <summary>Primary row zero: begins options fade-out and the following start-game transition.</summary>
    StartGame = 0,
    /// <summary>Primary row one: selects English text, toggling the language.</summary>
    EnglishText = 1,
    /// <summary>Primary row two: selects Japanese text, toggling the language.</summary>
    JapaneseText = 2,
    /// <summary>Primary row three: dissolves to the controller-assignment page.</summary>
    ControllerSettings = 3,
    /// <summary>Primary row four: dissolves to the Icon Cancel/Moonwalk special-settings page.</summary>
    SpecialSettings = 4,
}
