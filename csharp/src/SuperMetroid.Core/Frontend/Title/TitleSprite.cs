namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The title's two whole compositions: the Super Metroid logo and the Nintendo copyright. The title cards are typed text
/// (<see cref="TitleCardTyping"/>, #1979). Values are arbitrary; installed title artwork is keyed by these names, and only
/// the extraction pipeline knows which game frame each one came from.
/// </summary>
public enum TitleSprite
{
    /// <summary>The Super Metroid title logo held on the title screen.</summary>
    SuperMetroidLogo,
    /// <summary>The Nintendo copyright line drawn beneath the title logo.</summary>
    NintendoCopyright,
}
