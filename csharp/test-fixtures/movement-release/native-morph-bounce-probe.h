// #449: deterministic impact-boundary fixture. Include after native-release-probe.h.
int DiagnosticMorphBounceVariant(const char *rom, const char *output, int timing) {
  int run_jump = timing == 2;
  int wide = timing != 0;
  timing = timing == 1;
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  const uint32 speeds[] = { 0, 0x1ffff, 0x2c7ff, 0x2e3ff, 0x2e400, 0x2ffff, 0x30000, 0x50000 };
  const uint32 carries[] = { 0, 0x14000, 0x30000, 0x50000, 0x4000, 0xc000, 0x14000, 0x20000 };
  fprintf(f, "left,speed,carry,inputMode,frame,input,x,y,pose,bounce,ySpeed,yDirection,baseSpeed,extraSpeed,xAccel,animFrame,animTimer\n");
  for (int left = 0; left < 2; left++)
  for (int speed = 0; speed < (run_jump ? 16 : timing ? 9 : 8); speed++)
  for (int carry = 0; carry < (wide ? 4 : 8); carry++)
  for (int held = 0; held < 2; held++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    int grounded = timing && speed == 8;
    int width = wide ? 144 : 16;
    room_width_in_blocks = width; room_height_in_blocks = wide ? 80 : 32;
    room_width_in_scrolls = wide ? 9 : 1; room_height_in_scrolls = wide ? 5 : 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < width; x++) level_data[16 * width + x] = 0x8000;
    for (int y = 0; y <= 16; y++) level_data[y * width] = level_data[y * width + width - 1] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff; equipped_items = 4; samus_health = 99;
    samus_x_pos = samus_prev_x_pos = run_jump ? 1024 : wide ? 512 : 128;
    samus_y_pos = samus_prev_y_pos = run_jump ? 235 : timing && !grounded ? 180 : 249;
    samus_pose = samus_prev_pose = left ? 0x32 : 0x31;
    if (timing) samus_pose = samus_prev_pose = grounded ? (left ? 0x41 : 0x1d) : (left ? 0x2a : 0x29);
    if (run_jump) samus_pose = samus_prev_pose = left ? 2 : 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = run_jump ? 0 : timing ? (grounded ? 4 : 6) : 8;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
    uint32 vertical = run_jump ? 0 : timing ? (grounded ? 0 : 0x18000) : speeds[speed];
    samus_y_speed = vertical >> 16; samus_y_subspeed = vertical; samus_y_dir = grounded || run_jump ? 0 : 2;
    uint32 base = run_jump ? 0 : timing ? 0x14000 : carry < 4 ? carries[carry] : 0x14000;
    samus_x_base_speed = base >> 16; samus_x_base_subspeed = base;
    if (timing) samus_has_momentum_flag = carry != 0;
    if (timing || carry >= 4) {
      const uint32 extra[] = { 0, 0xc000, 0x14000, 0x20000 };
      uint32 value = timing ? extra[carry] : carries[carry];
      samus_x_extra_run_speed = value >> 16;
      samus_x_extra_run_subspeed = value;
    }
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    uint16 previous = 0;
    for (int frame = 0; frame < (run_jump ? 180 : 96); frame++) {
      uint16 input = held ? 0x80 | (left ? 0x200 : 0x100) : 0;
      if (timing) input = (grounded || frame >= speed * 2 + 8 ? (left ? 0x200 : 0x100) : 0) | (held ? 0x80 : 0) |
        (!grounded && (frame == speed * 2 || (frame >= speed * 2 + 2 && frame < speed * 2 + 8)) ? 0x400 : 0);
      if (run_jump) {
        const int run_frames[] = { 8, 16, 24, 40 };
        int launch = run_frames[carry], morph = launch + 8 + speed * 2;
        int forward = left ? 0x200 : 0x100;
        int jump = held || frame < launch + 8 ? 0x80 : 0;
        int down = frame == morph || (frame >= morph + 2 && frame < morph + 8) ? 0x400 : 0;
        input = frame < launch ? 0x8000 | forward : frame == launch ? jump | 0x800 :
          jump | down | (frame >= morph && frame < morph + 8 ? 0 : forward);
      }
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
      fprintf(f, "%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%04X,%04X%04X,%04X,%04X%04X,%04X%04X,%04X,%04X,%04X\n",
        left,speed,carry,held,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,used_for_ball_bounce_on_landing,samus_y_speed,samus_y_subspeed,samus_y_dir,
        samus_x_base_speed,samus_x_base_subspeed,samus_x_extra_run_speed,samus_x_extra_run_subspeed,
        samus_x_accel_mode,samus_anim_frame,samus_anim_frame_timer);
    }
  }
  fclose(f); return 0;
}
int DiagnosticMorphBounce(const char *rom, const char *output) { return DiagnosticMorphBounceVariant(rom, output, 0); }
int DiagnosticMorphTiming(const char *rom, const char *output) { return DiagnosticMorphBounceVariant(rom, output, 1); }
int DiagnosticRunJumpMorph(const char *rom, const char *output) { return DiagnosticMorphBounceVariant(rom, output, 2); }
