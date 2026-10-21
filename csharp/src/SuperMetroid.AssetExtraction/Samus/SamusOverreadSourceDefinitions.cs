namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Where on the cartridge each range in the installed Samus overread file starts.</summary>
internal static class SamusOverreadSourceDefinitions
{
    /// <summary>$80:9131, the first program byte the SpaceTime palette-copy overrun reads (through its $00:9131 mirror).</summary>
    internal const int SpacetimeCopySource = 0x809131;

    /// <summary>$9B:C90A, the grapple-handler code the Speed Booster palette overrun reads as sixteen colours.</summary>
    internal const int SpeedBoosterGrappleCode = 0x9bc90a;

    /// <summary>$90:DD75, the turning and posture-transition handler code before the HUD's posture-transition flags.</summary>
    internal const int HudPrecedingPostureBytes = 0x90dd75;

    /// <summary>$90:DDB6, the spin, held and X-ray HUD handler code after the HUD's posture-transition flags.</summary>
    internal const int HudFollowingPostureBytes = 0x90ddb6;
}
