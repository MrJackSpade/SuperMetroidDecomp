namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The compatibility services that reproduce what Kraid's fingernails, Crocomire's spit volley and the opening
/// cinematic's egg shell read past the data the game meant them to read (#1500), as the import pipeline decoded
/// them from the player's game data. The session's gameplay runtime binds them to its enemy session, which hands each
/// boss's service to that boss's fight; the opening cinematic hands the shell's to its fragments. Lifetime: the
/// installation's, shared by every runtime and cinematic of the session.
/// </summary>
/// <remarks>
/// Evidence: in the original game Kraid's fingernail contour walk, Crocomire's spit-shot aiming and a falling egg-shell
/// fragment's gravity curve each index their authored table with no count, so a nail above every ledge, the volley's
/// tenth shot and a fragment that falls past the curve read the data after it; the Kraid nail contour, Crocomire tenth
/// shot and intro egg motion checks decode that data from the player's game and compare it with the services held here.
/// </remarks>
[CompatibilityService("Kraid's fingernails, Crocomire's volley and the intro egg shell read what lies past their authored data")]
public sealed class BossAndIntroOverreadServices
{
    /// <summary>Keeps every decoded service.</summary>
    /// <param name="kraidNailPastLedges">Where Kraid's body pushes back a fingernail higher than his contour ledges.</param>
    /// <param name="crocomireTenthShotGradient">The gradient Crocomire's last spit shot is aimed by.</param>
    /// <param name="shellFragmentFallPastCurve">How far egg-shell fragments fall after their gravity curve ends.</param>
    public BossAndIntroOverreadServices(
        KraidNailPastLedges kraidNailPastLedges,
        CrocomireTenthShotGradient crocomireTenthShotGradient,
        ShellFragmentFallPastCurve shellFragmentFallPastCurve)
    {
        KraidNailPastLedges = Ensure.NotNull(kraidNailPastLedges);
        CrocomireTenthShotGradient = Ensure.NotNull(crocomireTenthShotGradient);
        ShellFragmentFallPastCurve = Ensure.NotNull(shellFragmentFallPastCurve);
    }

    /// <summary>Where Kraid's body pushes back a fingernail higher than his contour ledges.</summary>
    public KraidNailPastLedges KraidNailPastLedges { get; }

    /// <summary>The gradient Crocomire's last spit shot is aimed by.</summary>
    public CrocomireTenthShotGradient CrocomireTenthShotGradient { get; }

    /// <summary>How far egg-shell fragments fall after their gravity curve ends.</summary>
    public ShellFragmentFallPastCurve ShellFragmentFallPastCurve { get; }
}
