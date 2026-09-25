namespace SuperMetroid.Core.Rooms;

/// <summary>Mother Brain fake-death room physical PLM draws at $84:94A3..9716.</summary>
internal static class MotherBrainFakeDeathPlmDrawDefinitions
{
    /// <summary><c>$84:94A3</c>: fill wall.</summary>
    internal const ushort FillWall = 0x94a3;
    /// <summary><c>$84:94B1</c>: escape door.</summary>
    internal const ushort EscapeDoor = 0x94b1;
    /// <summary><c>$84:9505</c>: background row 2.</summary>
    internal const ushort BackgroundRow2 = 0x9505;
    /// <summary><c>$84:9523</c>: background row 3.</summary>
    internal const ushort BackgroundRow3 = 0x9523;
    /// <summary><c>$84:9541</c>: background row 4.</summary>
    internal const ushort BackgroundRow4 = 0x9541;
    /// <summary><c>$84:955F</c>: background row 5.</summary>
    internal const ushort BackgroundRow5 = 0x955f;
    /// <summary><c>$84:957D</c>: background row 6.</summary>
    internal const ushort BackgroundRow6 = 0x957d;
    /// <summary><c>$84:959B</c>: background row 7.</summary>
    internal const ushort BackgroundRow7 = 0x959b;
    /// <summary><c>$84:95B9</c>: background row 8.</summary>
    internal const ushort BackgroundRow8 = 0x95b9;
    /// <summary><c>$84:95D7</c>: background row 9.</summary>
    internal const ushort BackgroundRow9 = 0x95d7;
    /// <summary><c>$84:95F5</c>: background row a.</summary>
    internal const ushort BackgroundRowA = 0x95f5;
    /// <summary><c>$84:9613</c>: background row b.</summary>
    internal const ushort BackgroundRowB = 0x9613;
    /// <summary><c>$84:9631</c>: background row c.</summary>
    internal const ushort BackgroundRowC = 0x9631;
    /// <summary><c>$84:964F</c>: background row d.</summary>
    internal const ushort BackgroundRowD = 0x964f;
    /// <summary><c>$84:966D</c>: cartridge-unused background row E.</summary>
    internal const ushort BackgroundRowEUnused = 0x966d;
    /// <summary><c>$84:968B</c>: cartridge-unused background row F.</summary>
    internal const ushort BackgroundRowFUnused = 0x968b;
    /// <summary><c>$84:96A9</c>: clear ceiling block.</summary>
    internal const ushort ClearCeilingBlock = 0x96a9;
    /// <summary><c>$84:96B1</c>: clear ceiling tube.</summary>
    internal const ushort ClearCeilingTube = 0x96b1;
    /// <summary><c>$84:96BF</c>: clear bottom middle side tube.</summary>
    internal const ushort ClearBottomMiddleSideTube = 0x96bf;
    /// <summary><c>$84:96CB</c>: clear bottom middle tubes.</summary>
    internal const ushort ClearBottomMiddleTubes = 0x96cb;
    /// <summary><c>$84:96EF</c>: clear bottom left tube.</summary>
    internal const ushort ClearBottomLeftTube = 0x96ef;
    /// <summary><c>$84:9703</c>: clear bottom right tube.</summary>
    internal const ushort ClearBottomRightTube = 0x9703;
    /// <summary><c>$84:9717</c>: first byte of the following glass draw region.</summary>
    internal const ushort EndExclusive = 0x9717;

    private static readonly Dictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All =>
        Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList draw) =>
        Lists.TryGetValue(pointer, out draw);

    private static Dictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Build()
    {
        // Keep the complete native level words here. The draw consumer writes
        // their collision nibble even when a later visual override changes the
        // tile appearance; offsets are signed relative to the PLM origin.
        var lists = new Dictionary<ushort,
            RoomPlmShotBlockDrawDefinitions.DrawList>();
        Add(FillWall,
            new RoomPlmShotBlockDrawDefinitions.Run(0x8002,
                new ushort[] { 0x8340, 0x830f }, 0, -1),
            new RoomPlmShotBlockDrawDefinitions.Run(0x8001,
                new ushort[] { 0x8b0f }, 0, 0));
        Add(EscapeDoor,
            new RoomPlmShotBlockDrawDefinitions.Run(0x8004,
                new ushort[] { 0x9222, 0xd1af, 0xd1d0, 0xd220 }, 1, 0),
            new RoomPlmShotBlockDrawDefinitions.Run(0x8004,
                new ushort[] { 0x0223, 0x01eb, 0x01d0, 0x0221 }, 0, 0));
        Add(BackgroundRow2,
            new RoomPlmShotBlockDrawDefinitions.Run(0x000d,
                new ushort[] { 0x1241, 0x1242, 0x12fc, 0x12fc, 0x12fc, 0x1243, 0x1244, 0x12fc, 0x1245, 0x1642, 0x1241, 0x1241, 0x1246 }, 0, 0));
        Add(BackgroundRow3,
            new RoomPlmShotBlockDrawDefinitions.Run(0x000d,
                new ushort[] { 0x09ef, 0x01b2, 0x01e5, 0x01e5, 0x01e6, 0x01e5, 0x01e5, 0x01e5, 0x01e5, 0x05b2, 0x09ef, 0x09ef, 0x01b2 }, 0, 0));
        Add(BackgroundRow4,
            new RoomPlmShotBlockDrawDefinitions.Run(0x000d,
                new ushort[] { 0x01b1, 0x01d2, 0x01c6, 0x01c7, 0x00ff, 0x0206, 0x0207, 0x00ff, 0x01a6, 0x09ca, 0x060c, 0x05b1, 0x0a09 }, 0, 0));
        Add(BackgroundRow5,
            new RoomPlmShotBlockDrawDefinitions.Run(0x000d,
                new ushort[] { 0x01d1, 0x01f2, 0x01a4, 0x01e7, 0x01a4, 0x0226, 0x0227, 0x01a5, 0x01a4, 0x020d, 0x0e09, 0x01b1, 0x01ab }, 0, 0));
        Add(BackgroundRow6,
            new RoomPlmShotBlockDrawDefinitions.Run(0x000d,
                new ushort[] { 0x01b1, 0x0212, 0x01c4, 0x01c9, 0x01c4, 0x0206, 0x0207, 0x01c5, 0x01c4, 0x0628, 0x01ac, 0x01ec, 0x01ec }, 0, 0));
        Add(BackgroundRow7,
            new RoomPlmShotBlockDrawDefinitions.Run(0x000d,
                new ushort[] { 0x01b1, 0x0a0c, 0x05ca, 0x0dc7, 0x01aa, 0x01a8, 0x01a8, 0x01a8, 0x01a8, 0x0628, 0x01ab, 0x01cd, 0x01cd }, 0, 0));
        Add(BackgroundRow8,
            new RoomPlmShotBlockDrawDefinitions.Run(0x000d,
                new ushort[] { 0x01d1, 0x01d0, 0x05ea, 0x00ff, 0x00ff, 0x0206, 0x0207, 0x00ff, 0x01a7, 0x0a0d, 0x0609, 0x01eb, 0x01d0 }, 0, 0));
        Add(BackgroundRow9,
            new RoomPlmShotBlockDrawDefinitions.Run(0x000d,
                new ushort[] { 0x01eb, 0x01eb, 0x05ea, 0x00ff, 0x00ff, 0x0206, 0x0207, 0x00ff, 0x01a6, 0x00ff, 0x0a2c, 0x0609, 0x01ae }, 0, 0));
        Add(BackgroundRowA,
            new RoomPlmShotBlockDrawDefinitions.Run(0x000d,
                new ushort[] { 0x01ec, 0x01af, 0x05ea, 0x05c7, 0x05c6, 0x0206, 0x0207, 0x01a8, 0x01a6, 0x01a8, 0x01a8, 0x05d2, 0x01ae }, 0, 0));
        Add(BackgroundRowB,
            new RoomPlmShotBlockDrawDefinitions.Run(0x000d,
                new ushort[] { 0x01ac, 0x01af, 0x01b2, 0x05e7, 0x01e5, 0x0226, 0x0227, 0x01e5, 0x01a6, 0x01e6, 0x01e5, 0x05b2, 0x01cd }, 0, 0));
        Add(BackgroundRowC,
            new RoomPlmShotBlockDrawDefinitions.Run(0x000d,
                new ushort[] { 0x060c, 0x01ef, 0x01b2, 0x01e5, 0x01e6, 0x01e5, 0x01e5, 0x01e6, 0x01e5, 0x01e5, 0x01e5, 0x05b2, 0x01ef }, 0, 0));
        Add(BackgroundRowD,
            new RoomPlmShotBlockDrawDefinitions.Run(0x000d,
                new ushort[] { 0x1248, 0x1249, 0x124a, 0x124b, 0x1339, 0x124c, 0x124d, 0x1339, 0x124e, 0x1339, 0x1339, 0x124f, 0x1249 }, 0, 0));
        Add(BackgroundRowEUnused,
            new RoomPlmShotBlockDrawDefinitions.Run(0x000d,
                new ushort[] { 0x8319, 0x8319, 0x8319, 0x8319, 0x8319, 0x8319, 0x8319, 0x8319, 0x8319, 0x8319, 0x8319, 0x8319, 0x8319 }, 0, 0));
        Add(BackgroundRowFUnused,
            new RoomPlmShotBlockDrawDefinitions.Run(0x000d,
                new ushort[] { 0x8044, 0x8044, 0x8044, 0x8044, 0x8044, 0x8044, 0x8044, 0x8044, 0x8044, 0x8044, 0x8044, 0x8044, 0x8044 }, 0, 0));
        Add(ClearCeilingBlock,
            new RoomPlmShotBlockDrawDefinitions.Run(0x8002,
                new ushort[] { 0x12fc, 0x00ff }, 0, 0));
        Add(ClearCeilingTube,
            new RoomPlmShotBlockDrawDefinitions.Run(0x8005,
                new ushort[] { 0x12fc, 0x00ff, 0x00ff, 0x00ff, 0x00ff }, 0, 0));
        Add(ClearBottomMiddleSideTube,
            new RoomPlmShotBlockDrawDefinitions.Run(0x8004,
                new ushort[] { 0x00ff, 0x00ff, 0x00ff, 0x1339 }, 0, 0));
        Add(ClearBottomMiddleTubes,
            new RoomPlmShotBlockDrawDefinitions.Run(0x8007,
                new ushort[] { 0x00ff, 0x00ff, 0x00ff, 0x00ff, 0x00ff, 0x00ff, 0x1339 }, 1, 0),
            new RoomPlmShotBlockDrawDefinitions.Run(0x8007,
                new ushort[] { 0x00ff, 0x00ff, 0x00ff, 0x00ff, 0x00ff, 0x00ff, 0x1339 }, 0, 0));
        Add(ClearBottomLeftTube,
            new RoomPlmShotBlockDrawDefinitions.Run(0x8005,
                new ushort[] { 0x00ff, 0x00ff, 0x00ff, 0x00ff, 0x1339 }, 1, 0),
            new RoomPlmShotBlockDrawDefinitions.Run(0x0001,
                new ushort[] { 0x00ff }, 0, 0));
        Add(ClearBottomRightTube,
            new RoomPlmShotBlockDrawDefinitions.Run(0x8005,
                new ushort[] { 0x00ff, 0x00ff, 0x00ff, 0x00ff, 0x1339 }, -1, 0),
            new RoomPlmShotBlockDrawDefinitions.Run(0x0001,
                new ushort[] { 0x00ff }, 0, 0));
        return lists;

        void Add(ushort pointer,
            params RoomPlmShotBlockDrawDefinitions.Run[] runs)
        {
            if (!lists.TryAdd(pointer,
                    new RoomPlmShotBlockDrawDefinitions.DrawList(pointer, runs)))
                throw new InvalidDataException(
                    $"Duplicate Mother Brain fake-death draw ${pointer:X4}.");
        }
    }
}
