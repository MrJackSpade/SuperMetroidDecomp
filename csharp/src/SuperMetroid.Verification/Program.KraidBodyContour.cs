using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static KraidHeadInstructionsVerification;
using static Program;
using System.Reflection;
using System.Linq.Expressions;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static KraidContourCaseMappingsVerification;
using static SuiteTimingVerification;

/// <summary>Represents kraid body contour verification.</summary>
internal static class KraidBodyContourVerification
{
    /// <summary>Creates kraid body contour probe.</summary>
    internal static Func<RoomEnemySlot, SamusProjectileSlot, bool> CreateKraidBodyContourProbe()
    {
        Type scratchType = typeof(KraidBehavior).GetNestedType(
            "KraidCollisionScratch", BindingFlags.NonPublic)!;
        Type shotType = typeof(KraidBehavior).GetNestedType(
            "KraidCollisionShot", BindingFlags.NonPublic)!;
        var body = Expression.Parameter(typeof(RoomEnemySlot), "body");
        var shot = Expression.Parameter(typeof(SamusProjectileSlot), "shot");
        var scratch = Expression.Variable(scratchType, "scratch");
        Expression Field(string path) => path.Split('.').Aggregate((Expression)shot, Expression.PropertyOrField);
        var nativeShot = Expression.New(shotType.GetConstructor(
            [typeof(Position16<RoomPixels>), typeof(Extent16<RoomPixels>), typeof(ushort), typeof(ushort)])!,
            Field("Position.Whole"), Field("Radius"),
            Field(nameof(SamusProjectileSlot.Type)), Field(nameof(SamusProjectileSlot.Damage)));
        // Enter the body-contour branch at its exact Y threshold. The surrounding
        // mouth/body dispatch is covered by the encounter collision fixtures.
        var bottom = Expression.Convert(Expression.Subtract(Expression.Subtract(
            Expression.Convert(Field("Position.Y.Whole"), typeof(int)),
            Expression.Convert(Field("Radius.Y"), typeof(int))),
            Expression.Constant(1)), typeof(ushort));
        return Expression.Lambda<Func<RoomEnemySlot, SamusProjectileSlot, bool>>(
            Expression.Block([scratch],
                Expression.Assign(scratch, Expression.Default(scratchType)),
                Expression.Assign(Expression.Field(scratch, "Bottom"), bottom),
                Expression.Call(scratch, scratchType.GetMethod("OverlapsBody")!, body, nativeShot)),
            body, shot).Compile();
    }
    /// <summary>Verifies kraid body contour.</summary>
    internal static void VerifyKraidBodyContour(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidBodyContourCases), () => VerifyKraidBodyContourCases(rom));
        var enemies = new RoomEnemySystem();
        var overlaps = CreateKraidBodyContourProbe();
        var body = enemies.SlotTable.Slots[0];
        var shot = new SamusProjectileSystem().Slots[0];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            short y = unchecked((short)raw);
            short expected = NativeKraidBodyLeftEdge(rom, y);
            body.Position = body.Position.WithWholeY((ushort)raw);
            shot.Position = shot.Position.WithWholeY(unchecked((ushort)(raw + y)));
            foreach (ushort bodyX in new ushort[] { 256, 32768, 65520 })
            foreach (ushort radius in new ushort[] { 0, 4, 16 })
            for (int delta = -1; delta <= 1; delta++)
            {
                body.Position = body.Position.WithWholeX(bodyX);
                shot.Radius = shot.Radius.WithX(radius);
                shot.Position = shot.Position.WithWholeX(unchecked((ushort)(bodyX + expected - radius + delta)));
                bool reference = unchecked((short)(bodyX + expected -
                    unchecked((ushort)(shot.Position.X.Whole + radius)))) < 0;
                AssertEqual(reference, overlaps(body, shot), "Actual contour collision edge, translation and strict inequality");
            }
        }
        Console.WriteLine("Kraid body contour: every signed Y and 1769472 actual collision boundary probes pass with ROM reads forbidden.");
    }

    /// <summary>Kraid contour head fixtures command.</summary>
    [VerificationCommand("--kraid-contour-head-fixtures")]
    internal static int KraidContourHeadFixturesCommand()
    {
        var kraidFixtureRom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyKraidBodyContour), () => VerifyKraidBodyContour(kraidFixtureRom));
        Suite(nameof(VerifyKraidHeadInstructionDefinitions), () => VerifyKraidHeadInstructionDefinitions(kraidFixtureRom));
        return 0;
    }
}
