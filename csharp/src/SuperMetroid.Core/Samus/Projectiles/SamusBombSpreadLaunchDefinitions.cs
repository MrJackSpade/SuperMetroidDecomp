namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Fixed Bomb Spread mechanics, not editable explosion artwork or animation.</summary>
internal static class SamusBombSpreadLaunchDefinitions
{
    /// <summary>The centre bomb's rise, two and a half pixels per update; the bombs beside it rise one pixel and the edge bombs none.</summary>
    internal static UnsignedSixteenSixteen CentreRise { get; } = UnsignedSixteenSixteen.FromSixteenthsOfPixel(40);
}
