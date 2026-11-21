using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Exports authored bank-$8D projectile OAM parts, not animation control flow.</summary>
public static class EnemyProjectileSpritemapFiles
{
    /// <summary>Imports named enemy-projectile OAM compositions, including program frames read through the bank-$86 operands they are stored at.</summary>
    /// <param name="bus">Non-null cartridge address space supplying bank $8D spritemaps and their bank $86 instruction operands.</param>
    /// <returns>New UTF-8 JSON bytes containing base and program-use frames with signed pixel offsets, character pictures, sizes, palettes, depths, and flips.</returns>
    /// <remarks>Only visual compositions are exported; projectile instruction flow and frame timing remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A native spritemap exceeds the supported part count or the resulting visual document is invalid.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var frames = new Dictionary<string, EnemyProjectilePartDocument[]>(StringComparer.Ordinal);
        foreach (string name in EnemyProjectileSpritemapDefinitions.FrameNames)
            frames.Add(name, EnemySpritemapFiles.ExtractProjectileParts(bus, CartridgeFormatDefinitions.EnemyProjectileSpritemapBank,
                EnemyProjectileCompositionSources.Spritemap(EnemyProjectileFrames.Identity(name))));
        foreach ((SuperMetroid.Core.Areas.Ceres.CeresElevatorArrivalPose pose, string name) in EnemyProjectileSpritemapDefinitions.ElevatorArrivalFrameNames)
            frames.Add(name, EnemySpritemapFiles.ExtractProjectileParts(bus, CartridgeFormatDefinitions.EnemyProjectileSpritemapBank,
                EnemyProjectileCompositionSources.ElevatorArrivalSpritemap(pose)));
        var programFrames = new Dictionary<string, EnemyProjectilePartDocument[]>(StringComparer.Ordinal);
        // The installed file lists program frames in the order their operands are stored in bank $86.
        EnemyProjectilePresentationFrameDefinition[] stored = EnemyProjectilePresentationFrameSets.All.ToArray();
        Array.Sort(stored, (left, right) => EnemyProjectileProgramFrameSources.Operand(left.Frame)
            .CompareTo(EnemyProjectileProgramFrameSources.Operand(right.Frame)));
        foreach (EnemyProjectilePresentationFrameDefinition frame in stored)
        {
            int operand = EnemyProjectileProgramFrameSources.Operand(frame.Frame);
            int pointer = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(EnemySourceDefinitions.ProjectileInstructionBank, operand));
            programFrames.Add(frame.Name, EnemySpritemapFiles.ExtractProjectileParts(bus, CartridgeFormatDefinitions.EnemyProjectileSpritemapBank, pointer));
        }
        return EnemyProjectileSpritemapDocuments.Write(new EnemyProjectileSpritemapDocument
        {
            Version = EnemyProjectileSpritemapDocuments.Version,
            Frames = frames,
            ProgramFrames = programFrames,
        });
    }
}
