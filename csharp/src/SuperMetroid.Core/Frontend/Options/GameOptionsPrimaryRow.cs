namespace SuperMetroid.Core.Frontend.Options;

/// <summary>The five rows of the primary options page; <see cref="GameOptionsLayout"/> places them from the top.</summary>
public enum GameOptionsPrimaryRow
{
    /// <summary>Begins the options fade-out and the following start-game transition.</summary>
    StartGame,
    /// <summary>Selects English text, toggling the language.</summary>
    EnglishText,
    /// <summary>Selects Japanese text, toggling the language.</summary>
    JapaneseText,
    /// <summary>Dissolves to the controller-assignment page.</summary>
    ControllerSettings,
    /// <summary>Dissolves to the Icon Cancel/Moonwalk special-settings page.</summary>
    SpecialSettings,
}
