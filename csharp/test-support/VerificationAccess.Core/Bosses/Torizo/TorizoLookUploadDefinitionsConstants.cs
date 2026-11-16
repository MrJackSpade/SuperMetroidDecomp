using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Torizo;
using L = SuperMetroid.Core.Bosses.Torizo.TorizoArtworkLook;

namespace SuperMetroid.AssetExtraction.Bosses.Torizo;

/// <summary>Cartridge values of <see cref="TorizoLookUploadDefinitions"/> that only verification reads.</summary>
internal static class TorizoLookUploadDefinitionsConstants
{
    /// <summary>Bank $AA containing the Torizo instruction-controlled VRAM transfer records.</summary>
    public const byte Bank = 0xaa;

    /// <summary>
    /// The cartridge site of each Torizo look's copy, in the bank's order, with the Torizo whose instruction lists make it
    /// and the look it is part of: the cartridge oracle finds each descriptor by its instruction. A look's copies are its
    /// sites in order (#1942).
    /// </summary>
    internal static readonly TransferSite[] Sites =
    [
        new(0xb0f1, EncounterArtwork.BombTorizo, L.GutBlownUp),
        new(0xb0fa, EncounterArtwork.BombTorizo, L.GutBlownUp),
        new(0xb103, EncounterArtwork.BombTorizo, L.GutBlownUp),
        new(0xb10c, EncounterArtwork.BombTorizo, L.GutBlownUp),
        new(0xb161, EncounterArtwork.BombTorizo, L.FaceHalfBlownAway),
        new(0xb16a, EncounterArtwork.BombTorizo, L.FaceHalfBlownAway),
        new(0xb173, EncounterArtwork.BombTorizo, L.FaceHalfBlownAway),
        new(0xb17c, EncounterArtwork.BombTorizo, L.FaceHalfBlownAway),
        new(0xb189, EncounterArtwork.BombTorizo, L.FaceBlownUp),
        new(0xb192, EncounterArtwork.BombTorizo, L.FaceBlownUp),
        new(0xb19b, EncounterArtwork.BombTorizo, L.FaceBlownUp),
        new(0xb1a4, EncounterArtwork.BombTorizo, L.FaceBlownUp),
        new(0xb1ad, EncounterArtwork.BombTorizo, L.FaceBlownUp),
        new(0xb88f, EncounterArtwork.BombTorizo, L.EyesDark),
        new(0xb89c, EncounterArtwork.BombTorizo, L.EyesGlowingFaintly),
        new(0xb8a9, EncounterArtwork.BombTorizo, L.EyesGlowingBrightly),
        new(0xb8b6, EncounterArtwork.BombTorizo, L.EyesGlowingFully),
        new(0xb8cb, EncounterArtwork.BombTorizo, L.EyesDarkAgain),
        new(0xb8d8, EncounterArtwork.BombTorizo, L.EyesGlowingFaintlyAgain),
        new(0xb8e5, EncounterArtwork.BombTorizo, L.EyesGlowingBrightlyAgain),
        new(0xb8f2, EncounterArtwork.BombTorizo, L.EyesGlowingFullyAgain),
        new(0xc9cb, EncounterArtwork.GoldenTorizo, L.Awakening),
        new(0xca10, EncounterArtwork.GoldenTorizo, L.EyesDark),
        new(0xca1d, EncounterArtwork.GoldenTorizo, L.EyesGlowingFaintly),
        new(0xca2a, EncounterArtwork.GoldenTorizo, L.EyesGlowingBrightly),
        new(0xca37, EncounterArtwork.GoldenTorizo, L.EyesGlowingFully),
        new(0xca4c, EncounterArtwork.GoldenTorizo, L.EyesDarkAgain),
        new(0xca59, EncounterArtwork.GoldenTorizo, L.EyesGlowingFaintlyAgain),
        new(0xca66, EncounterArtwork.GoldenTorizo, L.EyesGlowingBrightlyAgain),
        new(0xca73, EncounterArtwork.GoldenTorizo, L.EyesGlowingFullyAgain),
        new(0xd035, EncounterArtwork.GoldenTorizo, L.ChestParting),
        new(0xd03e, EncounterArtwork.GoldenTorizo, L.ChestParting),
        new(0xd04b, EncounterArtwork.GoldenTorizo, L.ChestHalfOpen),
        new(0xd054, EncounterArtwork.GoldenTorizo, L.ChestHalfOpen),
        new(0xd061, EncounterArtwork.GoldenTorizo, L.ChestOpen),
        new(0xd06a, EncounterArtwork.GoldenTorizo, L.ChestOpen),
        new(0xd08f, EncounterArtwork.GoldenTorizo, L.ChestHeldOpen),
        new(0xd098, EncounterArtwork.GoldenTorizo, L.ChestHeldOpen),
        new(0xd0a5, EncounterArtwork.GoldenTorizo, L.ChestHalfClosed),
        new(0xd0ae, EncounterArtwork.GoldenTorizo, L.ChestHalfClosed),
        new(0xd0bb, EncounterArtwork.GoldenTorizo, L.ChestNearlyClosed),
        new(0xd0c4, EncounterArtwork.GoldenTorizo, L.ChestNearlyClosed),
        new(0xd0d1, EncounterArtwork.GoldenTorizo, L.ChestClosed),
        new(0xd0da, EncounterArtwork.GoldenTorizo, L.ChestClosed),
        new(0xd137, EncounterArtwork.GoldenTorizo, L.EyeBeamEyesDark),
        new(0xd144, EncounterArtwork.GoldenTorizo, L.EyeBeamEyesGlowingFaintly),
        new(0xd151, EncounterArtwork.GoldenTorizo, L.EyeBeamEyesGlowingBrightly),
        new(0xd15e, EncounterArtwork.GoldenTorizo, L.EyeBeamEyesGlowingFully),
        new(0xd1a5, EncounterArtwork.GoldenTorizo, L.StunnedEyesDark),
        new(0xd1b2, EncounterArtwork.GoldenTorizo, L.StunnedEyesGlowingFaintly),
        new(0xd1bf, EncounterArtwork.GoldenTorizo, L.StunnedEyesGlowingBrightly),
        new(0xd1cc, EncounterArtwork.GoldenTorizo, L.StunnedEyesGlowingFully),
    ];

    /// <summary>Where one look's copy sits in the cartridge: its <c>$814B</c> instruction in bank $AA.</summary>
    /// <param name="Instruction">The bank-$AA address of the copy's <c>$814B</c> instruction.</param>
    /// <param name="Torizo">The Torizo whose instruction lists make the copy.</param>
    /// <param name="Look">The look the copy is part of.</param>
    internal readonly record struct TransferSite(ushort Instruction, EncounterArtwork Torizo, TorizoArtworkLook Look);

    /// <summary>Provides the bank and sites of Torizo instruction VRAM transfer records.</summary>
    extension(TorizoLookUploadDefinitions)
    {
        /// <inheritdoc cref="TorizoLookUploadDefinitionsConstants.Bank"/>
        internal static byte Bank => TorizoLookUploadDefinitionsConstants.Bank;
        /// <inheritdoc cref="TorizoLookUploadDefinitionsConstants.Sites"/>
        internal static ReadOnlySpan<TorizoLookUploadDefinitionsConstants.TransferSite> Sites => TorizoLookUploadDefinitionsConstants.Sites;
    }
}
