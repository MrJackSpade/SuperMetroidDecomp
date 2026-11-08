namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>How far the grapple's muzzle flare has come since the beam fired.</summary>
public enum GrappleFlareStage
{
    /// <summary>No flare: the grapple is torn down, or the flare was cleared; the next connected-beam draw arms it as just fired.</summary>
    None,

    /// <summary>The beam has just fired: every flare draw before the next connected-beam draw restarts the flare on its opening frame.</summary>
    JustFired,

    /// <summary>The flare has been shown past its start and animates on.</summary>
    Shown,
}
