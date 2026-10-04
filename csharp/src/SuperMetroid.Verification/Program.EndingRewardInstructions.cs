using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingRewardInstructions(ISnesAddressSpace bus)
    {
        for (int pointer = EndingRewardInstructionDefinitions.Start;
             pointer < EndingRewardInstructionDefinitions.End; pointer += sizeof(ushort))
        {
            int address = (int)new SnesAddress(0x8b, (ushort)pointer);
            ushort nativeWord = (ushort)(bus.ReadByte(address) |
                bus.ReadByte(address + 1) << 8);
            AssertEqual(nativeWord, EndingRewardInstructionDefinitions.ReadWord((ushort)pointer),
                $"ending reward instruction $8B:{pointer:X4} matches cartridge");
        }
        AssertThrows<InvalidDataException>(() =>
            EndingRewardInstructionDefinitions.ReadWord(EndingRewardInstructionDefinitions.End),
            "ending reward reader rejects the next actor family");
        AssertThrows<InvalidDataException>(() =>
            EndingRewardInstructionDefinitions.ReadWord(
                unchecked((ushort)(EndingRewardInstructionDefinitions.Start + 1))),
            "ending reward reader rejects unaligned instructions");

        // Original display operands select catalog frames independently of native storage order.
        ushort[] operands = [
            0xed1f, 0xed27, 0xed33, 0xed37, 0xed3b, 0xed3f, 0xed43, 0xed47,
            0xed4b, 0xed4f, 0xed53, 0xed85, 0xed8f, 0xed97, 0xed9f, 0xeda3,
            0xeda7, 0xedb3, 0xedbb, 0xedc3, 0xedcf, 0xedd9, 0xeddd, 0xede1,
            0xede5, 0xede9, 0xeded, 0xedf1, 0xedf5, 0xee1b, 0xee1f, 0xee23,
            0xee2b, 0xee37, 0xee3f, 0xee47, 0xee4f,
        ];
        string[] names = [
            "suitless-idle-upper", "suitless-lower", "suitless-hair-1", "suitless-hair-2",
            "suitless-hair-3", "suitless-hair-4", "suitless-hair-5", "suitless-hair-6",
            "suitless-hair-7", "suitless-hair-8", "suitless-standing", "suitless-prepare-jump",
            "suitless-jumping", "samus-falling", "samus-landing", "samus-landed",
            "samus-shooting", "suited-idle-body", "suited-helmet-head", "helmetless-head-1",
            "suited-headless-body", "suited-arm-1", "suited-arm-2", "suited-arm-3",
            "suited-arm-4", "suited-arm-5", "suited-arm-6", "suited-arm-7",
            "suited-arm-8", "helmetless-head-2", "helmetless-head-3", "helmetless-head-4",
            "suited-prepare-jump", "suited-jumping", "suited-helmet-jump-head-1", "suited-helmet-jump-head-2",
            "helmetless-jump-head",
        ];
        var catalog = EndingRewardSpriteDefinitions.Frames;
        AssertEqual(37, catalog.Count, "reward frame catalog count");
        for (int i = 0; i < catalog.Count; i++)
        {
            int operand = 0x8b0000 | operands[i];
            ushort pointer = (ushort)(bus.ReadByte(operand) | bus.ReadByte(operand + 1) << 8);
            int header = 0x8c0000 | pointer;
            int count = bus.ReadByte(header) | bus.ReadByte(header + 1) << 8;
            AssertEqual(pointer, catalog[i].Pointer, "reward original frame pointer");
            AssertEqual(count, catalog[i].StockPartCount, "reward original OAM part count");
            AssertEqual(names[i], catalog[i].Name, "reward published artwork key");
            if (i is 0 or 1 or >= 10 and <= 20 or >= 29)
                VerifyEndingRewardCalculatedParts(bus, catalog[i]);
        }
        AssertTrue(catalog.Select(frame => frame.Name).SequenceEqual(names), "reward catalog enumeration order");
        foreach (int invalid in new[] { int.MinValue, -1, 37, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => _ = catalog[invalid], "reward catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingRewardSpriteDefinitions.FramePointer((EndingRewardSpriteFrame)invalid), "reward frame bounds");
        }

        ushort[] starts =
        [
            0xed1d, 0xed25, 0xed2d, 0xed59, 0xed7f, 0xed95, 0xed9d,
            0xedb1, 0xedb9, 0xedc1, 0xedc9, 0xedd3, 0xee0f, 0xee15,
            0xee27, 0xee3d, 0xee4d,
        ];
        foreach (ushort start in starts)
        {
            var native = new IntroDiscoverySprite(120, 120, 0x0a00, start);
            var installed = new IntroDiscoverySprite(120, 120, 0x0a00, start);
            for (int frame = 0; frame < 480; frame++)
            {
                // Scene callbacks own actor allocation and velocity; the
                // independent list interpreters advance across each callback.
                var nativeCallbacks = new List<ushort>();
                var generatedCallbacks = new List<ushort>();
                native.Step(bus, (opcode, cursor) => { nativeCallbacks.Add(opcode); return cursor; }, pointer =>
                    (ushort)(bus.ReadByte(0x8b0000 | pointer) | bus.ReadByte(0x8b0000 | (pointer + 1)) << 8));
                installed.Step(bus, (opcode, cursor) => { generatedCallbacks.Add(opcode); return cursor; },
                    EndingRewardInstructionDefinitions.ReadWord);
                AssertTrue(nativeCallbacks.SequenceEqual(generatedCallbacks),
                    $"reward actor ${start:X4} callback order/timing at frame {frame}");
                AssertEqual(native.InstructionPointer, installed.InstructionPointer,
                    $"reward actor ${start:X4} cursor at frame {frame}");
                AssertEqual(native.SpriteMapPointer, installed.SpriteMapPointer,
                    $"reward actor ${start:X4} visual frame at frame {frame}");
                AssertEqual(native.PreInstructionPointer, installed.PreInstructionPointer,
                    $"reward actor ${start:X4} pre-instruction at frame {frame}");
                AssertEqual(native.IsActive, installed.IsActive,
                    $"reward actor ${start:X4} lifetime at frame {frame}");
            }
        }
    }

    private static void VerifyEndingRewardCalculatedParts(ISnesAddressSpace bus, EndingRewardSpriteFrameDefinition definition)
    {
        SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(bus,
            definition.Pointer, definition.StockPartCount, definition.Name);
        SpriteComposition supplied = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
        SpriteComposition Calculate(SpriteComposition value)
        {
            value = EndingRewardHeadParts.CalculateIfMatching(definition.Pointer, value);
            value = EndingRewardStandingParts.CalculateIfMatching(definition.Pointer, value);
            value = EndingRewardPrepareJumpParts.CalculateIfMatching(definition.Pointer, value);
            value = EndingRewardJumpParts.CalculateIfMatching(definition.Pointer, value);
            value = EndingRewardSuitlessGridParts.CalculateIfMatching(definition.Pointer, value);
            value = EndingRewardSuitlessStandingParts.CalculateIfMatching(definition.Pointer, value);
            return EndingRewardShootingSceneParts.CalculateIfMatching(definition.Pointer, value);
        }
        SpriteComposition calculated = Calculate(supplied);
        AssertTrue(!ReferenceEquals(supplied, calculated), "original reward composition uses calculated layout");
        string Identity(SpriteComposition value) => SelectedPresentationHash.Create("reward-head", value.AppendIdentity);
        AssertEqual(Identity(supplied), Identity(calculated), "original reward composition fields and ordering");
        foreach (ushort y in new ushort[] { 72, 0xfff8 })
        {
            var originalOam = new OamBuffer();
            var calculatedOam = new OamBuffer();
            originalOam.BeginFrame(); calculatedOam.BeginFrame();
            DrawImportedSpritemap(bus, originalOam, 0x8c0000 | definition.Pointer, 120, y, 0x0800, originIsOnScreen: y == 72);
            if (y == 72) calculated.DrawOnScreen(calculatedOam, 120, y, 0x0800);
            else calculated.DrawOffScreen(calculatedOam, 120, y, 0x0800);
            originalOam.FinalizeFrame(); calculatedOam.FinalizeFrame();
            AssertTrue(originalOam.LowTable.SequenceEqual(calculatedOam.LowTable) && originalOam.HighTable.SequenceEqual(calculatedOam.HighTable),
                "reward composition preserves native OAM and clipping");
        }
        SpriteVisualPart first = visual[0];
        foreach (SpriteVisualPart edit in new[]
        {
            first with { OffsetX = first.OffsetX + 1 }, first with { OffsetY = first.OffsetY + 1 },
            first with { TileColumn = (first.TileColumn + 1) % 14 }, first with { TileRow = first.TileRow + 1 },
            first with { Size = first.Size == 16 ? 8 : 16, TileColumn = Math.Min(14, first.TileColumn) },
            first with { Priority = 2 }, first with { Palette = 3 },
            first with { FlipX = !first.FlipX }, first with { FlipY = !first.FlipY },
        })
        {
            visual[0] = edit;
            SpriteComposition edited = IntroCinematicSpriteCompiler.Compile(visual, "edited reward composition");
            AssertTrue(ReferenceEquals(edited, Calculate(edited)), "independent reward composition edits stay supplied");
        }
        foreach (int invalid in new[] { int.MinValue, -1, calculated.PartCount, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = calculated.Part(invalid), "reward composition part bounds");
    }
}
