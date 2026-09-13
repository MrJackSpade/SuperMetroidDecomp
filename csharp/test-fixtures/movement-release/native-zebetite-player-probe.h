// #443: first native consumer for the narrowly specified isolated candidate.
// Uses MOV1 room/movement seed plus the documented fixed candidate metadata.
// Live FX/PLM equivalence and full state import remain to be audited.
static int DiagnosticZebetitePlayerRun(const char *rom, const char *seed_path, const char *output, int second_jump_delay, const uint8 *inputs, int frame_count, const char *projectile_seed) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  size_t size = 0; uint8 *seed = ReadWholeFile(seed_path, &size);
  if (!seed || size != 3200) { free(seed); return 4; }
  uint32 *w = (uint32 *)seed;
  if (w[0] != 0x31564f4d || w[1] != 0xdd58 || w[2] != 64 || w[3] != 16 || w[6] != 1) { free(seed); return 5; }
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
  room_ptr = w[1]; room_width_in_blocks = w[2]; room_height_in_blocks = w[3];
  room_size_in_blocks = w[2] * w[3] * 2;
  for (uint32 i = 0; i < w[2] * w[3]; i++) {
    level_data[i] = seed[128 + i * 3] | seed[129 + i * 3] << 8;
    BTS[i] = seed[130 + i * 3];
  }
  interactive_enemy_indexes[0] = 0xffff;
  samus_x_pos = w[4] >> 16; samus_x_subpos = w[4];
  samus_y_pos = w[5] >> 16; samus_y_subpos = w[5];
  samus_pose = samus_prev_pose = w[6];
  samus_x_radius = w[7]; samus_y_radius = w[8];
  samus_x_base_speed = w[9] >> 16; samus_x_base_subspeed = w[9];
  samus_x_extra_run_speed = w[10] >> 16; samus_x_extra_run_subspeed = w[10];
  samus_x_accel_mode = w[11]; samus_has_momentum_flag = w[12];
  samus_x_speed_divisor = w[13]; samus_x_decel_mult = w[14];
  equipped_items = w[15]; equipped_beams = w[16];
  fx_type = w[17]; fx_y_pos = w[18]; lava_acid_y_pos = w[19];
  fx_liquid_options = w[20]; liquid_physics_type = w[21];
  samus_anim_frame = w[22]; samus_anim_frame_timer = w[23]; samus_anim_frame_buffer = w[24];
  samus_y_speed = w[25] >> 16; samus_y_subspeed = w[25]; samus_y_dir = w[26];
  samus_total_x_speed = w[27] >> 16; samus_total_x_subspeed = w[27];
  extra_samus_x_displacement = w[28] >> 16; extra_samus_x_subdisplacement = w[28];
  extra_samus_y_displacement = w[29] >> 16; extra_samus_y_subdisplacement = w[29];
  samus_y_accel = w[30]; samus_y_subaccel = w[31];

  free(seed);
  room_width_in_scrolls = 4; room_height_in_scrolls = 1;
  for (int i = 0; i < 4; i++) scrolls[i] = 1;
  samus_pose_x_dir = samus_prev_pose_x_dir = samus_last_different_pose_x_dir = 8;
  samus_last_different_pose = 1;
  samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = 0;
  samus_prev_x_pos = samus_x_pos; samus_prev_x_subpos = samus_x_subpos;
  samus_prev_y_pos = samus_y_pos; samus_prev_y_subpos = samus_y_subpos;
  samus_health = samus_max_health = 999; samus_missiles = samus_max_missiles = 10; hud_item_index = 1;
  samus_x_speed_table_pointer = 0x9f55; samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
  grapple_beam_function = 0xc4f0;
  button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
  button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
  button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
  layer1_x_pos = ideal_layer1_xpos = 641;
  events_that_happened[0] = 8; first_free_enemy_index = 448; enemy_index_to_shake = 0xffff;
  for (int part = 0; part < 2; part++) {
    uint16 index = part ? 384 : 128;
    EnemyData *e = gEnemyData(index); EnemyDef *d = get_EnemyDef_A2(0xe27f);
    e->enemy_ptr = 0xe27f; e->bank = 0xa6; e->health = 1000;
    e->x_width = d->x_radius; e->y_height = d->y_radius; e->layer = d->layer;
    e->properties = 0x2000; e->parameter_1 = part ? 2 : 0;
    cur_enemy_index = index; RunAsmCode(0xa6fb72, 0, index, 0, 0);
    Get_Zebetites(index)->zebet_parameter_2 = part ? 128 : 384;
    Get_Zebetites(index)->zebet_var_A = 0xfc67;
  }
  if (projectile_seed) {
    size_t epj_size = 0; uint8 *epj = ReadWholeFile(projectile_seed, &epj_size);
    if (!epj || epj_size != 656 || memcmp(epj, "EPJ1", 4) || epj[6] != 18 || epj[7]) { free(epj); return 10; }
    int turret_count = 0;
    for (int slot = 0; slot < 18; slot++) {
      int offset = 8 + slot * 36;
      uint16 id = epj[offset] | epj[offset + 1] << 8;
      if (id == 0xc17e) turret_count++;
      else if (id) { free(epj); return 11; }
    }
    if (turret_count != 12) { free(epj); return 12; }
    random_number = epj[4] | epj[5] << 8;
    // Transpose slot-major snapshot words into the eighteen contiguous WRAM arrays.
    for (int slot = 0; slot < 18; slot++)
      for (int field = 0; field < 18; field++) {
        int src = 8 + (slot * 18 + field) * 2;
        int dst = 0x1997 + field * 36 + slot * 2;
        g_ram[dst] = epj[src]; g_ram[dst + 1] = epj[src + 1];
      }
    free(epj); eproj_enable_flag = 0x8000;
  }
  FILE *f = fopen(output, "w"); if (!f) return 6;
  fprintf(f, "frame,input,x,y,pose,anim,timer,cameraX,cameraY,missiles,shotType,shotX,shotY,upper,lower,health,upperFlash,lowerFlash,upperAi,lowerAi,random,upperId,lowerId,generation\n");
  uint16 previous = 0;
  for (int frame = 60; frame < 60 + frame_count; frame++) {
    uint16 input = frame == 60 || frame == 61 ? 0x400 : frame == 66 ? 0x200 : frame == 78 ? 0x40 : frame == 80 ? 0x280 : frame >= 81 && frame < 99 ? 0x180 : 0;
    if (second_jump_delay) {
      input = frame == 60 || frame == 61 || frame == 280 || frame == 281 ? 0x400 :
        frame == 66 || frame == 286 ? 0x200 : frame == 78 || frame == 292 ? 0x40 :
        frame == 80 ? 0x280 : frame >= 81 && frame < 105 ? 0x180 :
        frame >= 150 && frame < 210 ? 0x80 : frame == 250 ? 0x100 : 0;
      if (frame >= 151 && frame < 162) input |= 0x200;
      int jump = 300 + second_jump_delay;
      if (frame >= jump && frame < jump + 3) input |= 0x280;
      else if (frame >= jump + 3 && frame < jump + 21) input |= 0x180;
    }
    if (inputs) {
      int offset = (frame - 60) * 2;
      input = inputs[offset] | inputs[offset + 1] << 8;
    }
    joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
    // Keep the word counter independent of the wrapping byte after frame 253.
    nmi_frame_counter_word = frame + 2;
    nmi_frame_counter_byte = (uint8)(frame + 2);
    memset(enemy_drawing_queue_sizes, 0, 16);
    if (projectile_seed) RunAsmCode(0x808111, 0, 0, 0, 0);
    RunAsmCode(0xa08eb6, 0, 0, 0, 0);
    RunAsmCode(0x90e695, 0, 0, 0, 0); RunAsmCode(0xa09785, 0, 0, 0, 0);
    RunAsmCode(0xa08fd4, 0, 0, 0, 0);
    samus_contact_damage_index = 0;
    RunAsmCode(0x900000 | samus_movement_handler, 0, 0, 0, 0);
    uint32 stages[] = {0x908000,0x90dde9,0x91e8b6,0x91eb88,0x90eab3,0x90e9ce};
    for(int i=0;i<6;i++) RunAsmCode(stages[i],0,0,0,0);
    if (projectile_seed) {
      RunAsmCode(0x868104, 0, 0, 0, 0);
      RunAsmCode(0xa09894, 0, 0, 0, 0);
      RunAsmCode(0xa0996c, 0, 0, 0, 0);
    }
    RunAsmCode(0x9094ec,0,0,0,0); RunAsmCode(0xa09169,0,0,0,0);
    fprintf(f,"%d,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u\n",frame,input,
      ((uint32)samus_x_pos<<16)|samus_x_subpos,((uint32)samus_y_pos<<16)|samus_y_subpos,
      samus_pose,samus_anim_frame,samus_anim_frame_timer,layer1_x_pos,layer1_y_pos,samus_missiles,
      projectile_type[0],projectile_x_pos[0],projectile_y_pos[0],gEnemyData(128)->health,gEnemyData(384)->health,
      samus_health,gEnemyData(128)->flash_timer,gEnemyData(384)->flash_timer,gEnemyData(128)->ai_handler_bits,gEnemyData(384)->ai_handler_bits,random_number,
      gEnemyData(128)->enemy_ptr,gEnemyData(384)->enemy_ptr,events_that_happened[0] & 0x38);
  }
  fclose(f); return 0;
}

int DiagnosticZebetitePlayer(const char *rom, const char *seed, const char *output) {
  return DiagnosticZebetitePlayerRun(rom, seed, output, 0, NULL, 60, NULL);
}

int DiagnosticZebetitePlayerSecondHit(const char *rom, const char *seed, const char *output, int jump_delay) {
  if (jump_delay != 3 && jump_delay != 4) return 7;
  return DiagnosticZebetitePlayerRun(rom, seed, output, jump_delay, NULL, 360, NULL);
}

int DiagnosticZebetitePlayerInputsWithProjectiles(const char *rom, const char *seed, const char *output, const char *input_path, const char *projectile_seed) {
  size_t size = 0;
  uint8 *data = ReadWholeFile(input_path, &size);
  if (!data || size < 12 || memcmp(data, "ZBI1", 4)) { free(data); return 8; }
  uint32 *header = (uint32 *)data;
  if (header[1] != 60 || header[2] == 0 || header[2] > 10000 || size != 12 + header[2] * 2) {
    free(data); return 9;
  }
  int result = DiagnosticZebetitePlayerRun(rom, seed, output, 0, data + 12, header[2], projectile_seed);
  free(data);
  return result;
}

int DiagnosticZebetitePlayerInputs(const char *rom, const char *seed, const char *output, const char *input_path) {
  return DiagnosticZebetitePlayerInputsWithProjectiles(rom, seed, output, input_path, NULL);
}
