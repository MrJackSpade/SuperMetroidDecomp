namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The compatibility services that reproduce what Kraid's fingernails, Crocomire's spit volley and the opening
/// cinematic's egg shell read past the data the original game meant them to read (#1500), as the import pipeline decoded
/// them from the player's game data. The session's gameplay runtime binds them to its enemy session, which hands each
/// boss's service to that boss's fight; the opening cinematic hands the shell's to its fragments. Lifetime: the
/// installation's, shared by every runtime and cinematic of the session.
/// </summary>
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
