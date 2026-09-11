// #516: original CPU draw output across the below-viewport elevator range.
// This isolates drawing, not camera movement, IRQ timing or scene composition.
#include "native-bounded-cpu.h"
#include "snes/ppu.h"
#include "native-spin-door-probe.h"
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

// Independent PPU composition using the SAME captured memory, not a claim that
// the original CPU loaded those tiles. Only numerical pixel witnesses are output.
int DiagnosticElevatorPpu(const char *rom, const char *input, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(input, "rb"); if (!f) return 4;
  Ppu *p = g_snes->ppu; ppu_reset(p);
  uint16 regs[13];
  if (fread(p->vram, 1, sizeof(p->vram), f) != sizeof(p->vram) ||
      fread(p->cgram, 1, sizeof(p->cgram), f) != sizeof(p->cgram) ||
      fread(p->oam, 1, sizeof(p->oam), f) != sizeof(p->oam) ||
      fread(p->highOam, 1, sizeof(p->highOam), f) != sizeof(p->highOam) ||
      fread(regs, 1, sizeof(regs), f) != sizeof(regs) || fgetc(f) != EOF) {
    fclose(f); fprintf(stderr, "Invalid elevator PPU witness size.\n"); return 5;
  }
  fclose(f);
  ppu_write(p, 0x00, regs[12]); ppu_write(p, 0x01, regs[11]);
  ppu_write(p, 0x05, 9); // Mode 1, BG3 priority as in gameplay.
  ppu_write(p, 0x2c, regs[10]);
  p->bgLayer[0].tilemapAdr = 0x5000; p->bgLayer[0].tilemapWider = true;
  p->bgLayer[0].tileAdr = regs[7];
  p->bgLayer[0].hScroll = regs[0]; p->bgLayer[0].vScroll = regs[1];
  p->bgLayer[1].tilemapAdr = regs[6]; p->bgLayer[1].tileAdr = regs[8];
  p->bgLayer[1].tilemapWider = regs[4] == 64; p->bgLayer[1].tilemapHigher = regs[5] == 64;
  p->bgLayer[1].hScroll = regs[2]; p->bgLayer[1].vScroll = regs[3];
  p->bgLayer[2].tilemapAdr = 0x5800; p->bgLayer[2].tileAdr = regs[9];
  uint8 *images = calloc(2, 256 * 224 * 4); if (!images) return 7;
  uint16 originalPalette[16]; memcpy(originalPalette, p->cgram + 192, sizeof(originalPalette));
  f = fopen(output, "wx"); if (!f) { free(images); return 4; }
  fprintf(f, "experiment,x,y\n");
  for (int experiment = 0; experiment < 2; experiment++) {
  // Negative control: compensate the PPU's physical-line offset to emulate the
  // managed compositor's current zero-based BG sampling, without moving OBJ.
  p->bgLayer[0].vScroll = regs[1] - experiment;
  p->bgLayer[1].vScroll = regs[3] - experiment;
  memcpy(p->cgram + 192, originalPalette, sizeof(originalPalette));
  for (int pass = 0; pass < 2; pass++) {
    if (pass) memset(p->cgram + 192, 0, 16 * sizeof(uint16));
    PpuBeginDrawing(p, images + pass * 256 * 224 * 4, 256 * 4, 0);
    ppu_runLine(p, 0);
    for (int line = 1; line <= 224; line++) {
      ppu_write(p, 0x2c, line <= 32 ? 4 : regs[10]);
      ppu_runLine(p, line);
    }
  }
  int count = 0;
  for (int pixel = 0; pixel < 256 * 80; pixel++) {
    if (memcmp(images + pixel * 4, images + 256 * 224 * 4 + pixel * 4, 3)) {
      fprintf(f, "%d,%d,%d\n", experiment, pixel % 256, pixel / 256); count++;
    }
  }
  printf("Independent PPU experiment %d top-edge palette differences: %d\n", experiment, count);
  }
  fclose(f); free(images); return 0;
}

// Execute the cartridge's Green Brinstar destination header and asset loader,
// independently of managed room expansion. Report only diagnostic hashes.
int DiagnosticElevatorRoomAssets(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false;
  door_def_ptr = 0x8ca6; samus_x_pos = 128; samus_y_pos = 16;
  samus_health = samus_max_health = 99;
  collected_items = equipped_items = 4; samus_missiles = 5;
  reg_NMITIMEN = 0x30; fx_y_pos = lava_acid_y_pos = 0xffff;
  SpinDoorRun(0x82de12); // Load destination room/header.
  SpinDoorRun(0x82e36e); // Decompress and install room graphics/level data.
  uint32 hash = 2166136261u;
  uint8 *tile = (uint8 *)g_snes->ppu->vram + 0x338 * 32;
  for (int b = 0; b < 32; b++) hash = (hash ^ tile[b]) * 16777619u;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "block55,tile0338hash\n%04X,%08X\n", level_data[55], hash);
  fclose(f); return 0;
}

int DiagnosticElevatorHandoff(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram)); g_snes->cpu->e = false;
  // Actual managed departure boundary from Green Brinstar's upward ride.
  door_def_ptr = 0x8ca6; samus_x_pos = samus_prev_x_pos = 128;
  samus_y_pos = samus_prev_y_pos = 0xfff8;
  elevator_flags = elevator_status = 1; elevator_direction = 0x8000;
  door_transition_flag_elevator_zebetites = 1;
  samus_health = samus_max_health = 99; collected_items = equipped_items = 4;
  samus_missiles = 5; samus_x_radius = 5; samus_y_radius = 21;
  reg_NMITIMEN = 0x30; fx_y_pos = lava_acid_y_pos = 0xffff;
  uint32 stages[] = {0x82de12, 0x82e353, 0x82e36e, 0x82e38e, 0x82e3c0, 0x82e4a9, 0x82e6a2};
  fprintf(f, "stage,samusY,cameraY,bg1Y,elevatorStatus,elevatorFlags\n");
  for (int i = 0; i < 7; i++) {
    if (i == 5) {
      // Run the real directional dispatcher, including its final camera snap.
      int budget = 100;
      while (!(door_transition_flag & 0x8000) && budget--) SpinDoorRun(0x80ae4e);
      if (!(door_transition_flag & 0x8000)) { fclose(f); return 6; }
    }
    SpinDoorRun(stages[i]);
    fprintf(f, "%06X,%04X,%04X,%04X,%04X,%04X\n", stages[i], samus_y_pos,
      layer1_y_pos, reg_BG1VOFS, elevator_status, elevator_flags);
    fflush(f);
  }
  fclose(f); return 0;
}
