using SuperMetroid.Core.Rom;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports authored bank-$8D projectile OAM parts, not animation control flow.</summary>
[CpuBusBoundary("codec: ROM bank-$86 projectile instruction operands and bank-$8D spritemaps → enemy projectile spritemaps")]
public static class EnemyProjectileSpritemapFiles
{
    /// <summary>Imports named enemy-projectile OAM compositions, including frames selected by native instruction operands.</summary>
    /// <param name="bus">Non-null cartridge address space supplying bank $8D spritemaps and their bank $86 instruction operands.</param>
    /// <returns>New UTF-8 JSON bytes containing base and program-use frames with signed pixel offsets, tile references, sizes, palettes, priorities, and flips.</returns>
    /// <remarks>Only visual compositions are exported; projectile instruction flow and frame timing remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A native spritemap exceeds the supported part count or the resulting visual document is invalid.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach ((ushort pointer, string name) in EnemyProjectileSpritemapDefinitions.Frames)
            frames.Add(name, EnemySpritemapFiles.ExtractParts(bus, 0x8d, pointer));
        var programFrames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (EnemyProjectilePresentationFrameDefinition frame in
                 EnemyProjectilePresentationFrameDefinitions.All)
        {
            ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(EnemyCartridgeTables.ProjectileInstructionBank, frame.OperandAddress));
            programFrames.Add(frame.Name, EnemySpritemapFiles.ExtractParts(bus, 0x8d, pointer));
        }
        return EnemyProjectileSpritemapCatalog.Write(new EnemyProjectileSpritemapDocument
        {
            Version = (int)EnemyProjectileSpritemapVersion.Current,
            Frames = frames,
            ProgramFrames = programFrames,
        });
    }
}
