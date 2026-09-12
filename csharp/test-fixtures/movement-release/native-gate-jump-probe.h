#include "native-bounded-cpu.h"

// Original CPU counterpart of GateGlitchRoomAudit.RunJump, without renderer/UI.
int DiagnosticGateJump(const char *rom, const char *output, int shootFrame, int aimFrame, int releaseLeft) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
  room_width_in_blocks = 32; room_height_in_blocks = 48;
  room_width_in_scrolls = 2; room_height_in_scrolls = 3;
  room_size_in_blocks = 3072; area_index = 2;
  const uint8 *state = RomFixedPtr(0x8fae81);
  uint32 level = state[0] | state[1] << 8 | state[2] << 16;
  uint16 population = state[20] | state[21] << 8;
  DecompressToMem(level, g_ram + 0x10000);
  memcpy(BTS, (uint8 *)level_data + 3072, 1536);
  interactive_enemy_indexes[0] = 0xffff;
  samus_pose = samus_prev_pose = 6; samus_pose_x_dir = samus_prev_pose_x_dir = 4;
  samus_x_pos = samus_prev_x_pos = 140; samus_y_pos = samus_prev_y_pos = 379;
  samus_x_radius = 5; samus_y_radius = 21;
  samus_x_speed_table_pointer = 0x9f55; samus_input_handler = 0xe913;
  samus_anim_frame_timer = 5; samus_health = 99;
  hud_item_index = 1; samus_missiles = 10;
  button_config_shoot_x = 0x40; button_config_run_b = 0x8000; button_config_jump_a = 0x80;
  button_config_up = 0x800; button_config_down = 0x400;
  button_config_left = 0x200; button_config_right = 0x100;
  button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
  button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
  fx_y_pos = lava_acid_y_pos = 0xffff;
  layer1_y_pos = 224; plm_flag = 0x8000;
  for (uint16 entry = population; ; entry += 6) {
    uint32 address = 0x8f0000 | entry;
    if (!GET_WORD(RomFixedPtr(address))) break;
    ProbeRunBoundedRegisters(0x84846a, 0, entry, 0);
  }
  int gate = -1;
  for (int i = 0; i < 40; i++) if (plm_header_ptr[i] == 0xc82a) gate = i;
  if (gate < 0) { fclose(f); return 8; }
  uint16 previous = 0;
  fprintf(f, "frame,input,x,subx,y,pose,gateTimer,gateInstruction\n");
  for (int frame = -60; frame < 80; frame++) {
    uint16 input = frame < 0 ? 0 : 0x280;
    if (aimFrame > 0 && frame == -1) input = 0x200;
    if (frame >= aimFrame) input |= 0x10;
    if (releaseLeft && frame >= aimFrame) input &= ~0x200;
    if (frame == shootFrame) input |= 0x40;
    joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
    samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
    samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
    ProbeRunBounded(0x90ec22); ProbeRunBounded(0x918000); ProbeRunBounded(0x909c5b);
    ProbeRunBounded(0x90ac1c); ProbeRunBounded(0x90be62);
    ProbeRunBounded(0x90aece); ProbeRunBounded(0x90eb02);
    ProbeRunBounded(0x90a337); ProbeRunBounded(0x908000);
    ProbeRunBounded(0x91e8b6); ProbeRunBounded(0x91eb88);
    ProbeRunBounded(0x90eab3); ProbeRunBounded(0x8485b4);
    fprintf(f, "%d,%04X,%d,%d,%d,%02X,%d,%04X\n", frame, input,
      samus_x_pos, samus_x_subpos, samus_y_pos, samus_pose, plm_timers[gate], plm_instr_list_ptrs[gate]);
    vram_write_queue_tail = 0;
  }
  fclose(f); return 0;
}
