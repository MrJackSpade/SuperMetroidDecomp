namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Which parts of Phantoon's body his fight makes hittable while his body shows.</summary>
internal enum PhantoonBodyHitArea
{
    /// <summary>None: only his centre point, which absorbs a shot or a bomb as he turns solid (<see cref="PhantoonSolidifyingCentre"/>).</summary>
    SolidifyingCentre,
    /// <summary>His whole body: its centre, eye and three tentacles.</summary>
    WholeBody,
    /// <summary>His vulnerable eye alone.</summary>
    EyeOnly,
}
