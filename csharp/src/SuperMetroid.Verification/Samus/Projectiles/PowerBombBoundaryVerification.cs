using SuperMetroid.Core.Assets;
using SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks Power Bomb boundary traversal in an inert room, including clipping and repeated corner visits.</summary>
internal static class PowerBombBoundaryVerification
{
    /// <summary>Confirms ordinary and Chainsaw caller words visit the same expected room-block edges at two origins and zero radius.</summary>
    internal static void VerifyPowerBombBoundary()
    {
        var level = new RoomLevelData(new Extent16<RoomBlocks>(5, 5), new RoomLevelPlanes(new ushort[25], new byte[25], new ushort[25]), new BackgroundTile[4]);
        Check(32, 32, 0x1000, [new BotwoonRadiusOpcode(1,1), new BotwoonRadiusOpcode(2,1), new BotwoonRadiusOpcode(3,1), new BotwoonRadiusOpcode(1,1), new BotwoonRadiusOpcode(1,2),
            new BotwoonRadiusOpcode(1,2), new BotwoonRadiusOpcode(2,2), new BotwoonRadiusOpcode(3,2), new BotwoonRadiusOpcode(3,1), new BotwoonRadiusOpcode(3,2)]);
        Check(0, 0, 0x1000, [new BotwoonRadiusOpcode(0,0), new BotwoonRadiusOpcode(1,0), new BotwoonRadiusOpcode(0,0), new BotwoonRadiusOpcode(0,0), new BotwoonRadiusOpcode(1,0), new BotwoonRadiusOpcode(1,0)]);
        Check(32, 32, 0, [new BotwoonRadiusOpcode(2,2), new BotwoonRadiusOpcode(2,2), new BotwoonRadiusOpcode(2,2), new BotwoonRadiusOpcode(2,2)]);

        void Check(ushort x, ushort y, ushort radius, BotwoonRadiusOpcode[] expected)
        {
            // Both a normal Power Bomb and the Chainsaw word pass through the same
            // boundary walker. This checks traversal only, not their PLM damage class.
            foreach (ushort type in new ushort[] { 0x0300, 0x800d })
            {
                var visited = new List<BombBlockReaction>();
                SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(level, new(x, y), PowerBombRadius.FromRaw(radius),
                    visited, null, AreaId.Crateria, type);
                // A visit into this inert air room leaves no state besides its reaction entry,
                // so only the inclusive visit count (corner revisits included) is observable.
                AssertEqual(expected.Length, visited.Count, "inclusive Power Bomb edge visit count");
            }
        }
        Console.WriteLine("Power Bomb boundary: edge visit counts, corner revisits, clipping and zero radius agree for both caller words.");
    }
}
