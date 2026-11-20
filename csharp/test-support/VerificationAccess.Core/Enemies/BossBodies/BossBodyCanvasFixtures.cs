using SuperMetroid.AssetExtraction.Enemies;
using System.Buffers.Binary;
using System.Collections.Immutable;
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
    /// Binds <paramref name="sheets"/> to <paramref name="enemies"/> and gives Crocomire the body his initialization
    /// attaches, showing nothing; his death state is his encounter's own.
    /// </summary>
    internal static void AttachCrocomire(RoomEnemySystem enemies, BossBodySheets sheets)
    {
        enemies.BossArtworkBinding.BodySheets = sheets;
        enemies.Presentation.AttachBossBody(new BossBodyParts(BossBodyOwner.Crocomire));
    }

    /// <summary>
    /// Attaches the body that <paramref name="part"/>'s boss attaches at its initialization, before any of its parts shows a
    /// frame, with no part showing anything on it yet. A place that is no part a boss draws on its body, as any ordinary
    /// enemy or Draygon's tail, attaches none. Its boss's body sheets are those already bound to <paramref name="enemies"/>.
    /// </summary>
    internal static void AttachBodyOfPart(RoomEnemySystem enemies, RoomEnemyPlace part)
    {
        if (part.Entity is not IBossBodyFrames)
            return;
        foreach ((BossBodyOwner owner, ImmutableArray<EnemyDefinitionId> parts) in BossBodyPartDefinitions.DrawOrders)
        {
            if (parts.Contains(part.DefinitionId))
            {
                enemies.Presentation.AttachBossBody(new BossBodyParts(owner));
                return;
            }
        }
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
    internal static byte[] AsVram(BossBodyParts body, BossBodyOwner owner, BossSourceArtwork bosses, int firstWord, int vramByteCount)
    {
        ushort[] words = BossBodyWords.Of(body.Publish(), owner, bosses, blankWord: 0);
        byte[] shown = new byte[vramByteCount];
        for (int word = 0; word < words.Length; word++)
            BinaryPrimitives.WriteUInt16LittleEndian(shown.AsSpan((firstWord + word) * sizeof(ushort)), words[word]);
        return shown;
    }
}
