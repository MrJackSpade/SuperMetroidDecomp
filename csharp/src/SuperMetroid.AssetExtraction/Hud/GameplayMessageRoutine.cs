namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// The setup and draw routines a message definition names, which the message extractors validate (#1416). Values are
/// arbitrary; <c>GameplayMessageRoutineWords</c> names the cartridge routine each one is.
/// </summary>
internal enum GameplayMessageRoutine
{
    /// <summary>Writes the large message box's border layout.</summary>
    DrawLargeTilemap = 1,
    /// <summary>Writes the small message box's border layout.</summary>
    DrawSmallTilemap,
    /// <summary>Patches the configured shoot-button glyph and selects the large box.</summary>
    PatchShootButton,
    /// <summary>Patches the configured run-button glyph and selects the large box.</summary>
    PatchRunButton,
    /// <summary>Prepares the screen for a small message box.</summary>
    SetupSmall,
    /// <summary>Prepares the screen for a large message box.</summary>
    SetupLarge,
}
