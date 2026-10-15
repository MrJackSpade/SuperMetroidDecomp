namespace SuperMetroid.Core.Frontend.Options;

/// <summary>The three rows of the special-settings page, valued by their cursor index.</summary>
public enum GameOptionsSpecialRow
{
    /// <summary>Special row zero: toggles Icon Cancel.</summary>
    IconCancel = GameOptionsRomData.Rows.SpecialIconCancel,
    /// <summary>Special row one: toggles Moonwalk.</summary>
    Moonwalk = GameOptionsRomData.Rows.SpecialMoonwalk,
    /// <summary>Special row two: returns to the primary page.</summary>
    Exit = GameOptionsRomData.Rows.SpecialExit,
}
