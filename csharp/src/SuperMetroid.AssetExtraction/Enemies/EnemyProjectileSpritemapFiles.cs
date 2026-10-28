using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Exports authored bank-$8D projectile OAM parts, not animation control flow.</summary>
public static class EnemyProjectileSpritemapFiles
{
    /// <summary>Imports named enemy-projectile OAM compositions, including program frames read through the bank-$86 operands they are stored at.</summary>
    /// <param name="bus">Non-null cartridge address space supplying bank $8D spritemaps and their bank $86 instruction operands.</param>
    /// <returns>New UTF-8 JSON bytes containing base and program-use frames with signed pixel offsets, tile references, sizes, palettes, priorities, and flips.</returns>
    /// <remarks>Only visual compositions are exported; projectile instruction flow and frame timing remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A native spritemap exceeds the supported part count or the resulting visual document is invalid.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (string name in EnemyProjectileSpritemapDefinitions.FrameNames)
            frames.Add(name, EnemySpritemapFiles.ExtractParts(bus, CartridgeFormatDefinitions.EnemyProjectileSpritemapBank,
                EnemyProjectileCompositionSources.Spritemap(EnemyProjectileFrames.Identity(name))));
        var programFrames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        // The installed file lists program frames in the order their operands are stored in bank $86.
        EnemyProjectilePresentationFrameDefinition[] stored = EnemyProjectilePresentationFrameSets.All.ToArray();
        Array.Sort(stored, (left, right) => EnemyProjectileProgramFrameSources.Operand(left.Frame)
            .CompareTo(EnemyProjectileProgramFrameSources.Operand(right.Frame)));
        foreach (EnemyProjectilePresentationFrameDefinition frame in stored)
        {
            ushort operand = EnemyProjectileProgramFrameSources.Operand(frame.Frame);
            ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(EnemySourceDefinitions.ProjectileInstructionBank, operand));
            programFrames.Add(frame.Name, EnemySpritemapFiles.ExtractParts(bus, CartridgeFormatDefinitions.EnemyProjectileSpritemapBank, pointer));
        }
        return EnemyProjectileSpritemapCatalog.Write(new EnemyProjectileSpritemapDocument
        {
            Version = (int)EnemyProjectileSpritemapVersion.Current,
            Frames = frames,
            ProgramFrames = programFrames,
        });
    }
}
