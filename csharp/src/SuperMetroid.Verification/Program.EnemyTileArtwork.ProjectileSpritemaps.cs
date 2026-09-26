using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledEnemyProjectileSpritemaps(
        ISnesAddressSpace bus, string directory, EnemyTileArtworkCatalog stock)
    {
        EnemyProjectileSpritemapCatalog installed = stock.ProjectileSpritemaps
            ?? throw new InvalidDataException("Installed enemy-projectile compositions are missing.");
        foreach ((ushort pointer, _) in EnemyProjectileSpritemapDefinitions.Frames)
        foreach ((ushort originX, ushort originY, ushort graphicsIndex) in new[]
                 {
                     ((ushort)0x0080, (ushort)0x0004, (ushort)0x0000),
                     ((ushort)0x01ff, (ushort)0x00fc, (ushort)0x0a04),
                 })
        {
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            nativeOam.BeginFrame();
            installedOam.BeginFrame();
            bool onScreen = (originY & 0xff00) == 0;
            nativeOam.AddEnemyProjectileSpritemap(bus, pointer,
                originX, originY, graphicsIndex, onScreen);
            installedOam.AddEnemySpritemap(installed.Get(pointer).Span,
                originX, originY,
                unchecked((ushort)(graphicsIndex & 0xff00)),
                unchecked((byte)graphicsIndex),
                clipVerticalWrap: true, originYIsOnScreen: onScreen);
            AssertEqual(nativeOam.NextByteOffset, installedOam.NextByteOffset,
                $"installed $8D:{pointer:X4} OAM part count");
            AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                       nativeOam.HighTable.SequenceEqual(installedOam.HighTable),
                $"installed $8D:{pointer:X4} OAM pixels, wrapping, tile base and palette");
        }

        var nativeSamus = new SamusState { XPosition = 0x0080, YPosition = 0 };
        var installedSamus = new SamusState { XPosition = 0x0080, YPosition = 0 };
        var nativeArrival = new CeresElevatorArrivalState(bus, nativeSamus);
        var installedArrival = new CeresElevatorArrivalState(
            new EnemyProjectileVisualReadGuard(bus), installedSamus, installed);
        for (int frame = 0; frame < 3; frame++)
        {
            nativeArrival.Step(nativeSamus);
            installedArrival.Step(installedSamus);
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            nativeOam.BeginFrame();
            installedOam.BeginFrame();
            nativeArrival.Draw(nativeOam, 0, 0);
            installedArrival.Draw(installedOam, 0, 0);
            AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                       nativeOam.HighTable.SequenceEqual(installedOam.HighTable),
                $"Ceres elevator arrival frame {frame} draws from installed $8D compositions");
        }

        string overrides = Path.Combine(directory, "eproj-overrides");
        Directory.CreateDirectory(overrides);
        EnemyProjectileSpritemapDocument document =
            JsonSerializer.Deserialize<EnemyProjectileSpritemapDocument>(
                File.ReadAllBytes(Path.Combine(directory,
                    EnemyProjectileSpritemapDefinitions.FileName)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        SpriteVisualPart[] pad = document.Frames["ceres_elevator_pad_0"];
        pad[0] = pad[0] with { OffsetX = pad[0].OffsetX + 1 };
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName),
            EnemyProjectileSpritemapCatalog.Write(document));
        EnemyProjectileSpritemapCatalog edited = EnemyTileArtworkFiles.Load(
            directory, overrides).ProjectileSpritemaps!;
        var originalOam = new OamBuffer();
        var editedOam = new OamBuffer();
        originalOam.BeginFrame();
        editedOam.BeginFrame();
        originalOam.AddEnemySpritemap(installed.Get(0xb1ba).Span,
            0x0080, 0x0004, 0, 0, clipVerticalWrap: true);
        editedOam.AddEnemySpritemap(edited.Get(0xb1ba).Span,
            0x0080, 0x0004, 0, 0, clipVerticalWrap: true);
        AssertEqual((byte)(originalOam.LowTable[0] + 1), editedOam.LowTable[0],
            "enemy-projectile composition override moves the live Ceres pad OAM part");
    }

    private sealed class EnemyProjectileVisualReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if ((address & 0xff0000) == 0x8d0000)
                throw new InvalidOperationException(
                    $"Installed enemy projectile reread $8D:{address & 0xffff:X4}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
