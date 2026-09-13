// #441: original-CPU Power Bomb production, HDMA, enemy damage and pickup sequence.
// Include after native-release-probe.h; all binary inputs/outputs remain private.
int DiagnosticMetroidFarming(const char *rom, const char *seed_path, const char *output, int random_seed, int shots) {
  if (random_seed < 1 || random_seed > 65535 || (shots != 2 && shots != 3)) return 4;
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  size_t size = 0; uint8 *seed = ReadWholeFile(seed_path, &size);
  if (!seed || size < 128) { free(seed); return 5; }
  uint32 *w = (uint32 *)seed;
  if (w[0] != 0x31564f4d || w[1] != 0x91f8 || w[2] != 144 || w[3] != 80 ||
      size != 128 + w[2] * w[3] * 3 || w[6] != 0x1d || w[15] != 0x24 || w[16]) { free(seed); return 6; }
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
  room_ptr = w[1]; room_width_in_blocks = w[2]; room_height_in_blocks = w[3]; room_size_in_blocks = w[2] * w[3] * 2;
  room_width_in_scrolls = 9; room_height_in_scrolls = 5;
  memcpy(scrolls, RomFixedPtr(0x8f9283), 50);
  up_scroller = RomFixedPtr(0x8f91f8)[6]; down_scroller = RomFixedPtr(0x8f91f8)[7];
  for (uint32 i = 0; i < w[2] * w[3]; i++) {
    level_data[i] = seed[128 + i * 3] | seed[129 + i * 3] << 8; BTS[i] = seed[130 + i * 3];
  }
  samus_x_pos = samus_prev_x_pos = w[4] >> 16; samus_x_subpos = w[4];
  samus_y_pos = samus_prev_y_pos = w[5] >> 16; samus_y_subpos = w[5];
  samus_pose = samus_prev_pose = samus_last_different_pose = w[6]; samus_x_radius = w[7]; samus_y_radius = w[8];
  samus_pose_x_dir = samus_prev_pose_x_dir = samus_last_different_pose_x_dir = 8;
  samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = 4;
  equipped_items = w[15]; equipped_beams = w[16]; fx_type = w[17]; fx_y_pos = w[18]; lava_acid_y_pos = w[19];
  fx_liquid_options = w[20]; liquid_physics_type = w[21]; samus_anim_frame = w[22]; samus_anim_frame_timer = w[23];
  samus_anim_frame_buffer = w[24]; free(seed);
  samus_x_speed_table_pointer = 0x9f55; samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
  samus_health = samus_max_health = 399; samus_power_bombs = samus_max_power_bombs = 5;
  samus_missiles = samus_max_missiles = samus_super_missiles = samus_max_super_missiles = 10; hud_item_index = 3;
  button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
  first_free_enemy_index = 64; enemy_index_to_shake = 0xffff;
  Enemy_Metroid *e = Get_Metroid(0);
  e->base.enemy_ptr = 0xdd7f; e->base.bank = 0xa3; e->base.x_pos = samus_x_pos; e->base.y_pos = samus_y_pos - 8;
  e->base.x_width = e->base.y_height = 10; e->base.health = 500; e->base.properties = 0x2000;
  e->base.instruction_timer = 1; e->base.spritemap_pointer = 0x804d; e->base.layer = 5;
  cur_enemy_index = 0; RunAsmCode(0xa3ea4f, 0, 0, 0, 0);
  random_number = random_seed; eproj_enable_flag = hdma_objects_enable_flag = 0x8000;
  FILE *f = fopen(output, "w"); if (!f) return 7;
  fprintf(f,"frame,input,x,y,pose,health,ammo,ehp,header,ex,ey,invincible,killed,rng,pbflag,pbstatus,pbx,pby,preRadius,radius,speed");
  for(int i=0;i<18;i++) fprintf(f,",kind%d,px%d,py%d,list%d,timer%d,pre%d,value%d,header%d",i,i,i,i,i,i,i,i);
  fprintf(f,",srx,sry,prx,pry\n"); uint16 previous = 0;
  for(int frame=0;frame<1050;frame++) {
    uint16 input = frame>=5 && frame<5+shots*345 && (frame-5)%345==0 ? 0x40 : 0;
    if (frame==850) input |= 0x800;
    if (frame>=855 && frame<885) input |= 0x80;
    joypad1_lastkeys=input; joypad1_newkeys=input & ~previous; previous=input;
    nmi_frame_counter_word=frame+1; nmi_frame_counter_byte=(uint8)(frame+1);
    RunAsmCode(0x8884b9,0,0,0,0); RunAsmCode(0x808111,0,0,0,0);
    memset(enemy_drawing_queue_sizes,0,16);
    RunAsmCode(0xa08eb6,0,0,0,0); RunAsmCode(0x90e695,0,0,0,0); RunAsmCode(0xa09785,0,0,0,0); RunAsmCode(0xa08fd4,0,0,0,0);
    samus_contact_damage_index=0; RunAsmCode(0x900000|samus_movement_handler,0,0,0,0);
    uint32 stages[]={0x908000,0x90dde9,0x91e8b6,0x91eb88,0x90eab3,0x90e9ce,0x868104,0xa09894,0xa0996c,0xa0a306,0x9094ec,0xa09169,0xa08687};
    for(int i=0;i<13;i++) RunAsmCode(stages[i],0,0,0,0);
    fprintf(f,"%d,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u",frame,input,
      ((uint32)samus_x_pos<<16)|samus_x_subpos,((uint32)samus_y_pos<<16)|samus_y_subpos,samus_pose,samus_health,samus_power_bombs,
      e->base.health,e->base.enemy_ptr,e->base.x_pos,e->base.y_pos,e->base.invincibility_timer,num_enemies_killed_in_room,random_number,
      power_bomb_flag,power_bomb_explosion_status,power_bomb_explosion_x_pos,power_bomb_explosion_y_pos,power_bomb_pre_explosion_flash_radius,power_bomb_explosion_radius,power_bomb_pre_explosion_radius_speed);
    for(int i=0;i<18;i++) fprintf(f,",%u,%u,%u,%u,%u,%u,%u,%u",eproj_id[i],eproj_x_pos[i],eproj_y_pos[i],eproj_instr_list_ptr[i],eproj_instr_timers[i],eproj_pre_instr[i],eproj_E[i],eproj_enemy_header_ptr[i]);
    fprintf(f,",%u,%u,%u,%u\n",samus_x_radius,samus_y_radius,eproj_radius[17]&255,eproj_radius[17]>>8);
  }
  fclose(f); return 0;
}
