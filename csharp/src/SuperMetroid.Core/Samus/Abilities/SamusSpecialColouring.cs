namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The one special suit colouring that holds Samus's suit (#1789). The stored shine, the shinespark, Crystal Flash and
/// the X-ray visor share it: starting one takes the suit from whichever held it, and each lets it go only while it
/// still holds it, so no colouring can end another's.
/// </summary>
public sealed class SamusSpecialColouring
{
    /// <summary>The colouring holding Samus's suit, or <see cref="SamusSpecialPaletteType.None"/>.</summary>
    public SamusSpecialPaletteType Holder { get; private set; }

    /// <summary>Whether <paramref name="colouring"/> holds Samus's suit.</summary>
    public bool Holds(SamusSpecialPaletteType colouring) => Holder == colouring;

    /// <summary>Gives Samus's suit to <paramref name="colouring"/>, ending the colouring that held it.</summary>
    internal void Take(SamusSpecialPaletteType colouring) => Holder = colouring;

    /// <summary>Returns Samus's suit to her own colours when <paramref name="colouring"/> holds it; otherwise changes nothing.</summary>
    internal void Release(SamusSpecialPaletteType colouring)
    {
        if (Holder == colouring)
            Holder = SamusSpecialPaletteType.None;
    }
}
