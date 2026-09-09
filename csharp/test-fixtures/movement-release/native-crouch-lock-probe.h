// #451: held opposite direction before/during/after a posture transition.
// Include after native-release-probe.h and dispatch before SDL starts.
int DiagnosticCrouchLock(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "left,ball,offset,repress,frame,input,x,y,pose,movement,animFrame,animTimer,baseSpeed,extraSpeed,xAccel,facing\n");
  for (int left = 0; left < 2; left++)
  for (int ball = 0; ball < 2; ball++)
  for (int offset = -8; offset <= 16; offset++)
  for (int repress = 0; repress < 2; repress++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 144; x++) level_data[16 * 144 + x] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff; equipped_items = 4; samus_health = 99;
    samus_x_pos = samus_prev_x_pos = 1024;
    samus_y_pos = samus_prev_y_pos = ball ? 249 : 235;
    samus_pose = samus_prev_pose = ball ? (left ? 0x41 : 0x1d) : (left ? 2 : 1);
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = ball ? 4 : 0;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    uint16 previous = 0;
    for (int frame = 0; frame < 96; frame++) {
      uint16 input = frame >= 16 + offset && !(repress && frame == 48) ? (left ? 0x100 : 0x200) : 0;
      if (frame == 16) input |= ball ? 0x800 : 0x400;
      samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
      samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      RunAsmCode(0x90ec22, 0, 0, 0, 0); RunAsmCode(0x90e90f, 0, 0, 0, 0);
      RunAsmCode(0x909c5b, 0, 0, 0, 0);
      RunAsmCode(0x900000 | samus_movement_handler, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0); RunAsmCode(0x90dde9, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0); RunAsmCode(0x91eb88, 0, 0, 0, 0);
      RunAsmCode(0x90eab3, 0, 0, 0, 0); RunAsmCode(0x90e9ce, 0, 0, 0, 0);
      RunAsmCode(0xa09169, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%02X,%04X,%04X,%04X%04X,%04X%04X,%04X,%02X\n",
        left,ball,offset,repress,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_movement_type,samus_anim_frame,samus_anim_frame_timer,
        samus_x_base_speed,samus_x_base_subspeed,samus_x_extra_run_speed,samus_x_extra_run_subspeed,
        samus_x_accel_mode,samus_pose_x_dir);
    }
  }
  fclose(f); return 0;
}
