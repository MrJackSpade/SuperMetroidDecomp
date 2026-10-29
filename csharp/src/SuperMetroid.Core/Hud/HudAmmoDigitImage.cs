using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.Core.Hud;

/// <summary>One numeral of an ammunition count (#1466).</summary>
/// <param name="Numeral">The numeral shown, zero through nine.</param>
public sealed record HudAmmoDigitImage(int Numeral) : HudImage
{
    /// <summary>The numeral shown, zero through nine.</summary>
    public int Numeral { get; } = Ensure.BetweenInclusive(0, GameplayHudDefinitions.Numerals - 1, Numeral);

    /// <inheritdoc/>
    public override string FrameName => $"Hud.AmmoDigit.{Numeral}";
}
