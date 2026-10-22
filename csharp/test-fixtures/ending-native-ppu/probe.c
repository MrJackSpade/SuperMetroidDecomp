// Headless independent raster check for #506. Reads a PPU memory dump written by
// --ending-native-ppu (VRAM, CGRAM, the 544-byte OAM upload, OBSEL, brightness);
// display registers come from retail $8B:F2FA. Does not run SDL or modify the ROM/game state.
#include "../../../upstream-sm/src/snes/ppu.h"
#include <windows.h>
bool g_new_ppu = false;
int main(int argc, char **argv) {
  SetErrorMode(SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX);
  if (argc != 3 && argc != 4) { fprintf(stderr, "usage: probe input.ppu output.bgra [offset-check]\n"); return 2; }
  FILE *f = fopen(argv[1], "rb");
  if (!f) { fprintf(stderr, "cannot open %s\n", argv[1]); return 3; }
  Snes snes = {0};
  Ppu *ppu = ppu_init(&snes); ppu_reset(ppu);
  if (fread(ppu->vram, 1, 65536, f) != 65536 || fread(ppu->cgram, 1, 512, f) != 512 ||
      fread(ppu->oam, 1, 512, f) != 512 || fread(ppu->highOam, 1, 32, f) != 32) {
    fprintf(stderr, "%s is shorter than VRAM, CGRAM and the OAM upload\n", argv[1]); return 7;
  }
  /* OBSEL is whatever the scene selects: $02 for the explosion, $03 elsewhere. */
  int obsel = fgetc(f), brightness = fgetc(f);
  if (obsel == EOF || brightness == EOF || fgetc(f) != EOF) {
    fprintf(stderr, "%s does not end with exactly OBSEL and brightness\n", argv[1]); return 6;
  }
  fclose(f);
  ppu_write(ppu, 0x00, (unsigned char)brightness);
  ppu_write(ppu, 0x01, (unsigned char)obsel);
  ppu_write(ppu, 0x05, 1);
  ppu_write(ppu, 0x07, 0x74); ppu_write(ppu, 0x08, 0x78);
  ppu_write(ppu, 0x0b, 0x44);
  if (argc == 4) {
    // Diagnostic only: isolate the native physical-line-one sampling difference.
    // This is NOT the cartridge's scroll setting, which remains zero above.
    if (!strcmp(argv[3], "offset-check")) {
      ppu_write(ppu, 0x0e, 0xff); ppu_write(ppu, 0x0e, 0xff);
      ppu_write(ppu, 0x10, 0xff); ppu_write(ppu, 0x10, 0xff);
    } else if (strcmp(argv[3], "burst") && strcmp(argv[3], "pause")) return 10;
  }
  ppu_write(ppu, 0x2c, 3); ppu_write(ppu, 0x2d, 0x12);
  ppu_write(ppu, 0x30, 2); ppu_write(ppu, 0x31, 0x33);
  if (argc == 4 && !strcmp(argv[3], "burst")) {
    // Retail $8B:F2B7, preceding the finale register handoff.
    ppu_write(ppu, 0x07, 0x70); ppu_write(ppu, 0x08, 0x7c);
    ppu_write(ppu, 0x2c, 0x11); ppu_write(ppu, 0x2d, 2);
    ppu_write(ppu, 0x31, 0x11);
  }
  unsigned char *pixels = calloc(256 * 224, 4);
  if (argc == 4 && !strcmp(argv[3], "pause")) {
    // Pause equipment: $82:9142 zeros BG1 scroll; $82:A0F7 zeros BG2/3.
    // Keep physical scanline sampling in the independent PPU, not C# layers.
    ppu_write(ppu, 0x05, 9);
    ppu_write(ppu, 0x07, 0x31); ppu_write(ppu, 0x08, 0x38); ppu_write(ppu, 0x09, 0x58);
    ppu_write(ppu, 0x0b, 0); ppu_write(ppu, 0x0c, 4);
    ppu_write(ppu, 0x2c, 0x17); ppu_write(ppu, 0x2d, 0);
    ppu_write(ppu, 0x30, 0); ppu_write(ppu, 0x31, 0);
  }
  PpuBeginDrawing(ppu, pixels, 256 * 4, 0);
  for (int line = 0; line <= 224; line++) ppu_runLine(ppu, line);
  f = fopen(argv[2], "wb"); if (!f) return 8;
  int ok = fwrite(pixels, 4, 256 * 224, f) == 256 * 224;
  fclose(f); free(pixels); ppu_free(ppu);
  return ok ? 0 : 9;
}
