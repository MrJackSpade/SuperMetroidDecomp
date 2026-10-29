using System.Buffers.Binary;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// The body layer a boss's fight attaches before its body frames draw (#1406, #1466), for checks that draw those frames
/// alone and compare the words they show with the cartridge's VRAM.
/// </summary>
internal static class BossBodyCanvasFixtures
{
    /// <summary>Binds <paramref name="sheets"/> to <paramref name="enemies"/> and attaches <paramref name="owner"/>'s cleared body layer.</summary>
    internal static BossBodyCanvas AttachBlank(RoomEnemySystem enemies, BossBodySheets sheets, BossBodyOwner owner)
    {
        enemies.BossArtworkBinding.BodySheets = sheets;
        var canvas = new BossBodyCanvas(sheets.Of(owner), BossBodyDefinitions.SharedBodyRows);
        enemies.Presentation.AttachBossBody(canvas);
        return canvas;
    }

    /// <summary>
    /// Binds <paramref name="sheets"/> to <paramref name="enemies"/> and gives Crocomire what his initialization does: a
    /// death state with a cleared working canvas, and a cleared shown body layer. Returns the death state.
    /// </summary>
    internal static CrocomireDeathState AttachCrocomire(RoomEnemySystem enemies, BossBodySheets sheets)
    {
        enemies.BossArtworkBinding.BodySheets = sheets;
        BossBodySheet sheet = sheets.Of(BossBodyOwner.Crocomire);
        var death = new CrocomireDeathState { WorkingBody = new BossBodyCanvas(sheet, BossBodyDefinitions.SharedBodyRows) };
        enemies.CrocomireBehavior.Access.CrocomireDeath = death;
        enemies.Presentation.AttachBossBody(new BossBodyCanvas(sheet, BossBodyDefinitions.SharedBodyRows));
        return death;
    }

    /// <summary>Whether <paramref name="first"/> and <paramref name="second"/> show the same parts over the same cells.</summary>
    internal static bool ShowSame(BossBodyCanvas first, BossBodyCanvas second, BossBodySheets sheets) =>
        Describe(first.Publish(sheets)) == Describe(second.Publish(sheets));

    /// <summary>Each part <paramref name="layer"/> shows, with the first row and row masks of the cells it shows.</summary>
    private static string Describe(BossBodyLayer layer) => string.Join(";", layer.Placements.Select(placement =>
        placement.Part.Name + " " + placement.Shows.Top + " " + string.Join(",", placement.Shows.Rows)));

    /// <summary>
    /// A VRAM-sized image of the words <paramref name="canvas"/> shows, laid out from entry <paramref name="firstWord"/>,
    /// where the cartridge's VRAM holds them, with every cell still showing the blank left zero, as fresh VRAM's are.
    /// </summary>
    internal static byte[] AsVram(BossBodyCanvas canvas, RoomEnemySystem enemies, int firstWord, int vramByteCount)
    {
        BossBodySheets sheets = enemies.BossArtworkBinding.BodySheets ?? throw new InvalidOperationException("No body sheets are bound.");
        ushort[] words = BossBodyWords.Of(canvas.Publish(sheets), canvas.Owner,
            enemies.BossArtwork ?? throw new InvalidOperationException("No boss artwork is bound."), blankWord: 0);
        byte[] shown = new byte[vramByteCount];
        for (int word = 0; word < words.Length; word++)
            BinaryPrimitives.WriteUInt16LittleEndian(shown.AsSpan((firstWord + word) * sizeof(ushort)), words[word]);
        return shown;
    }
}
