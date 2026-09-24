namespace SuperMetroid.Core.Hardware;

/// <summary>Resolves immutable compiled artwork at NMI; references, not content blobs, belong in pending debugger state.</summary>
public interface IVramAssetProvider
{
    ReadOnlyMemory<byte> Resolve(VramAssetId asset);
}

/// <summary>Mutually exclusive compiled artwork sources understood by the VRAM queue.</summary>
public enum VramAssetId
{
    /// <summary>Existing cartridge/WRAM transfer; reads SourceAddress instead of an installed resource.</summary>
    None,
    /// <summary>Standard gameplay BG3 character sheet followed by its native zero padding.</summary>
    StandardHudTiles,
    /// <summary>Escape-timer OBJ tiles 480 through 495 from the first native transfer record.</summary>
    EscapeTimerFirstTiles,
    /// <summary>Escape-timer OBJ tiles 496 through 504 from the second native transfer record.</summary>
    EscapeTimerSecondTiles,
    /// <summary>$90:C3B1 selection 0: Power beam artwork.</summary>
    BeamPowerTiles,
    /// <summary>$90:C3B1 selection 1: Wave beam artwork.</summary>
    BeamWaveTiles,
    /// <summary>$90:C3B1 selection 2: Ice beam artwork.</summary>
    BeamIceTiles,
    /// <summary>$90:C3B1 selection 3: Ice/Wave artwork.</summary>
    BeamIceWaveTiles,
    /// <summary>$90:C3B1 selection 4: Spazer artwork.</summary>
    BeamSpazerTiles,
    /// <summary>$90:C3B1 selection 5: Spazer/Wave artwork.</summary>
    BeamSpazerWaveTiles,
    /// <summary>$90:C3B1 selection 6: Spazer/Ice artwork.</summary>
    BeamSpazerIceTiles,
    /// <summary>$90:C3B1 selection 7: Spazer/Ice/Wave artwork.</summary>
    BeamSpazerIceWaveTiles,
    /// <summary>$90:C3B1 selection 8: Plasma artwork.</summary>
    BeamPlasmaTiles,
    /// <summary>$90:C3B1 selection 9: Plasma/Wave artwork.</summary>
    BeamPlasmaWaveTiles,
    /// <summary>$90:C3B1 selection 10: Plasma/Ice artwork.</summary>
    BeamPlasmaIceTiles,
    /// <summary>$90:C3B1 selection 11: Plasma/Ice/Wave artwork.</summary>
    BeamPlasmaIceWaveTiles,
    /// <summary>The eight ice/wave projectile-trail characters.</summary>
    ProjectileIceWaveTrailTiles,
    /// <summary>The four missile/Super Missile trail characters.</summary>
    ProjectileMissileTrailTiles,
    /// <summary>First Grapple endpoint frame from $9A:8200.</summary>
    GrapplePointFirstTiles,
    /// <summary>Second Grapple endpoint frame from $9A:8400.</summary>
    GrapplePointSecondTiles,
    /// <summary>Third Grapple endpoint frame from $9A:8600.</summary>
    GrapplePointThirdTiles,
    /// <summary>Fourth Grapple endpoint frame from $9A:8800.</summary>
    GrapplePointFourthTiles,
    /// <summary>Four horizontal Grapple segment frames from $9A:8220.</summary>
    GrappleHorizontalSegmentTiles,
    /// <summary>Four diagonal Grapple segment frames from $9A:8A20.</summary>
    GrappleDiagonalSegmentTiles,
    /// <summary>Four vertical Grapple segment frames from $9A:9220.</summary>
    GrappleVerticalSegmentTiles,
    // Append-only: numeric values appear in pending debugger-state VRAM records.
    /// <summary>First quarter of the installed standard BG3 sheet restored during Kraid's death.</summary>
    KraidBg3RestoreQuarter0,
    /// <summary>Second quarter of the installed standard BG3 sheet restored during Kraid's death.</summary>
    KraidBg3RestoreQuarter1,
    /// <summary>Third quarter of the installed standard BG3 sheet restored during Kraid's death.</summary>
    KraidBg3RestoreQuarter2,
    /// <summary>Fourth quarter of the installed standard BG3 sheet restored during Kraid's death.</summary>
    KraidBg3RestoreQuarter3
}
