using static SuperMetroid.Verification.Enemies.SpacePirates.NinjaSpacePirateNativeListAddresses;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>#641: the ninja Space Pirates' typed programs, each matched step by step to its native bank-$B2 list.</summary>
internal static class NinjaSpacePirateAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyNinjaSpacePirateAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, NinjaSpacePirateAnimations.Programs,
            NinjaSpacePirateFrameDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'V' => cue.Kind switch
                    {
                        NinjaSpacePirateCueKind.PlaySound => (cue.Sound ?? throw new InvalidOperationException($"{at}: a sound cue carries no sound.")).Value,
                        NinjaSpacePirateCueKind.SetPalette => (cue.Palette ?? throw new InvalidOperationException($"{at}: a palette cue carries no palette.")).PaletteBits,
                        NinjaSpacePirateCueKind.ThrowClaw => cue.ThrowsRight ? (ushort)1 : (ushort)0,
                        NinjaSpacePirateCueKind.ResetSpeed or
                            NinjaSpacePirateCueKind.SetFunction or
                            NinjaSpacePirateCueKind.StartActive or
                            NinjaSpacePirateCueKind.StartLeftDivekick or
                            NinjaSpacePirateCueKind.StartRightDivekick => throw new InvalidOperationException($"{at}: {cue.Kind} has no word operand."),
                        _ => throw new InvalidOperationException($"{at}: {cue.Kind} has no word operand."),
                    },
                    'S' => unchecked((ushort)cue.Offset),
                    'Y' => unchecked((ushort)cue.OffsetY),
                    'E' => (ushort)cue.Function,
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            });
        Console.WriteLine($"  Ninja Space Pirate programs: {Entries.Length} programs ({steps} steps) match bank $B2.");
    }
}
