namespace SuperMetroid.Core.Hardware;

/// <summary>The kinds of source an OBJ character upload can show.</summary>
public enum ObjCharacterSourceKind
{
    /// <summary>An installed VRAM asset.</summary>
    Asset,
    /// <summary>The installed artwork a native upload's source address selects.</summary>
    Transfer,
    /// <summary>One installed Crocomire skeleton chunk.</summary>
    CrocomireSkeletonChunk,
    /// <summary>Bytes copied from game memory.</summary>
    Memory,
}
