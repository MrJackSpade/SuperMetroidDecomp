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
    StandardHudTiles
}
