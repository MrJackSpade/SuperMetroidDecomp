using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Development-tool members of <see cref="TitlePalettePresentation"/>; never linked by player hosts.</summary>
internal static class TitlePalettePresentationTooling
{
    /// <summary>Installed ambient-color identities for the development dependency auditor.</summary>
    internal static IReadOnlyCollection<ushort> ColorPointers => new AmbientPointerSequence();

    /// <summary>Enumerates each ambient effect's color operand addresses without materializing an array.</summary>
    internal sealed class AmbientPointerSequence : IReadOnlyCollection<ushort>
    {
        /// <summary>The number of color operands across all installed title ambient frames.</summary>
        public int Count => TitleScreenAmbientPaletteFxColorDefinitions.All.Sum(
            program => program.FrameCount * program.ColorsPerFrame);
        /// <summary>Walks program frames in catalog order, skipping each frame's instruction word.</summary>
        public IEnumerator<ushort> GetEnumerator()
        {
            foreach (var program in TitleScreenAmbientPaletteFxColorDefinitions.All)
            for (int frame = 0; frame < program.FrameCount; frame++)
            for (int index = 0; index < program.ColorsPerFrame; index++)
                yield return (ushort)(program.FramePointer(frame) + sizeof(ushort) * (index + 1));
        }
        /// <summary>Provides the same pointer sequence to non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
