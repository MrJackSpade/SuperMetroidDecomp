using System.Reflection;
using System.Linq.Expressions;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Builds a probe for the production body-overlap predicate with its bottom coordinate set
    /// to the contour branch's exact vertical threshold.
    /// </summary>
    private static Func<RoomEnemySlot, SamusProjectileSlot, bool> CreateKraidBodyContourProbe()
    {
        Type scratchType = typeof(RoomEnemySystem).GetNestedType(
            "KraidCollisionScratch", BindingFlags.NonPublic)!;
        Type shotType = typeof(RoomEnemySystem).GetNestedType(
            "KraidCollisionShot", BindingFlags.NonPublic)!;
        var body = Expression.Parameter(typeof(RoomEnemySlot), "body");
        var shot = Expression.Parameter(typeof(SamusProjectileSlot), "shot");
        var scratch = Expression.Variable(scratchType, "scratch");
        Expression Field(string name) => Expression.PropertyOrField(shot, name);
        var nativeShot = Expression.New(shotType.GetConstructor(
            Enumerable.Repeat(typeof(ushort), 6).ToArray())!,
            Field(nameof(SamusProjectileSlot.XPosition)), Field(nameof(SamusProjectileSlot.YPosition)),
            Field(nameof(SamusProjectileSlot.XRadius)), Field(nameof(SamusProjectileSlot.YRadius)),
            Field(nameof(SamusProjectileSlot.Type)), Field(nameof(SamusProjectileSlot.Damage)));
        // Enter the body-contour branch at its exact Y threshold. The surrounding
        // mouth/body dispatch is covered by the encounter collision fixtures.
        var bottom = Expression.Convert(Expression.Subtract(Expression.Subtract(
            Expression.Convert(Field(nameof(SamusProjectileSlot.YPosition)), typeof(int)),
            Expression.Convert(Field(nameof(SamusProjectileSlot.YRadius)), typeof(int))),
            Expression.Constant(1)), typeof(ushort));
        return Expression.Lambda<Func<RoomEnemySlot, SamusProjectileSlot, bool>>(
            Expression.Block([scratch],
                Expression.Assign(scratch, Expression.Default(scratchType)),
                Expression.Assign(Expression.Field(scratch, "Bottom"), bottom),
                Expression.Call(scratch, scratchType.GetMethod("OverlapsBody")!, body, nativeShot)),
            body, shot).Compile();
    }
    /// <summary>
    /// Compares the production Kraid body-contour collision edge with the native calculation
    /// across every signed vertical coordinate and representative positions, radii, and edge cases.
    /// </summary>
    /// <param name="rom">Address space used to calculate the native contour boundary.</param>
    private static void VerifyKraidBodyContour(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidBodyContourCases), () => VerifyKraidBodyContourCases(rom));
        var enemies = new RoomEnemySystem();
        var overlaps = CreateKraidBodyContourProbe();
        var body = enemies.Slots[0];
        var shot = new SamusProjectileSystem().Slots[0];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            short y = unchecked((short)raw);
            short expected = NativeKraidBodyLeftEdge(rom, y);
            body.YPosition = (ushort)raw;
            shot.YPosition = unchecked((ushort)(raw + y));
            foreach (ushort bodyX in new ushort[] { 256, 32768, 65520 })
            foreach (ushort radius in new ushort[] { 0, 4, 16 })
            for (int delta = -1; delta <= 1; delta++)
            {
                body.XPosition = bodyX;
                shot.XRadius = radius;
                shot.XPosition = unchecked((ushort)(bodyX + expected - radius + delta));
                bool reference = unchecked((short)(bodyX + expected -
                    unchecked((ushort)(shot.XPosition + radius)))) < 0;
                AssertEqual(reference, overlaps(body, shot), "Actual contour collision edge, translation and strict inequality");
            }
        }
        Console.WriteLine("Kraid body contour: every signed Y and 1769472 actual collision boundary probes pass with ROM reads forbidden.");
    }
}
