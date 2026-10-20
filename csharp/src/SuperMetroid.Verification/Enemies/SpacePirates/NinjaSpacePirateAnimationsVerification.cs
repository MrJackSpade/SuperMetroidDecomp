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
                instruction.Check(cue, at);
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'V' => cue switch
                    {
                        NinjaSpacePirateCue.PlaySound playSound => (playSound.Sound ?? throw new InvalidOperationException($"{at}: a sound cue carries no sound.")).Value,
                        NinjaSpacePirateCue.SetPalette setPalette => (setPalette.Palette ?? throw new InvalidOperationException($"{at}: a palette cue carries no palette.")).PaletteBits,
                        NinjaSpacePirateCue.ThrowClaw throwClaw => throwClaw.Throw == HorizontalDirection.Right ? (ushort)1 : (ushort)0,
                        NinjaSpacePirateCue.Command { Kind: NinjaSpacePirateCueKind.ResetSpeed } or NinjaSpacePirateCue.SetFunction or NinjaSpacePirateCue.Command { Kind: NinjaSpacePirateCueKind.StartActive } or NinjaSpacePirateCue.Command { Kind: NinjaSpacePirateCueKind.StartLeftDivekick } or NinjaSpacePirateCue.Command { Kind: NinjaSpacePirateCueKind.StartRightDivekick } => throw new InvalidOperationException($"{at}: {cue} has no word operand."),
                        _ => throw new InvalidOperationException($"{at}: {cue} has no word operand."),
                    },
                    'S' => unchecked((ushort)AssertIs<NinjaSpacePirateCue.ThrowClaw>(cue, $"{at}: cue").Offset),
                    'Y' => unchecked((ushort)AssertIs<NinjaSpacePirateCue.ThrowClaw>(cue, $"{at}: cue").OffsetY),
                    'E' => (ushort)AssertIs<NinjaSpacePirateCue.SetFunction>(cue, $"{at}: cue").Function,
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            });
        Console.WriteLine($"  Ninja Space Pirate programs: {Entries.Length} programs ({steps} steps) match bank $B2.");
    }
}
