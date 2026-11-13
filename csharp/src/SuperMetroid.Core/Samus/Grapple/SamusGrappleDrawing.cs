namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>How Samus draws for her grapple's phase (<see cref="SamusGrappleBeamDrawing.DrawingFor"/>).</summary>
public enum SamusGrappleDrawing
{
    /// <summary>No grapple: Samus's ordinary drawing, charge flare included.</summary>
    Ordinary,

    /// <summary>Firing, connected or wall-grabbing: the grapple's flare before Samus and its beam after her, with no charge flare or echoes.</summary>
    Beam,

    /// <summary>The frame a cancel, release, drop or grapple wall jump begins: Samus and her echoes alone, with no flare or beam.</summary>
    Teardown,
}
