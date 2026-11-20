namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>A Powamp's balloon, and how far the grapple pulls it (#1472, #1519).</summary>
/// <param name="GrappleTravelDistance">The distance in pixels the grapple pulls the balloon.</param>
/// <param name="Contact">Whether Samus's touch and shots reach the balloon, as its placement says.</param>
internal sealed record PowampBalloonSpawnSettings(int GrappleTravelDistance, PowampContact Contact) : PowampSpawnSettings(Contact);
