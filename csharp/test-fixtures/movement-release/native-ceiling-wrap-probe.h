#include "native-bounded-cpu.h"

// #410: observe original A352 dispatch addresses without replacing arithmetic.
// Air-only synthetic rooms isolate byte-offset generation from PLM side effects.
int DiagnosticCeilingWrap(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "width,tilex,visit,byteoffset\n");
  for (int width = 112; width <= 144; width += 16) for (int tilex = 63; tilex <= 65; tilex++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    Cpu *cpu = g_snes->cpu;
    cpu->e = false; cpu->sp = 0x1ff0; cpu->dp = 0;
    room_width_in_blocks = width; room_height_in_blocks = 32;
    room_width_in_scrolls = width / 16; room_height_in_scrolls = 2;
    room_size_in_blocks = width * 32 * 2;
    projectile_x_pos[0] = tilex * 16; projectile_y_pos[0] = 4;
    projectile_x_radius[0] = 1; projectile_y_radius[0] = 8;
    cpu->db = cpu->k = 0x94; cpu->pc = 0xa352;
    cpu->a = cpu->x = cpu->y = 0; cpu->mf = cpu->xf = false;
    cpu->spBreakpoint = cpu->sp; g_calling_asm_from_c = true;
    int visit = 0;
    for (int budget = 100000; g_calling_asm_from_c; budget--) {
      if (!budget) { fclose(f); return 6; }
      if (cpu->k == 0x94 && cpu->pc == 0xa3d0)
        fprintf(f, "%d,%d,%d,%04X\n", width, tilex, visit++, cpu->x);
      cpu_runOpcode(cpu);
      while (g_snes->dma->dmaBusy) dma_doDma(g_snes->dma);
    }
  }
  fclose(f); return 0;
}
