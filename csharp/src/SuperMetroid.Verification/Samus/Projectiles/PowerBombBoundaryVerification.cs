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
        var level = new RoomLevelData(new Extent16<RoomBlocks>(5, 5), new RoomLevelPlanes(new RoomTerrainBlock[25], new RoomBlockBehavior[25], new RoomBlockAppearance[25]), RoomLevelData.NothingBeyondGrid(), []);
        Check(32, 32, 16, [new BotwoonRadiusOpcode(1,1), new BotwoonRadiusOpcode(2,1), new BotwoonRadiusOpcode(3,1), new BotwoonRadiusOpcode(1,1), new BotwoonRadiusOpcode(1,2),
            new BotwoonRadiusOpcode(1,2), new BotwoonRadiusOpcode(2,2), new BotwoonRadiusOpcode(3,2), new BotwoonRadiusOpcode(3,1), new BotwoonRadiusOpcode(3,2)]);
        Check(0, 0, 16, [new BotwoonRadiusOpcode(0,0), new BotwoonRadiusOpcode(1,0), new BotwoonRadiusOpcode(0,0), new BotwoonRadiusOpcode(0,0), new BotwoonRadiusOpcode(1,0), new BotwoonRadiusOpcode(1,0)]);
        Check(32, 32, 0, [new BotwoonRadiusOpcode(2,2), new BotwoonRadiusOpcode(2,2), new BotwoonRadiusOpcode(2,2), new BotwoonRadiusOpcode(2,2)]);
        // #1920: a blast both left of and above the room, whose far edges pass 65535, has each far edge held to the
        // room's last block, which leaves every edge before its near one: the blast reaches nothing.
        foreach (ushort type in new ushort[] { 0x0300, 0x800d })
        {
            var cornerVisits = new List<BombBlockReaction>();
            SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(level, new(65530, 65530), PowerBombRadius.FromWholePixels(16),
                cornerVisits, null, SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(type));
            AssertEqual(0, cornerVisits.Count, "a blast left of and above the room reaches no block");
        }

        void Check(ushort x, ushort y, int radiusPixels, BotwoonRadiusOpcode[] expected)
        {
            // Both a normal Power Bomb and the Chainsaw word pass through the same
            // boundary walker. This checks traversal only, not their PLM damage class.
            foreach (ushort type in new ushort[] { 0x0300, 0x800d })
            {
                var visited = new List<BombBlockReaction>();
                SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(level, new(x, y), PowerBombRadius.FromWholePixels(radiusPixels),
                    visited, null, SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(type));
                // A visit into this inert air room leaves no state besides its reaction entry,
                // so only the inclusive visit count (corner revisits included) is observable.
                AssertEqual(expected.Length, visited.Count, "inclusive Power Bomb edge visit count");
            }
        }
        Console.WriteLine("Power Bomb boundary: edge visit counts, corner revisits, clipping and zero radius agree for both caller words.");
    }
}
