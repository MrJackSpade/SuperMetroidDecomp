namespace SuperMetroid.Core.Assets;

/// <summary>
/// Named BG2 stream identities for the 42 mixed OAM/BG2 Crocomire fight-body
/// frames. Their OAM components are authored in the extended-enemy catalog;
/// native hitbox pointers and the new-instruction-frame gate remain engine data.
/// </summary>
internal static class CrocomireBg2FrameDefinitions
{
    /// <summary>Schema version passed to the BG2 frame extractor and catalog loader.</summary>
    internal const int Version = 1;

    /// <summary>Published JSON filename used for Crocomire's extracted BG2 frame definitions.</summary>
    internal const string FileName = "crocomire-bg2-frames.json";

    /// <summary>ROM bank containing the source data for Crocomire's BG2 frame artwork.</summary>
    internal const byte Bank = CrocomireBodyVisualDefinitions.Bank;

    /// <summary>Indexed definitions for the mixed Crocomire body frames that include BG2 artwork.</summary>
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
