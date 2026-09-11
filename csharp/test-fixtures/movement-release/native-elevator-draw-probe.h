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

// Arrival-only actor + scrolling comparison. Room geometry and actor spawn are
// from $8F:97B5/$A1:8B61; this does not execute the preceding door coroutine.
int DiagnosticElevatorArrival(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
  room_width_in_scrolls = room_height_in_scrolls = 1;
  up_scroller = 0x70; down_scroller = 0xa0; scrolls[0] = 1;
  layer1_y_pos = 32; elevator_status = 2; elevator_flags = 0x80;
  elevator_direction = 0x8000;
  Enemy_Elevator *E = Get_Elevator(0);
  E->base.x_pos = 128; E->base.y_pos = 162;
  E->elevat_parameter_2 = 320;
  ProbeRunBounded(0xa394e6); // Elevator_Init pins Samus to parameter-2 position.
  samus_prev_x_pos = samus_x_pos; samus_prev_y_pos = samus_y_pos;
  fprintf(f, "frame,samusY,cameraY,cameraSubY,status\n");
  for (int frame = 0; frame < 100; frame++) {
    ProbeRunBounded(0xa3952a); // Elevator_Frozen / active-state dispatcher.
    ProbeRunBounded(0x9094ec); // MainScrollingRoutine, including prior-position publication.
    fprintf(f, "%d,%d,%d,%d,%d\n", frame, samus_y_pos,
      layer1_y_pos, layer1_y_subpos, elevator_status);
  }
  fclose(f); return 0;
}
