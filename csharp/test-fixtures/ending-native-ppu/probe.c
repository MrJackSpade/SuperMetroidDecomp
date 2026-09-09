// Headless independent raster check for #506. Reads the raw memory prefix of an
// SMFRAME layered packet; display registers come from retail $8B:F2FA, not its
// C# layer descriptors. Does not run SDL or modify the ROM/game state.
#include "../../../upstream-sm/src/snes/ppu.h"
#include <windows.h>
bool g_new_ppu = false;
int main(int argc, char **argv) {
  SetErrorMode(SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX);
  if (argc != 3 && argc != 4) { fprintf(stderr, "usage: probe input.smframe output.bgra [offset-check]\n"); return 2; }
  FILE *f = fopen(argv[1], "rb");
  if (!f) return 3;
  unsigned char signature[8]; unsigned short version; unsigned int fades;
  if (fread(signature, 1, 8, f) != 8 || memcmp(signature, "SMFRAME\0", 8)) return 4;
  if (fread(&version, 2, 1, f) != 1 || version < 22 || version > 24) return 5;
  fseek(f, 18, SEEK_CUR);
  if (fread(&fades, 4, 1, f) != 1 || fades != 0 || fgetc(f) != 3) return 6;
  Snes snes = {0};
  Ppu *ppu = ppu_init(&snes); ppu_reset(ppu);
  if (fread(ppu->vram, 1, 65536, f) != 65536 || fread(ppu->cgram, 1, 512, f) != 512 ||
      fread(ppu->oam, 1, 512, f) != 512 || fread(ppu->highOam, 1, 32, f) != 32) return 7;
  fseek(f, 4, SEEK_CUR); // modeled OAM count: native PPU always reads all slots
  int obsel = fgetc(f), brightness = fgetc(f); fclose(f);
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
    } else if (strcmp(argv[3], "burst")) return 10;
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
  PpuBeginDrawing(ppu, pixels, 256 * 4, 0);
  for (int line = 0; line <= 224; line++) ppu_runLine(ppu, line);
  f = fopen(argv[2], "wb"); if (!f) return 8;
  int ok = fwrite(pixels, 4, 256 * 224, f) == 256 * 224;
  fclose(f); free(pixels); ppu_free(ppu);
  return ok ? 0 : 9;
}
