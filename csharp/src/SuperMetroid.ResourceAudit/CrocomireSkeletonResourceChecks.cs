using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified skeleton export and component-capacity omissions, not the battle.</summary>
internal static class CrocomireSkeletonResourceChecks
{
    internal static void Run()
    {
        ExtendedEnemyCompositionResourceChecks.Run("Crocomire skeleton",
            CrocomireSkeletonVisualDefinitions.Frames.ToArray(),
            (int)EnemyExtendedFrameSchema.PreCrocomireSkeleton,
            EnemyExtendedFrameDefinitions.PreCrocomireSkeletonFrameCount);
        ConfirmFragmentationCapacity();
    }

    private static void ConfirmFragmentationCapacity()
    {
        EnemyExtendedFrameDefinition frame = CrocomireSkeletonVisualDefinitions.Frames.ToArray()
            .Single(item => item.Pointer == CrocomireSkeletonVisualDefinitions.FirstThirteenComponentFrame);
        int count = CrocomireSkeletonVisualDefinitions.MaximumComponents;
        var source = ConstructedComponentSource(frame, count);
        EnemyExtendedVisualComponent[] extracted = EnemyExtendedFrameFiles.ExtractComponents(source, frame);
        ExtendedEnemyCompositionResourceChecks.Require(extracted.Length == count,
            "the importer must retain all thirteen collapse components, not truncate to eight");
        EnemyExtendedFrameDocument document = ExtendedEnemyCompositionResourceChecks.CreateDocument();
        document.Frames[frame.Name] = extracted;
        EnemyExtendedFrameCatalog installed = ExtendedEnemyCompositionResourceChecks.Load(document);
        ExtendedEnemyCompositionResourceChecks.Require(installed.TryGetDisplay(frame.Bank, frame.Pointer,
            out var components) && components.Length == count, "all thirteen components must survive installation");
        for (int index = 0; index < count; index++)
        {
            var component = components.Span[index];
            ExtendedEnemyCompositionResourceChecks.Require(component.Offset.X == index && component.Offset.Y == -index &&
                component.Parts.Length == 1 && component.Parts[0].X.SignedOffset == 3 && component.Parts[0].Y == 4,
                "the importer/loader must preserve component order, signed offsets and OAM pieces");
        }
        Reject(() => EnemyExtendedFrameFiles.ExtractComponents(ConstructedComponentSource(frame, count + 1), frame),
            "the skeleton importer must still reject a fourteenth component");
        document.Frames[frame.Name] = [.. extracted, extracted[0]];
        Reject(() => ExtendedEnemyCompositionResourceChecks.Load(document),
            "the skeleton loader must still reject a fourteenth component");
        var ordinary = EnemyExtendedFrameDefinitions.Frames.First();
        int ordinaryCount = EnemyExtendedFrameDefinitions.MaximumComponents + 1;
        Reject(() => EnemyExtendedFrameFiles.ExtractComponents(ConstructedComponentSource(ordinary, ordinaryCount), ordinary),
            "other native OAM families must retain their existing component bound");
        document.Frames[frame.Name] = extracted;
        document.Frames[ordinary.Name] = Enumerable.Repeat(extracted[0], ordinaryCount).ToArray();
        Reject(() => ExtendedEnemyCompositionResourceChecks.Load(document),
            "other installed OAM families must retain their existing component bound");
        Console.WriteLine("Crocomire collapse: importer and loader preserve thirteen ordered components; family-specific limits remain enforced.");
    }

    private static void Reject(Action action, string message)
    {
        bool rejected = false;
        try { action(); }
        catch (InvalidDataException) { rejected = true; }
        ExtendedEnemyCompositionResourceChecks.Require(rejected, message);
    }

    /// <summary>Synthetic import bytes for exactly one root; unspecified reads, including hitboxes, fail.</summary>
    [CpuBusBoundary("exact bus test: lays out one native extended-frame root and its components at their cartridge addresses")]
    private static SyntheticCartridge ConstructedComponentSource(EnemyExtendedFrameDefinition frame, int count)
    {
        var source = new SyntheticCartridge(
            address => new InvalidDataException($"Unexpected synthetic composition read {address:X6}."),
            _ => new InvalidOperationException("Static import must not mutate emulated state."));
        int address = ((int)new SnesAddress(frame.Bank, frame.Pointer));
        int sprite = ((int)new SnesAddress(frame.Bank, checked((ushort)(0x8000))));
        source.SetWord(address, count);
        for (int index = 0; index < count; index++)
        {
            int record = address + 2 + index * 8;
            source.SetWord(record, index);
            source.SetWord(record + 2, -index);
            source.SetWord(record + 4, sprite & 0xffff);
            // No hitbox bytes: extracting editable art must not read native mechanics.
        }
        source.SetWord(sprite, 1);
        source.SetWord(sprite + 2, 3);
        source.SetByte(sprite + 4, 4);
        source.SetWord(sprite + 5, 0);
        return source;
    }
}
