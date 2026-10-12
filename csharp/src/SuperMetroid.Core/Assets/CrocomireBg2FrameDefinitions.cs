namespace SuperMetroid.Core.Assets;

/// <summary>
/// Named BG2 stream identities for the 42 mixed OAM/BG2 Crocomire fight-body
/// frames. Their OAM components are authored in the extended-enemy catalog;
/// native hitbox pointers and the new-instruction-frame gate remain engine data.
/// </summary>
internal static class CrocomireBg2FrameDefinitions
{
    /// <summary>Schema version for the selected mixed-frame BG2 artwork file.</summary>
    internal const int Version = 1;
    /// <summary>Installed BG2 visual resource for Crocomire's mixed fight frames.</summary>
    internal const string FileName = "crocomire-bg2-frames.json";
    /// <summary>Native bank $A4 shared with Crocomire's compiled body-frame identities.</summary>
    internal const byte Bank = CrocomireBodyVisualDefinitions.Bank;

    /// <summary>Enumerates the first 42 body roots that carry BG2 components, in the compiled frame order.</summary>
    internal static EnemyBg2FrameDefinitionSequence Frames =>
        new(CrocomireBodyVisualDefinitions.MixedBg2FrameCount, Frame);

    /// <summary>The first 42 selected body roots have BG2 components. Preserve
    /// their published artwork keys with an uppercase four-digit native pointer.</summary>
    internal static EnemyBg2FrameDefinition Frame(int index)
    {
        if ((uint)index >= CrocomireBodyVisualDefinitions.MixedBg2FrameCount)
            throw new IndexOutOfRangeException();
        ushort pointer = CrocomireBodyVisualDefinitions.FramePointer(index);
        return new(pointer, $"crocomire_body_bg2_{pointer:X4}");
    }
}
