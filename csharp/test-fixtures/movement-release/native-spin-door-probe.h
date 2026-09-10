// #517: execute the retail Flyway -> Bomb Torizo loading stages. This is a
// CPU-state probe, NOT a rendered-frame or audio-timing comparison. Complete
// queued VRAM requests and vblank waits at their polling boundaries; never
// alter Samus velocity to make a stage terminate.
#include "snes/dma.h"
extern bool g_calling_asm_from_c;
static void SpinDoorRun(uint32 address) {
  Cpu *cpu = g_snes->cpu;
  cpu->db = cpu->k = address >> 16; cpu->pc = address;
  cpu->a = cpu->x = cpu->y = 0; cpu->mf = cpu->xf = false;
  cpu->sp = cpu->spBreakpoint = 0x1ff0; cpu->dp = 0;
  g_ram[0x1ffff] = 1; g_calling_asm_from_c = true;
  for (int budget = 10000000; g_calling_asm_from_c; budget--) {
    uint32 pc = (uint32)cpu->k << 16 | cpu->pc;
    if (!budget) { fprintf(stderr, "Spin door budget: %06X entry %06X\n", pc, address); exit(6); }
    if (pc == 0x8082c9) g_snes->inVblank = true;
    if (pc == 0x8082ce) g_snes->inVblank = false;
    if (pc == 0x808343) waiting_for_nmi = 0;
    // DoorTransitionVRAM_Flag ($05BC) is cleared by the IRQ after transfer.
    // Pixel contents are deliberately outside this register-lifetime probe.
    if (pc == 0x82e02c || pc == 0x82e06b || pc == 0x82e50d || pc == 0x82e609)
      *(uint16 *)(g_ram + 0x5bc) = 0;
    cpu_runOpcode(cpu);
    while (g_snes->dma->dmaBusy) dma_doDma(g_snes->dma);
  }
}
int DiagnosticSpinDoor(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "stage,x,y,base,extra,yspeed,ydir,pose\n");
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false;
  door_def_ptr = 0x8bc2; layer1_x_pos = 512;
  samus_x_pos = samus_prev_x_pos = 760;
  samus_y_pos = samus_prev_y_pos = 128;
  samus_pose = samus_prev_pose = 0x19;
  samus_movement_type = samus_prev_movement_type2 = 3;
  samus_pose_x_dir = samus_prev_pose_x_dir = 8;
  samus_x_radius = 5; samus_y_radius = 12;
  samus_x_base_speed = 1; samus_x_base_subspeed = 0x6000;
  samus_has_momentum_flag = 1; samus_x_extra_run_speed = 2;
  samus_y_speed = 2; samus_y_subspeed = 0x3456; samus_y_dir = 2;
  samus_health = samus_max_health = 99;
  samus_anim_frame_timer = 1; samus_x_accel_mode = 2;
  random_number = 9; reg_NMITIMEN = 0x30;
  fx_y_pos = lava_acid_y_pos = 0xffff;
  // Header, complete room asset/scroll loading, IRQ scrolling setup, placement
  // and tiles, all remaining room actors/PLMs, and the final narrow-door nudge.
  uint32 stages[] = {0x82de12, 0x82e36e, 0x82e38e, 0x82e3c0, 0x82e4a9, 0x82e6a2};
  for (int i = 0; i < 6; i++) {
    if (i == 4) {
      for (int scroll = 1; scroll < 64; scroll++) SpinDoorRun(0x80ae7e);
      // The enclosing IRQ performs this completion publication after the last
      // directional call; invoke that publication without another position step.
      door_transition_flag |= 0x8000;
    }
    SpinDoorRun(stages[i]);
    fprintf(f, "%06X,%04X%04X,%04X%04X,%04X%04X,%04X%04X,%04X%04X,%04X,%04X\n",
      stages[i],samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
      samus_x_base_speed,samus_x_base_subspeed,samus_x_extra_run_speed,samus_x_extra_run_subspeed,
      samus_y_speed,samus_y_subspeed,samus_y_dir,samus_pose);
    fflush(f);
  }
  samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
  grapple_beam_function = 0xc4f0; samus_x_speed_table_pointer = 0x9f55;
  button_config_run_b = 0x8000; button_config_jump_a = 0x80;
  button_config_shoot_x = 0x40; button_config_aim_up_R = 0x10;
  button_config_aim_down_L = 0x20;
  for (int frame = 0; frame < 40; frame++) {
    nmi_frame_counter_word = nmi_frame_counter_byte = frame + 2;
    joypad1_lastkeys = joypad1_newkeys = 0;
    uint32 movement[] = {0x90e695,0xa09785,0x90a337,0x908000,
      0x90dde9,0x91e8b6,0x91eb88,0x90eab3,0x90e9ce,0xa09169};
    for (int j = 0; j < 10; j++) SpinDoorRun(movement[j]);
    fprintf(f, "F%05d,%04X%04X,%04X%04X,%04X%04X,%04X%04X,%04X%04X,%04X,%04X\n",
      frame,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
      samus_x_base_speed,samus_x_base_subspeed,samus_x_extra_run_speed,samus_x_extra_run_subspeed,
      samus_y_speed,samus_y_subspeed,samus_y_dir,samus_pose);
  }
  fclose(f); return 0;
}
