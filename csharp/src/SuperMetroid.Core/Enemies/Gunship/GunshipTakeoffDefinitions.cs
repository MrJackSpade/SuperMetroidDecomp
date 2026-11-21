using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The gunship's takeoff (#1406, #1538): how long its engines take to fire up, and the look they show once they do.</summary>
internal static class GunshipTakeoffDefinitions
{
    /// <summary>
    /// The phase of the engines' rumble count on which the rumble shakes Samus and the gunship down; otherwise it
    /// shakes them up, so the shake alternates every update.
    /// </summary>
    internal static readonly UpdateAlternation RumbleShakesDown = new(1);

    /// <summary>
    /// The updates the escape takeoff waits for the engines to fire up before the rumble begins, the last showing the
    /// takeoff look.
    /// </summary>
    internal const int EngineStartUpdates = 5;

    /// <summary>The frames the fired-up engines rumble before the steady liftoff begins.</summary>
    internal const int EngineRumbleFrames = 128;

    /// <summary>
    /// The rumble frame on which the rumble strengthens: before it the rumble shakes Samus and the gunship one pixel, from
    /// it two, and on it the liftoff's dust clouds rise.
    /// </summary>
    internal const int RumbleStrengthensFrame = 64;

    /// <summary>How far the rumble shakes Samus and the gunship each update before it strengthens, in pixels (#2339).</summary>
    internal const int RumbleShake = 1;

    /// <summary>How far the strengthened rumble shakes Samus and the gunship each update, in pixels (#2339).</summary>
    internal const int StrongRumbleShake = 2;

    /// <summary>How far the steady liftoff raises Samus and the gunship each update, in pixels.</summary>
    internal const int SteadyLiftoffRise = 2;

    /// <summary>The top hull's Y above which the steady liftoff starts accelerating.</summary>
    internal const int AcceleratingLiftoffY = 896;

    /// <summary>The top hull's Y above which the accelerating takeoff completes and the gunship flies off.</summary>
    internal const int TakeoffCompleteY = 256;

    /// <summary>The gunship's look once its engines fire up for the takeoff, which its hull and the takeoff's dust show.</summary>
    internal static EncounterLook TakeoffLook { get; } = new(EncounterArtwork.Gunship, "Takeoff");
}
