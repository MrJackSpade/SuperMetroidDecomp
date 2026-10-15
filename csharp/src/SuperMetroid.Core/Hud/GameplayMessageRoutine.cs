namespace SuperMetroid.Core.Hud;

/// <summary>The bank-$85 setup and draw routines a message definition names.</summary>
public enum GameplayMessageRoutine : ushort
{
    /// <summary>$85:825A, writes the large message-box border layout.</summary>
    DrawLargeTilemap = 0x825a,
    /// <summary>$85:8289, writes the small message-box border layout.</summary>
    DrawSmallTilemap = 0x8289,
    /// <summary>$85:83C5, patches the configured shoot-button glyph and selects the large box.</summary>
    PatchShootButton = 0x83c5,
    /// <summary>$85:83CC, patches the configured run-button glyph and selects the large box.</summary>
    PatchRunButton = 0x83cc,
    /// <summary>$85:8436, configures PPU state for a small message box.</summary>
    SetupSmall = 0x8436,
    /// <summary>$85:8441, configures PPU state for a large message box.</summary>
    SetupLarge = 0x8441,
}
