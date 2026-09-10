// #516: original CPU draw output across the below-viewport elevator range.
// This isolates drawing, not camera movement, IRQ timing or scene composition.
#include "native-bounded-cpu.h"
int DiagnosticElevatorDraw(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "worldY,cameraY,count,lowOam,highOam\n");
  for (int worldY = 256; worldY <= 294; worldY++)
  for (int cameraY = 0; cameraY <= 32; cameraY += 2) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    samus_x_pos = 128; samus_y_pos = worldY; layer1_y_pos = cameraY;
    // Pose/frame zero is the observed forward-facing elevator pose. Execute
    // Samus_DrawWhenNotAnimatingOrDying, the even-NMI elevator draw target.
    ProbeRunBounded(0x908a00);
    fprintf(f, "%d,%d,%d,", worldY, cameraY, oam_next_ptr / 4);
    for (int b = 0; b < oam_next_ptr; b++) fprintf(f, "%02X", g_ram[0x370 + b]);
    fputc(',', f);
    for (int b = 0; b < 32; b++) fprintf(f, "%02X", g_ram[0x570 + b]);
    fputc('\n', f);
  }
  fclose(f); return 0;
}
