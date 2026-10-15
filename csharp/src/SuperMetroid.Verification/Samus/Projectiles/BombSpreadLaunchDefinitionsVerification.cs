using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks compiled Bomb Spread launch words and actual slot initialization against retail source data.</summary>
internal static class BombSpreadLaunchDefinitionsVerification
{
    /// <summary>Compares five native fuse, X, Y, and subvelocity records and verifies hold-modified launches without reading migrated ROM tables.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Bomb spread launch definitions")]
    internal static void VerifyBombSpreadLaunchDefinitions(SuperMetroidAddressSpace rom)
    {
        var reference = Enumerable.Range(0,SamusBombSpreadRomData.SlotCount).Select(i =>
            new BombSpreadLaunchDefinition(CartridgeImportSource.Require(rom).ReadCartridgeWord((SamusBombSpreadRomData.FuseTimers+i*2)),
                CartridgeImportSource.Require(rom).ReadCartridgeWord((SamusBombSpreadRomData.XVelocities+i*2)), CartridgeImportSource.Require(rom).ReadCartridgeWord((SamusBombSpreadRomData.YSpeeds+i*2)),
                CartridgeImportSource.Require(rom).ReadCartridgeWord((SamusBombSpreadRomData.YSubspeeds+i*2)))).ToArray();
        for(int i=0;i<reference.Length;i++)
            AssertEqual(reference[i],SamusBombSpreadLaunchDefinitions.ForSlot(i),"All native Bomb Spread launch words");
        Action<SamusBombProjectileSystem,SamusState> spawn=(system,player)=>system.SpawnBombSpread(player);
        var guarded=NewBombSpreadLaunchReadGuard(rom);
        var bombs=new SamusBombProjectileSystem();
        var samus=new SamusState();
        for(int hold=0;hold<=ushort.MaxValue;hold++)
        {
            samus.BombSpreadChargeTimeoutCounter=(ushort)hold;
            samus.ProjectileFlareCounter=SamusBombSpreadRomData.RequiredChargeFrames;
            samus.Position = samus.Position.WithWhole(new((ushort)hold, unchecked((ushort)~hold)));
            spawn(bombs,samus);
            for(int i=0;i<reference.Length;i++)
            {
                var slot=bombs.Slots[i]; var expected=reference[i];
                ushort y=unchecked((ushort)-(expected.YSpeed+((hold>>6)&3)));
                AssertEqual(expected.FuseTimer,slot.BombTimer.RawValue,"Spread fuse by physical slot");
                AssertEqual(expected.XVelocity,slot.BombSpreadDirectionMagnitude,"Spread native direction/magnitude word");
                AssertEqual(expected.YSubspeed,slot.BombSpreadInitialYSubvelocity,"Spread initial subvelocity");
                AssertEqual(expected.YSubspeed,slot.BombSpreadYSubvelocity,"Spread live subvelocity");
                AssertEqual(y,slot.BombSpreadYVelocity,"Spread hold modifier and negated whole velocity");
                AssertEqual(y,slot.BombSpreadBounceYVelocity,"Spread bounce retains launch velocity");
                AssertEqual(samus.Position.X.Whole,slot.Position.X.Whole,"Spread launch X wraps with Samus");
                AssertEqual(samus.Position.Y.Whole,slot.Position.Y.Whole,"Spread launch Y wraps with Samus");
            }
            AssertEqual(SamusBombSpreadRomData.SlotCount,bombs.BombCounter,"Spread allocates all five slots");
            AssertEqual((ushort)0,samus.ProjectileFlareCounter,"Spread consumes charge");
            AssertEqual((ushort)0,samus.BombSpreadChargeTimeoutCounter,"Spread clears hold counter");
        }
        Console.WriteLine("Bomb Spread launch: twenty native words and 327680 actual slot initializations match with launch ROM reads forbidden.");
    }

    /// <summary>Forbids production reads across the migrated Bomb Spread launch tables.</summary>
    private static CartridgeReadGuard NewBombSpreadLaunchReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
                address >= SamusBombSpreadRomData.FuseTimers &&
                address < SamusBombSpreadRomData.YSubspeeds + SamusBombSpreadRomData.SlotCount * 2,
            _ => new InvalidDataException("Bomb Spread still read its compiled launch data from ROM."));
}
