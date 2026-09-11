#include "native-bounded-cpu.h"

// #565: original boss icon OAM, all area boss bytes and map-download states.
int DiagnosticBossMap(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "area,map,bits,bytes,low,high\n");
  for (int area = 0; area < 6; area++) for (int map = 0; map < 2; map++)
  for (int bits = 0; bits < 256; bits++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    area_index = area; has_area_map = map; boss_bits_for_area[area] = bits;
    reg_BG1HOFS = 64; reg_BG1VOFS = 16;
    ProbeRunBoundedRegisters(0x82b892, 9, 0xc7cb, 0);
    fprintf(f, "%d,%d,%d,%d,", area, map, bits, oam_next_ptr);
    for (int i = 0; i < oam_next_ptr; i++) fprintf(f, "%02X", g_ram[0x370+i]);
    fputc(',', f);
    for (int i = 0; i < 32; i++) fprintf(f, "%02X", g_ram[0x570+i]);
    fputc('\n', f);
  }
  fclose(f); return 0;
}
