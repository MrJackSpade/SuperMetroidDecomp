using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares every compiled Bomb Spread launch slot against cartridge data while forbidding runtime reads of that data.</summary>
    /// <param name="rom">Cartridge address space used to capture the reference launch words before guarded spawning.</param>
    private static void VerifyBombSpreadLaunchDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int a) => (ushort)(rom.ReadByte(a) | rom.ReadByte(a+1)<<8);
        var reference = Enumerable.Range(0,SamusBombSpreadRomData.SlotCount).Select(i =>
            new BombSpreadLaunchDefinition(Word(SamusBombSpreadRomData.FuseTimers+i*2),
                Word(SamusBombSpreadRomData.XVelocities+i*2), Word(SamusBombSpreadRomData.YSpeeds+i*2),
                Word(SamusBombSpreadRomData.YSubspeeds+i*2))).ToArray();
        for(int i=0;i<reference.Length;i++)
            AssertEqual(reference[i],SamusBombSpreadLaunchDefinitions.ForSlot(i),"All native Bomb Spread launch words");
        var spawn=typeof(SamusBombProjectileSystem).GetMethod("SpawnBombSpread",BindingFlags.Instance|BindingFlags.NonPublic)!
            .CreateDelegate<Action<SamusBombProjectileSystem,ISnesAddressSpace,SamusState>>();
        var guarded=new BombSpreadLaunchReadGuard(rom);
        var bombs=new SamusBombProjectileSystem();
        var samus=new SamusState();
        for(int hold=0;hold<=ushort.MaxValue;hold++)
        {
            samus.BombSpreadChargeTimeoutCounter=(ushort)hold;
            samus.ProjectileFlareCounter=SamusBombSpreadRomData.RequiredChargeFrames;
            samus.XPosition=(ushort)hold; samus.YPosition=unchecked((ushort)~hold);
            spawn(bombs,guarded,samus);
            for(int i=0;i<reference.Length;i++)
            {
                var slot=bombs.Slots[i]; var expected=reference[i];
                ushort y=unchecked((ushort)-(expected.YSpeed+((hold>>6)&3)));
                AssertEqual(expected.FuseTimer,slot.BombTimer,"Spread fuse by physical slot");
                AssertEqual(expected.XVelocity,slot.BombSpreadXVelocity,"Spread native direction/magnitude word");
                AssertEqual(expected.YSubspeed,slot.BombSpreadInitialYSubvelocity,"Spread initial subvelocity");
                AssertEqual(expected.YSubspeed,slot.BombSpreadYSubvelocity,"Spread live subvelocity");
                AssertEqual(y,slot.BombSpreadYVelocity,"Spread hold modifier and negated whole velocity");
                AssertEqual(y,slot.BombSpreadBounceYVelocity,"Spread bounce retains launch velocity");
                AssertEqual(samus.XPosition,slot.XPosition,"Spread launch X wraps with Samus");
                AssertEqual(samus.YPosition,slot.YPosition,"Spread launch Y wraps with Samus");
            }
            AssertEqual(SamusBombSpreadRomData.SlotCount,bombs.BombCounter,"Spread allocates all five slots");
            AssertEqual((ushort)0,samus.ProjectileFlareCounter,"Spread consumes charge");
            AssertEqual((ushort)0,samus.BombSpreadChargeTimeoutCounter,"Spread clears hold counter");
        }
        Console.WriteLine("Bomb Spread launch: twenty native words and 327680 actual slot initializations match with launch ROM reads forbidden.");
    }

    /// <summary>Address-space adapter that rejects reads of the Bomb Spread launch table during spawning.</summary>
    /// <param name="source">Underlying address space that serves allowed reads and receives writes.</param>
    private sealed class BombSpreadLaunchReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-byte access through the guard so launch-table reads are rejected.</summary>
        /// <param name="address">Cartridge address requested by the caller.</param>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects access to the compiled launch-data range and forwards other reads to the source.</summary>
        /// <param name="address">Address to read from the guarded address space.</param>
        /// <returns>The byte supplied by the wrapped address space when the address is permitted.</returns>
        public byte ReadByte(int address)
        {
            if(address>=SamusBombSpreadRomData.FuseTimers && address<SamusBombSpreadRomData.YSubspeeds+SamusBombSpreadRomData.SlotCount*2)
                throw new InvalidDataException("Bomb Spread still read its compiled launch data from ROM.");
            return source.ReadByte(address);
        }
        /// <summary>Forwards writes unchanged to the wrapped address space.</summary>
        /// <param name="address">Address that receives the write.</param>
        /// <param name="value">Byte written to the address.</param>
        public void WriteByte(int address,byte value) => source.WriteByte(address,value);
    }
}
