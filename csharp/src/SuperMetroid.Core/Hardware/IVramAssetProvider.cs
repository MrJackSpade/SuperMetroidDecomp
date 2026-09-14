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
    ProjectileMissileTrailTiles
}
