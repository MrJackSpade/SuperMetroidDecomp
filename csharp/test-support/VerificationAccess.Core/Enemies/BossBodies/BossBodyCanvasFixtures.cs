using System.Buffers.Binary;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// The body a boss's fight attaches before its body frames show (#1406, #1466, #1515), for checks that show those frames
/// alone and compare the words they show with the cartridge's VRAM.
/// </summary>
internal static class BossBodyCanvasFixtures
{
    /// <summary>
    /// Binds <paramref name="sheets"/> to <paramref name="enemies"/> and attaches a body of <paramref name="owner"/>'s no part
    /// shows anything on yet.
    /// </summary>
    internal static BossBodyParts AttachBlank(RoomEnemySystem enemies, BossBodySheets sheets, BossBodyOwner owner)
    {
        enemies.BossArtworkBinding.BodySheets = sheets;
        var body = new BossBodyParts(owner);
        enemies.Presentation.AttachBossBody(body);
        return body;
    }

    /// <summary>
    /// Binds <paramref name="sheets"/> to <paramref name="enemies"/> and gives Crocomire what his initialization does: a
    /// death state and a body showing nothing. Returns the death state.
    /// </summary>
    internal static CrocomireDeathState AttachCrocomire(RoomEnemySystem enemies, BossBodySheets sheets)
    {
        enemies.BossArtworkBinding.BodySheets = sheets;
        var death = new CrocomireDeathState();
        enemies.CrocomireBehavior.Access.CrocomireDeath = death;
        enemies.Presentation.AttachBossBody(new BossBodyParts(BossBodyOwner.Crocomire));
        return death;
    }

    /// <summary>Whether <paramref name="first"/> and <paramref name="second"/> show the same frames, shown above the same row.</summary>
    internal static bool ShowSame(BossBodyParts first, BossBodyParts second) => Describe(first.Publish()) == Describe(second.Publish());

    /// <summary>Each frame <paramref name="layer"/> shows, beneath first, and the row above which alone it shows.</summary>
    private static string Describe(BossBodyLayer layer) =>
        string.Join(";", layer.Frames.Select(frame => frame.Name)) + " shown above " + (layer.ShownAbove?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "every row");

    /// <summary>
    /// A VRAM-sized image of the words <paramref name="body"/> of <paramref name="owner"/> shows, laid out from entry
    /// <paramref name="firstWord"/>, where the cartridge's VRAM holds them, with every cell no frame shows left zero, as fresh
    /// VRAM's are.
    /// </summary>
    internal static byte[] AsVram(BossBodyParts body, BossBodyOwner owner, RoomEnemySystem enemies, int firstWord, int vramByteCount)
    {
        ushort[] words = BossBodyWords.Of(body.Publish(), owner,
            enemies.BossArtwork ?? throw new InvalidOperationException("No boss artwork is bound."), blankWord: 0);
        byte[] shown = new byte[vramByteCount];
        for (int word = 0; word < words.Length; word++)
            BinaryPrimitives.WriteUInt16LittleEndian(shown.AsSpan((firstWord + word) * sizeof(ushort)), words[word]);
        return shown;
    }
}
