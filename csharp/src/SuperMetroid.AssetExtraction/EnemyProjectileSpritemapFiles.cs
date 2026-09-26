using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports authored bank-$8D projectile OAM parts, not animation control flow.</summary>
internal static class EnemyProjectileSpritemapFiles
{
    internal static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach ((ushort pointer, string name) in EnemyProjectileSpritemapDefinitions.Frames)
            frames.Add(name, EnemySpritemapFiles.ExtractParts(bus, 0x8d, pointer));
        var programFrames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (EnemyProjectilePresentationFrameDefinition frame in
                 EnemyProjectileInstructionMechanicsDefinitions.VisualFrames)
        {
            ushort pointer = unchecked((ushort)(
                bus.ReadByte(0x860000 | frame.OperandAddress) |
                bus.ReadByte(0x860000 | unchecked((ushort)(frame.OperandAddress + 1))) << 8));
            programFrames.Add(frame.Name, EnemySpritemapFiles.ExtractParts(bus, 0x8d, pointer));
        }
        return EnemyProjectileSpritemapCatalog.Write(new EnemyProjectileSpritemapDocument
        {
            Version = EnemyProjectileSpritemapDefinitions.Version,
            Frames = frames,
            ProgramFrames = programFrames,
        });
    }
}
