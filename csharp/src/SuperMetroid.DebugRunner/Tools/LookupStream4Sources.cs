using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using System.Reflection;
using System.Text.Json;

internal static partial class AssetTools
{
    private static void ExportLookupStream4HudTemplateSource(ISnesAddressSpace rom)
    {
        const int scale=4,width=256*scale,height=32*scale;byte[] pixels=new byte[width*height];
        var palette=new Rgba32[32];for(int color=0;color<32;color++){int word=rom.ReadByte(0x9a8000+color*2)|rom.ReadByte(0x9a8001+color*2)<<8;palette[color]=(color%4) switch { 0=>new Rgba32(40,40,40,255),1=>new Rgba32(255,255,255,255),2=>new Rgba32(130,130,130,255),_=>new Rgba32(0,0,0,255) };}
        for(int cell=0;cell<128;cell++)
        {
            int address=0x80988b+cell*2,word=rom.ReadByte(address)|rom.ReadByte(address+1)<<8,tile=word&0x3ff;
            for(int y=0;y<8;y++)for(int x=0;x<8;x++)
            {
                int sy=(word&0x8000)!=0?7-y:y,sx=(word&0x4000)!=0?7-x:x,row=HudTileAtlasFormat.SourceAddress+tile*16+sy*2;
                int ink=(rom.ReadByte(row)>>(7-sx)&1)|(rom.ReadByte(row+1)>>(7-sx)&1)<<1;
                for(int dy=0;dy<scale;dy++)for(int dx=0;dx<scale;dx++)pixels[((cell/32*8+y)*scale+dy)*width+(cell%32*8+x)*scale+dx]=(byte)((word>>10&7)*4+ink);
            }
        }
        string directory=Path.GetFullPath("csharp/test-temp/hud-auto-source");Directory.CreateDirectory(directory);
        using var image=File.Create(Path.Combine(directory,"native-template.png"));IndexedPng.Write(image,width,height,pixels,palette);
        Console.WriteLine("Native HUD initial four rows exported with diagnostic index colors; initial CGRAM is uniformly grey.");
    }

    private static void ExportLookupStream4HudAutoSource(ISnesAddressSpace rom)
    {
        const int scale=8,width=40*scale,height=24*scale;
        byte[] pixels=new byte[width*height];
        for(int state=0;state<2;state++)
        for(int cell=0;cell<6;cell++)
        {
            int address=0x80998b+state*12+cell*2;
            int word=rom.ReadByte(address)|rom.ReadByte(address+1)<<8;
            int tile=word&0x3ff;
            for(int y=0;y<8;y++)for(int x=0;x<8;x++)
            {
                int sy=(word&0x8000)!=0?7-y:y,sx=(word&0x4000)!=0?7-x:x;
                int row=HudTileAtlasFormat.SourceAddress+tile*16+sy*2;
                int ink=(rom.ReadByte(row)>>(7-sx)&1)|(rom.ReadByte(row+1)>>(7-sx)&1)<<1;
                for(int dy=0;dy<scale;dy++)for(int dx=0;dx<scale;dx++)
                    pixels[((cell/2*8+y)*scale+dy)*width+(state*24+cell%2*8+x)*scale+dx]=(byte)ink;
            }
        }
        string directory=Path.GetFullPath("csharp/test-temp/hud-auto-source");Directory.CreateDirectory(directory);
        using var output=File.Create(Path.Combine(directory,"native-auto.png"));
        IndexedPng.Write(output,width,height,pixels,[new Rgba32(20,20,20,255),new Rgba32(100,100,100,255),new Rgba32(180,180,180,255),new Rgba32(255,255,255,255)]);
        Console.WriteLine("Native HUD AUTO two-state geometry exported from9A:B200 characters and80:998B/9997 words.");
    }

    private static void ExportLookupStream4DraygonHealthSource(CartridgeImportAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        byte[] tiles = SnesGraphics.DecodePlanarTiles(Enumerable.Range(0, 0x2000).Select(i => rom.ReadByte(0xb0c800 + i)).ToArray(), 4, 16, out int width, out int height);
        byte[] roomPlanar = RomDataReader.Decompress(rom, RoomTilesetDefinitions.Get(0x1c).CharacterAddress);
        byte[] cre = RomDataReader.Decompress(rom, RoomAssetRomData.Tilesets.CreCharactersAddress);
        byte[] bgPlanar = new byte[Math.Max(roomPlanar.Length, RoomAssetRomData.GraphicsLayout.CreCharactersVramByteOffset + cre.Length)];
        roomPlanar.CopyTo(bgPlanar, 0); cre.CopyTo(bgPlanar, RoomAssetRomData.GraphicsLayout.CreCharactersVramByteOffset);
        byte[] bgTiles = SnesGraphics.DecodePlanarTiles(bgPlanar, 4, 16, out int bgWidth, out _);
        string directory = Path.GetFullPath("csharp/test-temp/draygon-health-source"); Directory.CreateDirectory(directory);
        var canvas = new byte[256 * 256];
        int cursor = 0xa5b240;
        while (Word(cursor) != 0xffff)
        {
            int first = (Word(cursor) - 0x2000) / 2, count = Word(cursor + 2); cursor += 4;
            for (int cell = 0; cell < count; cell++, cursor += 2)
            {
                int attr = Word(cursor), tile = attr & 0x3ff;
                for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
                {
                    int sx = (attr & 0x4000) == 0 ? x : 7 - x, sy = (attr & 0x8000) == 0 ? y : 7 - y;
                    byte ink = bgTiles[(tile / 16 * 8 + sy) * bgWidth + tile % 16 * 8 + sx];
                    canvas[((first + cell) / 32 * 8 + y) * 256 + (first + cell) % 32 * 8 + x] = ink;
                }
            }
        }
        var bodyTiles = new HashSet<int>();
        int targetFrame = 0, targetStream = 0;
        for (int facing = 0; facing < 2; facing++) for (int frame = 0; frame < 17; frame++)
        {
            int root = 0xa50000 | ((facing == 0 ? 0xa31b : 0xa643) + frame * 10);
            int source = 0xa50000 | Word(root + 6); source += 2;
            while (Word(source) != 0xffff)
            {
                int count = Word(source + 2); source += 4;
                for (int cell = 0; cell < count; cell++, source += 2)
                {
                    int attr = Word(source);
                    if ((attr >> 10 & 7) == 5)
                    {
                        int tile = attr & 0x3ff; bodyTiles.Add(tile);
                        for(int y=0;y<8;y++) for(int x=0;x<8;x++)
                            if(targetFrame == 0 && bgTiles[(tile / 16 * 8 + y) * bgWidth + tile % 16 * 8 + x] is 13 or 14)
                            { targetFrame=root; targetStream=0xa50000|Word(root+6); }
                    }
                }
            }
        }
        if(targetStream != 0)
        {
            var targetCanvas = new byte[256*256]; int source = targetStream+2;
            while(Word(source)!=0xffff)
            {
                int first=(Word(source)-0x2000)/2,count=Word(source+2); source+=4;
                for(int cell=0;cell<count;cell++,source+=2)
                {
                    int attr=Word(source),tile=attr&0x3ff;
                    for(int y=0;y<8;y++)for(int x=0;x<8;x++)
                    {
                        int sx=(attr&0x4000)==0?x:7-x,sy=(attr&0x8000)==0?y:7-y;
                        targetCanvas[((first+cell)/32*8+y)*256+(first+cell)%32*8+x]=bgTiles[(tile/16*8+sy)*bgWidth+tile%16*8+sx];
                    }
                }
            }
            var targetColors=Enumerable.Range(0,16).Select(i=>SnesGraphics.DecodeBgr555Color(Word(0xa5a277+i*2))).ToArray(); targetColors[0]=new Rgba32(35,35,35,255);
            byte[] enlarged=Enumerable.Range(0,512*512).Select(i=>targetCanvas[i/512/2*256+i%512/2]).ToArray();
            using(var output=File.Create(Path.Combine(directory,"material-target-frame.png")))IndexedPng.Write(output,512,512,enlarged,targetColors);
            for(int slot=13;slot<=14;slot++)
            {
                using var output=File.Create(Path.Combine(directory,$"material-target-slot-{slot}.png"));
                IndexedPng.Write(output,512,512,enlarged.Select(pixel=>(byte)(pixel==slot?1:0)).ToArray(),[new Rgba32(35,35,35,255),new Rgba32(255,220,0,255)]);
            }
            Console.WriteLine($"Draygon target-color source frame {targetFrame:X6}, stream {targetStream:X6}.");
        }
        var bodyUsage = new int[16];
        foreach (int tile in bodyTiles) for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
            bodyUsage[bgTiles[(tile / 16 * 8 + y) * bgWidth + tile % 16 * 8 + x]]++;
        for (int slot = 1; slot < 16; slot++)
        {
            byte[] bodyMask = Enumerable.Range(0, 512 * 512).Select(i => (byte)(canvas[i / 512 / 2 * 256 + i % 512 / 2] == slot ? 1 : 0)).ToArray();
            using var output = File.Create(Path.Combine(directory, $"material-body-slot-{slot}.png"));
            IndexedPng.Write(output, 512, 512, bodyMask, [new Rgba32(35,35,35,255),new Rgba32(255,220,0,255)]);
            Console.WriteLine($"Draygon material slot{slot}: BG2 firstframe {canvas.Count(pixel => pixel == slot)}, all34frames unique tiles {bodyUsage[slot]}, sprite atlas {tiles.Count(pixel => pixel == slot)}.");
        }
        for (int endpoint = 0; endpoint < 2; endpoint++)
        {
            var palette = Enumerable.Range(0, 16).Select(i => SnesGraphics.DecodeBgr555Color(Word(0xa5a277 + i * 2))).ToArray();
            for (int shade = 0; shade < 4; shade++) palette[9 + shade] = SnesGraphics.DecodeBgr555Color(Word(0xa596af + endpoint * 7 * 8 + shade * 2));
            palette[0] = new Rgba32(35,35,35,255);
            byte[] enlarged = Enumerable.Range(0, width * height * 16).Select(i => tiles[i / (width * 4) / 4 * width + i % (width * 4) / 4]).ToArray();
            using (var output = File.Create(Path.Combine(directory, $"atlas-{endpoint}.png"))) IndexedPng.Write(output, width * 4, height * 4, enlarged, palette);
            byte[] body = Enumerable.Range(0, 512 * 512).Select(i => canvas[i / 512 / 2 * 256 + i % 512 / 2]).ToArray();
            using (var output = File.Create(Path.Combine(directory, $"body-{endpoint}.png"))) IndexedPng.Write(output, 512, 512, body, palette);
            if (endpoint == 0) for (int shade = 0; shade < 4; shade++)
            {
                int slot = 9 + shade;
                using var output = File.Create(Path.Combine(directory, $"body-slot-{slot}.png"));
                IndexedPng.Write(output, 512, 512, body.Select(pixel => (byte)(pixel == slot ? 1 : 0)).ToArray(), [new Rgba32(35,35,35,255),new Rgba32(255,220,0,255)]);
                Console.WriteLine($"Native Draygon B23E slot{slot}: {canvas.Count(pixel => pixel == slot)} pixels; separate sprite atlas {tiles.Count(pixel => pixel == slot)}.");
            }
        }
    }

    private static void ExportLookupStream4NorfairRevealSource(CartridgeImportAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        int Long(int address) => Word(address) | rom.ReadByte(address + 2) << 16;
        var theme = RoomTilesetDefinitions.Get(9);
        byte[] paletteBytes = RomDataReader.Decompress(rom, theme.PaletteAddress);
        byte[] area = RomDataReader.Decompress(rom, theme.CharacterAddress);
        byte[] cre = RomDataReader.Decompress(rom, RoomAssetRomData.Tilesets.CreCharactersAddress);
        byte[] planar = new byte[Math.Max(area.Length, RoomAssetRomData.GraphicsLayout.CreCharactersVramByteOffset + cre.Length)];
        area.CopyTo(planar, 0); cre.CopyTo(planar, RoomAssetRomData.GraphicsLayout.CreCharactersVramByteOffset);
        byte[] map = RomDataReader.Decompress(rom, Long(0x8fbf34));
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, 16, out int width, out _);
        var colors = Enumerable.Range(0, 128).Select(i => SnesGraphics.DecodeBgr555Color((ushort)(paletteBytes[i * 2] | paletteBytes[i * 2 + 1] << 8))).ToArray();
        for (int color = 0; color < 14; color++)
        {
            ushort endpoint = Word(0xa6a693 + color * 2);
            ushort roomColor = (ushort)(paletteBytes[(113 + color) * 2] | paletteBytes[(113 + color) * 2 + 1] << 8);
            Console.WriteLine($"Norfair reveal endpoint slot{color + 1}: {endpoint:X4}; room theme9 {roomColor:X4}.");
            colors[113 + color] = SnesGraphics.DecodeBgr555Color(endpoint);
        }
        Console.WriteLine($"Norfair theme9 chars {theme.CharacterAddress:X6}, palette {theme.PaletteAddress:X6}; BG map {Long(0x8fbf34):X6}, {map.Length} bytes.");
        var canvas = new byte[256 * 256];
        for (int cell = 0; cell < 1024; cell++)
        {
            int attributes = map[cell * 2] | map[cell * 2 + 1] << 8;
            int tile = attributes & 0x3ff, palette = attributes >> 10 & 7;
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
            {
                int sx = (attributes & 0x4000) == 0 ? x : 7 - x;
                int sy = (attributes & 0x8000) == 0 ? y : 7 - y;
                byte index = pixels[(tile / 16 * 8 + sy) * width + tile % 16 * 8 + sx];
                canvas[(cell / 32 * 8 + y) * 256 + cell % 32 * 8 + x] = (byte)(palette * 16 + index);
            }
        }
        string directory = Path.GetFullPath("csharp/test-temp/norfair-reveal-source"); Directory.CreateDirectory(directory);
        byte[] enlarged = Enumerable.Range(0, 512 * 512).Select(i => canvas[i / 512 / 2 * 256 + i % 512 / 2]).ToArray();
        using (var output = File.Create(Path.Combine(directory, "native-background.png"))) IndexedPng.Write(output, 512, 512, enlarged, colors);
        for (int slot = 1; slot <= 14; slot++)
        {
            byte[] mask = enlarged.Select(pixel => (byte)(pixel == 112 + slot ? 1 : 0)).ToArray();
            using var output = File.Create(Path.Combine(directory, $"slot-{slot}.png"));
            IndexedPng.Write(output, 512, 512, mask, [new Rgba32(35,35,35,255), new Rgba32(255,220,0,255)]);
            Console.WriteLine($"Norfair native background slot{slot}: {canvas.Count(pixel => pixel == 112 + slot)} pixels.");
        }
    }
}
