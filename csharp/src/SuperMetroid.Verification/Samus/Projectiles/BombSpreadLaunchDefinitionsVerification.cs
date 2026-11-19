using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks compiled Bomb Spread launch words and actual slot initialization against retail source data.</summary>
internal static class BombSpreadLaunchDefinitionsVerification
{
    /// <summary>
    /// The launch velocity the cartridge gives a spread bomb from its vertical speed and subspeed words with
    /// <paramref name="bonusPixels"/> of held-Down bonus (#2459): it negates the bonus-raised speed word alone and keeps
    /// the subspeed as the velocity's fraction, so the centre bomb's speed of 2 and subspeed of one half launch it at
    /// -1.5 pixels per update.
    /// </summary>
    private static SignedSixteenSixteen CartridgeLaunchVelocity(int speedWord, int subspeedWord, int bonusPixels) =>
        SignedSixteenSixteen.FromParts(unchecked((short)(ushort)-(speedWord + bonusPixels)), subspeedWord);

    /// <summary>Compares five native fuse, X, Y, and subvelocity records and verifies hold-modified launches without reading migrated ROM tables.</summary>
    internal static void VerifyBombSpreadLaunchDefinitions(CartridgeImportAddressSpace rom)
    {
        int Word(int address) => CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
        var reference = Enumerable.Range(0,MorphBallBombDefinitions.PlaceCount).Select(i =>
            new BombSpreadLaunchDefinition(Word(MorphBallBombDefinitions.FuseTimers+i*2),
                BombSpreadLaunchWords.Unpack(Word(MorphBallBombDefinitions.XVelocities+i*2)),
                CartridgeLaunchVelocity(Word(MorphBallBombDefinitions.YSpeeds+i*2), Word(MorphBallBombDefinitions.YSubspeeds+i*2), 0))).ToArray();
        for(int i=0;i<reference.Length;i++)
            AssertEqual(reference[i],SamusBombSpreadLaunches.ForPlace(i),"All native Bomb Spread launch words");
        Action<SamusBombProjectileSystem,SamusState> spawn=(system,player)=>system.SpawnBombSpread(player);
        var guarded=NewBombSpreadLaunchReadGuard(rom);
        var bombs=new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        var samus=new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        for(int hold=0;hold<=MorphBallBombDefinitions.DownChargeTimeoutUpdates;hold++)
        {
            samus.BombSpreadCharge = UpdateCountdownAccess.AtAge(SuperMetroid.Core.Samus.Projectiles.MorphBallBombDefinitions.DownChargeTimeoutUpdates, (ushort)hold);
            samus.BeamCharge.SetFlareCount(BeamChargeDefinitions.FullyChargedCounter);
            samus.Position = samus.Position.WithWhole(new((ushort)hold, unchecked((ushort)~hold)));
            spawn(bombs,samus);
            for(int i=0;i<reference.Length;i++)
            {
                var slot=bombs.Bombs[i]; var expected=reference[i];
                BombSpreadFlight flight = slot.SpreadFlight ?? throw new InvalidOperationException($"Spread slot {i} has no flight.");
                AssertEqual(expected.FuseTimer,slot.Fuse.UpdatesLeft,"Spread fuse by physical slot");
                AssertEqual(expected.HorizontalVelocity,flight.HorizontalVelocity,"Spread horizontal velocity");
                AssertEqual(Word(MorphBallBombDefinitions.XVelocities+i*2), flight.PackedHorizontalVelocity,"Spread horizontal velocity re-encodes the native direction/magnitude word");
                // The cartridge's hold bonus is bits six and seven of the hold count, added to the speed word before it is negated.
                var launchVelocity=CartridgeLaunchVelocity(Word(MorphBallBombDefinitions.YSpeeds+i*2), Word(MorphBallBombDefinitions.YSubspeeds+i*2), (hold>>6)&3);
                AssertEqual(launchVelocity,flight.LaunchVelocity,"Spread hold modifier, negated speed word and authored subspeed");
                AssertEqual(launchVelocity,flight.VerticalVelocity,"Spread live velocity starts at the launch velocity");
                AssertEqual(samus.Position.X.Whole,slot.Position.X.Whole,"Spread launch X wraps with Samus");
                AssertEqual(samus.Position.Y.Whole,slot.Position.Y.Whole,"Spread launch Y wraps with Samus");
            }
            AssertEqual(MorphBallBombDefinitions.PlaceCount,bombs.BombCounter,"Spread allocates all five slots");
            AssertEqual(0, samus.BeamCharge.FlareCounter,"Spread consumes charge");
            AssertEqual(0, samus.BombSpreadCharge.Age,"Spread clears hold counter");
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
        for (int slot = 0; slot < MorphBallBombDefinitions.PlaceCount; slot++)
        {
            SignedSixteenSixteen launch = SamusBombSpreadLaunches.ForPlace(slot).HorizontalVelocity;
            foreach (SignedSixteenSixteen velocity in new[] { launch, -launch })
            foreach (ushort whole in boundaries)
            foreach (ushort fraction in boundaries)
            {
                // The cartridge decodes the byte-swapped word and steps the split X words with explicit carry or borrow.
                ushort swapped = LittleEndianBytesConstants.SwapBytes(checked((ushort)BombSpreadLaunchWords.Pack(velocity)));
                int subvelocity = LittleEndianBytes.HighByteInPlace(swapped);
                int magnitude = swapped & 127;
                bool left = (swapped & 1 << 7) != 0;
                int fractionSum = left ? fraction - subvelocity : fraction + subvelocity;
                int carry = left ? (fractionSum < 0 ? 1 : 0) : (fractionSum > ushort.MaxValue ? 1 : 0);
                ushort expectedWhole = unchecked((ushort)(left ? whole - magnitude - carry : whole + magnitude + carry));
                ushort expectedFraction = unchecked((ushort)fractionSum);

                var start = FixedPosition<RoomPixels>.FromWhole(new(whole, 0)).WithFractionX(fraction);
                var moved = start.AdvanceX(velocity);
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
            AssertEqual(new VerifyBombSpreadMotionWrapsCase(expectedWhole, unchecked((ushort)fractionalSum)), new VerifyBombSpreadMotionWrapsCase(accelerated.UnsignedWholePixels, accelerated.Fraction),
                $"spread gravity from {velocityWhole:X4}.{velocityFraction:X4} carries and wraps like the split words");
        }
    }

    /// <summary>Runs the Bomb Spread launch-word, slot-initialization and motion-wrap checks against the retail ROM.</summary>
    [SuperMetroid.Verification.Harness.VerificationCommand("--bomb-spread-launch")]
    internal static int BombSpreadLaunchCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroid.Verification.Harness.SuiteTimingVerification.Suite(
            nameof(VerifyBombSpreadLaunchDefinitions), () => VerifyBombSpreadLaunchDefinitions(rom));
        return 0;
    }

    /// <summary>Forbids production reads across the migrated Bomb Spread launch tables.</summary>
    private static CartridgeReadGuard NewBombSpreadLaunchReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
                address >= MorphBallBombDefinitions.FuseTimers &&
                address < MorphBallBombDefinitions.YSubspeeds + MorphBallBombDefinitions.PlaceCount * 2,
            _ => new InvalidDataException("Bomb Spread still read its compiled launch data from ROM."));
}
