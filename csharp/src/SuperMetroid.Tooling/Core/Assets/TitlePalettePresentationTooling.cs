using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Development-tool members of <see cref="TitlePalettePresentation"/>; never linked by player hosts.</summary>
internal static class TitlePalettePresentationTooling
{
    /// <summary>Installed ambient-color identities for the development dependency auditor.</summary>
    internal static IReadOnlyCollection<ushort> ColorPointers => new AmbientPointerSequence();

    internal sealed class AmbientPointerSequence : IReadOnlyCollection<ushort>
    {
        public int Count => TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All.Sum(
            program => program.FrameCount * program.ColorsPerFrame);
        public IEnumerator<ushort> GetEnumerator()
        {
            foreach (var program in TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All)
            for (int frame = 0; frame < program.FrameCount; frame++)
            for (int index = 0; index < program.ColorsPerFrame; index++)
                yield return (ushort)(program.FramePointer(frame) + sizeof(ushort) * (index + 1));
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
