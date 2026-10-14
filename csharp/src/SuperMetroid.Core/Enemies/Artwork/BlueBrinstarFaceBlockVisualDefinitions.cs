using SuperMetroid.Core.Enemies.FaceBlocks;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The five distinct bank-$A8 OAM compositions shown by the face block's neutral, Samus-left and Samus-right
/// animations. Activation and frame timing stay in the enemy's typed programs.
/// </summary>
internal static class BlueBrinstarFaceBlockVisualDefinitions
{
    /// <summary>Native bank of the face-block enemy and its OAM maps.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Publishes the neutral pose and the two turning poses each approach direction shows after it.</summary>
    internal static EnemySpritemapDefinition[] Frames()
    {
        ushort[] left = [.. BlueBrinstarFaceBlockAnimations.Spritemaps(BlueBrinstarFaceBlockProgram.SamusLeft)];
        ushort[] right = [.. BlueBrinstarFaceBlockAnimations.Spritemaps(BlueBrinstarFaceBlockProgram.SamusRight)];
        return
        [
            new(Bank, BlueBrinstarFaceBlockAnimations.Spritemaps(BlueBrinstarFaceBlockProgram.Initial).Single(), "face_block_neutral"),
            new(Bank, left[1], "face_block_samus_left_1"),
            new(Bank, left[2], "face_block_samus_left_2"),
            new(Bank, right[1], "face_block_samus_right_1"),
            new(Bank, right[2], "face_block_samus_right_2"),
        ];
    }
}
