using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

/// <summary>Diagnostic translation of upstream-sm/src/snes/ppu.c's reference Mode-7 sampling.</summary>
internal static class NativeMode7Probe
{
    internal static Rgba32[] Render(Mode7ObjRenderSnapshot snapshot, int scanlineBias)
    {
        var bg = snapshot.Background!.Value;
        int Sign13(int value) => (value << 19) >> 19;
        int Clip(int value) => (value & 0x2000) != 0 ? value | ~1023 : value & 1023;
        int cx = Sign13(bg.CenterX), cy = Sign13(bg.CenterY);
        int h = Clip(Sign13(bg.HorizontalOffset) - cx), v = Clip(Sign13(bg.VerticalOffset) - cy);
        var pixels = new Rgba32[256 * 224];
        var bytes = snapshot.Memory.Vram;
        var colors = snapshot.Memory.Cgram;
        for (int y = 0; y < 224; y++)
        {
            int startX = ((bg.MatrixA * h) & ~63) + ((bg.MatrixB * (y + scanlineBias)) & ~63) + ((bg.MatrixB * v) & ~63) + (cx << 8);
            int startY = ((bg.MatrixC * h) & ~63) + ((bg.MatrixD * (y + scanlineBias)) & ~63) + ((bg.MatrixD * v) & ~63) + (cy << 8);
            for (int x = 0; x < 256; x++)
            {
                int sx = (startX + bg.MatrixA * x) >> 8, sy = (startY + bg.MatrixC * x) >> 8;
                int color = 0;
                if ((uint)sx < 1024 && (uint)sy < 1024)
                {
                    int tile = bytes[((sy >> 3) * 128 + (sx >> 3)) * 2];
                    color = bytes[(tile * 64 + (sy & 7) * 8 + (sx & 7)) * 2 + 1];
                }
                pixels[y * 256 + x] = SnesGraphics.DecodeBgr555Color(colors[color]);
            }
        }
        var vram = new SnesVram(); vram.LoadBytes(0, bytes);
        var cgram = new SnesCgram();
        for (int i = 0; i < colors.Length; i++) cgram.SetColor(i, colors[i]);
        var oam = new OamBuffer(); oam.LoadUploadPayload(snapshot.Memory.Oam, snapshot.Memory.ModeledSpriteCount);
        var objects = new Rgba32[pixels.Length];
        var palettes = new byte[pixels.Length];
        SnesObjRenderer.RenderResolved(oam, vram, cgram, snapshot.ObjectSelection, objects, new byte[pixels.Length], palettes: palettes);
        SnesLayerCompositor.Composite(pixels, objects);
        for (int i = 0; i < pixels.Length; i++)
            if (!snapshot.Gradient.IsEmpty)
                pixels[i] = TitleGradientColorMath.Apply(pixels[i], snapshot.Gradient[i / 256], palettes[i] == 255 ? null : palettes[i]);
        MasterBrightnessFilter.Apply(pixels, snapshot.Brightness);
        return pixels;
    }
}
