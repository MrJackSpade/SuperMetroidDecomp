namespace SuperMetroid.Core.Game;

/// <summary>Native shared gold-Pirate palette selected when a ninja Space Pirate initializes.</summary>
internal static class NinjaSpacePiratePaletteDefinitions
{
    /// <summary>
    /// Enemy $F413's palette header resolves to $B2:8727. Ninja Pirate initialization
    /// copies this same 16-color source to its target OBJ palette, irrespective of the
    /// ninja actor's own definition or starting color.
    /// </summary>
    public const ushort SharedGoldPirateDefinition = 0xf413;

    /// <summary>Native source of the shared gold-Pirate color words at $B2:8727.</summary>
    public const int SharedGoldPirateSource = 0xb28727;

    /// <summary>Native target OBJ palette seven, CGRAM color $F0.</summary>
    public const int TargetColor = 240;
}
