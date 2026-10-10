using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Development-tool members of <see cref="TitlePalettePresentation"/>; never linked by player hosts.</summary>
internal static class TitlePalettePresentationTooling
{
    /// <summary>Installed ambient-color identities for the development dependency auditor.</summary>
    internal static IReadOnlyCollection<ushort> ColorPointers => new AmbientPointerSequence();

    /// <summary>Enumerates every color-word address referenced by the compiled title ambient-palette programs.</summary>
    internal sealed class AmbientPointerSequence : IReadOnlyCollection<ushort>
    {
        /// <summary>Gets the total number of color-word addresses across all program frames.</summary>
        public int Count => TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All.Sum(
            program => program.FrameCount * program.ColorsPerFrame);

        /// <summary>Returns each frame's color-word addresses in program, frame, and color order.</summary>
        /// <returns>An enumerator over ambient palette color-word addresses.</returns>
        public IEnumerator<ushort> GetEnumerator()
        {
            foreach (var program in TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All)
            for (int frame = 0; frame < program.FrameCount; frame++)
            for (int index = 0; index < program.ColorsPerFrame; index++)
                yield return (ushort)(program.FramePointer(frame) + sizeof(ushort) * (index + 1));
        }
        /// <summary>Returns a non-generic enumerator over the ambient palette color-word addresses.</summary>
        /// <returns>An enumerator that follows program, frame, and color order.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
