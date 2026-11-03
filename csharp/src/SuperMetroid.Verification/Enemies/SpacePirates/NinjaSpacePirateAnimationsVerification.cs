using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.SpacePirates.NinjaSpacePirateNativeListAddresses;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Audio;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>#641: the ninja Space Pirates' typed programs, each matched step by step to its native bank-$B2 list.</summary>
internal static class NinjaSpacePirateAnimationsVerification
{
    /// <summary>The row the native stance cue names: the one the Metal Pirates room loads the gold Ninja Pirates' colours into.</summary>
    private const int NinjaStanceRow = 1;

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyNinjaSpacePirateAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, NinjaSpacePirateAnimations.Programs,
            NinjaSpacePirateFrameToolingDefinitions.Spritemap,
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
                        NinjaSpacePirateCue.PlaySound playSound => (ushort)SoundEffectCommands.Command(playSound.Sound ?? throw new InvalidOperationException($"{at}: a sound cue carries no sound.")),
                        // A look cue's operand is the palette word of the row the cartridge selects: the gold wall row for the
                        // gold wall flash, the row the Metal Pirates room loads the gold Ninja Pirates into for the stance (#1512).
                        NinjaSpacePirateCue.SetShading setShading => SuperMetroid.Core.Assets.NativeSpriteGraphicsWords.PaletteBits(
                            setShading.Shading == SuperMetroid.Core.Enemies.SpacePirates.Artwork.NinjaSpacePirateShades.GoldWallFlash
                                ? SuperMetroid.AssetExtraction.Enemies.EnemySheetRowDefinitions.NinjaGoldWall : NinjaStanceRow),
                        NinjaSpacePirateCue.ThrowClaw throwClaw => throwClaw.Throw == HorizontalDirection.Right ? (ushort)1 : (ushort)0,
                        NinjaSpacePirateCue.Command { Kind: NinjaSpacePirateCueKind.ResetSpeed } or NinjaSpacePirateCue.SetFunction or NinjaSpacePirateCue.Command { Kind: NinjaSpacePirateCueKind.StartActive } or NinjaSpacePirateCue.Command { Kind: NinjaSpacePirateCueKind.StartLeftDivekick } or NinjaSpacePirateCue.Command { Kind: NinjaSpacePirateCueKind.StartRightDivekick } => throw new InvalidOperationException($"{at}: {cue} has no word operand."),
                        _ => throw new InvalidOperationException($"{at}: {cue} has no word operand."),
                    },
                    'S' => unchecked((ushort)AssertIs<NinjaSpacePirateCue.ThrowClaw>(cue, $"{at}: cue").Offset.X),
                    'Y' => unchecked((ushort)AssertIs<NinjaSpacePirateCue.ThrowClaw>(cue, $"{at}: cue").Offset.Y),
                    'E' => SpacePiratesNativeWords.NinjaSpacePirateFunctions.Word(AssertIs<NinjaSpacePirateCue.SetFunction>(cue, $"{at}: cue").Function),
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            });
        Console.WriteLine($"  Ninja Space Pirate programs: {Entries.Length} programs ({steps} steps) match bank $B2.");
    }
}
