using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks compiled Bomb Spread launch words and actual slot initialization against retail source data.</summary>
internal static class BombSpreadLaunchDefinitionsVerification
{
    /// <summary>Compares five native fuse, X, Y, and subvelocity records and verifies hold-modified launches without reading migrated ROM tables.</summary>
    internal static void VerifyBombSpreadLaunchDefinitions(SuperMetroidAddressSpace rom)
    {
        var reference = Enumerable.Range(0,MorphBallBombDefinitions.SlotCount).Select(i =>
            new BombSpreadLaunchDefinition(CartridgeImportSource.Require(rom).ReadCartridgeWord((MorphBallBombDefinitions.FuseTimers+i*2)),
                BombSpreadHorizontalVelocity.FromLaunchWord(CartridgeImportSource.Require(rom).ReadCartridgeWord((MorphBallBombDefinitions.XVelocities+i*2))), new UnsignedSixteenSixteen(CartridgeImportSource.Require(rom).ReadCartridgeWord((MorphBallBombDefinitions.YSpeeds+i*2)),
                CartridgeImportSource.Require(rom).ReadCartridgeWord((MorphBallBombDefinitions.YSubspeeds+i*2))))).ToArray();
        for(int i=0;i<reference.Length;i++)
            AssertEqual(reference[i],SamusBombSpreadLaunches.ForSlot(i),"All native Bomb Spread launch words");
        Action<SamusBombProjectileSystem,SamusState> spawn=(system,player)=>system.SpawnBombSpread(player);
        var guarded=NewBombSpreadLaunchReadGuard(rom);
        var bombs=new SamusBombProjectileSystem();
        var samus=new SamusState(InstalledFixtures.AnimationContinuation);
        for(int hold=0;hold<=ushort.MaxValue;hold++)
        {
            samus.BombSpreadChargeTimeoutCounter=(ushort)hold;
            samus.ProjectileFlareCounter=MorphBallBombDefinitions.RequiredChargeFrames;
            samus.Position = samus.Position.WithWhole(new((ushort)hold, unchecked((ushort)~hold)));
            spawn(bombs,samus);
            for(int i=0;i<reference.Length;i++)
            {
                var slot=bombs.Slots[i]; var expected=reference[i];
                ushort y=unchecked((ushort)-(expected.YSpeed.Whole+((hold>>6)&3)));
                AssertEqual(expected.FuseTimer,slot.BombTimer.RawValue,"Spread fuse by physical slot");
                AssertEqual(expected.XVelocity,slot.BombSpreadHorizontal,"Spread horizontal direction and speed");
                AssertEqual(expected.XVelocity.LaunchWord, slot.BombSpreadDirectionMagnitude,"Spread horizontal travel re-encodes the native direction/magnitude word");
                var launchVelocity=SignedSixteenSixteen.FromParts(unchecked((short)y),expected.YSpeed.Fraction);
                AssertEqual(launchVelocity,slot.BombSpreadLaunchVelocity,"Spread hold modifier, negated whole velocity and authored fraction");
                AssertEqual(launchVelocity,slot.BombSpreadVerticalVelocity,"Spread live velocity starts at the launch velocity");
                AssertEqual(y, slot.BombSpreadYVelocity,"Spread whole velocity word");
                AssertEqual(expected.YSpeed.Fraction, slot.BombSpreadYSubvelocity,"Spread fractional velocity word");
                AssertEqual(samus.Position.X.Whole,slot.Position.X.Whole,"Spread launch X wraps with Samus");
                AssertEqual(samus.Position.Y.Whole,slot.Position.Y.Whole,"Spread launch Y wraps with Samus");
            }
            AssertEqual(MorphBallBombDefinitions.SlotCount,bombs.BombCounter,"Spread allocates all five slots");
            AssertEqual(0, samus.ProjectileFlareCounter,"Spread consumes charge");
            AssertEqual(0, samus.BombSpreadChargeTimeoutCounter,"Spread clears hold counter");
        }
        Console.WriteLine("Bomb Spread launch: twenty native words and 327680 actual slot initializations match with launch ROM reads forbidden.");
        VerifyBombSpreadMotionWraps();
    }

    /// <summary>
    /// Checks the typed spread-bomb motion against the cartridge's split-word arithmetic at its wrap boundaries: the
    /// horizontal step's fraction carry and borrow and its whole-word wrap through zero and 65535 in both directions,
    /// for every launch speed, and gravity's fraction carry into a wrapping whole velocity word.
    /// </summary>
    private static void VerifyBombSpreadMotionWraps()
    {
        ushort[] boundaries = [0, 1, 0x7fff, 0x8000, 0xff00, 0xffff];
        for (int slot = 0; slot < MorphBallBombDefinitions.SlotCount; slot++)
        {
            int launchWord = SamusBombSpreadLaunches.ForSlot(slot).XVelocity.LaunchWord;
            foreach (BombSpreadHorizontalVelocity velocity in new[] {
                BombSpreadHorizontalVelocity.FromLaunchWord(launchWord),
                BombSpreadHorizontalVelocity.FromLaunchWord(launchWord).Reversed() })
            foreach (ushort whole in boundaries)
            foreach (ushort fraction in boundaries)
            {
                // The cartridge decodes the byte-swapped word and steps the split X words with explicit carry or borrow.
                ushort swapped = LittleEndianBytesConstants.SwapBytes(checked((ushort)velocity.LaunchWord));
                int subvelocity = LittleEndianBytes.HighByteInPlace(swapped);
                int magnitude = swapped & 127;
                bool left = (swapped & 1 << 7) != 0;
                int fractionSum = left ? fraction - subvelocity : fraction + subvelocity;
                int carry = left ? (fractionSum < 0 ? 1 : 0) : (fractionSum > ushort.MaxValue ? 1 : 0);
                ushort expectedWhole = unchecked((ushort)(left ? whole - magnitude - carry : whole + magnitude + carry));
                ushort expectedFraction = unchecked((ushort)fractionSum);

                var start = FixedPosition<RoomPixels>.FromWhole(new(whole, 0)).WithFractionX(fraction);
                var moved = start.AdvanceX(velocity.Displacement);
                AssertEqual(new VerifyBombSpreadMotionWrapsCase(expectedWhole, expectedFraction), new VerifyBombSpreadMotionWrapsCase(moved.X.Whole, moved.X.Fraction),
                    $"spread step {velocity} from {whole:X4}.{fraction:X4} matches the split-word carry and wrap");
            }
        }

        foreach (ushort velocityWhole in boundaries)
        foreach (ushort velocityFraction in boundaries)
        foreach (ushort gravityFraction in new ushort[] { 0, 0x1c00, 0xffff })
        {
            uint fractionalSum = (uint)velocityFraction + gravityFraction;
            ushort expectedWhole = unchecked((ushort)(velocityWhole + (fractionalSum > ushort.MaxValue ? 1 : 0)));
            var accelerated = SignedSixteenSixteen.FromParts(unchecked((short)velocityWhole), velocityFraction) +
                SignedSixteenSixteen.FromParts(0, gravityFraction);
            AssertEqual(new VerifyBombSpreadMotionWrapsCase(expectedWhole, unchecked((ushort)fractionalSum)), new VerifyBombSpreadMotionWrapsCase(accelerated.WholeWord, accelerated.Fraction),
                $"spread gravity from {velocityWhole:X4}.{velocityFraction:X4} carries and wraps like the split words");
        }
    }

    /// <summary>Runs the Bomb Spread launch-word, slot-initialization and motion-wrap checks against the retail ROM.</summary>
    [SuperMetroid.Verification.Harness.VerificationCommand("--bomb-spread-launch")]
    internal static int BombSpreadLaunchCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroid.Verification.Harness.SuiteTimingVerification.Suite(
            nameof(VerifyBombSpreadLaunchDefinitions), () => VerifyBombSpreadLaunchDefinitions(rom));
        return 0;
    }

    /// <summary>Forbids production reads across the migrated Bomb Spread launch tables.</summary>
    private static CartridgeReadGuard NewBombSpreadLaunchReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
                address >= MorphBallBombDefinitions.FuseTimers &&
                address < MorphBallBombDefinitions.YSubspeeds + MorphBallBombDefinitions.SlotCount * 2,
            _ => new InvalidDataException("Bomb Spread still read its compiled launch data from ROM."));
}
