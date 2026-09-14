// #445: obtain the actual Moat collision geometry before choosing a room-local CWJ seed.
#include "native-bounded-cpu.h"
int DiagnosticMoatCwjSearch(const char *rom,const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"firstJump,jump,frame,x,y,extra,movement\n");
  for(int firstJump=0;firstJump<=20;firstJump++) for(int jump=30;jump<=70;jump++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_ptr=0x95ff;
    ProbeRunBounded(0x82de6f); ProbeRunBounded(0x82def2); ProbeRunBounded(0x82ea73);
    // Incoming left doorway is open after transition. The bare decompressed room
    // still contains its four cap blocks; leaving them closed traps the seed.
    for(int y=6;y<=9;y++) level_data[y*room_width_in_blocks+1]=0;
    interactive_enemy_indexes[0]=0xffff;
    fx_y_pos=lava_acid_y_pos=0xffff;
    samus_health=samus_max_health=99;
    samus_x_pos=samus_prev_x_pos=24; samus_y_pos=samus_prev_y_pos=139;
    samus_x_radius=5; samus_y_radius=21;
    samus_pose=samus_prev_pose=9; samus_pose_x_dir=samus_prev_pose_x_dir=8;
    samus_movement_type=samus_prev_movement_type=1;
    samus_x_base_speed=2; samus_x_base_subspeed=0xc000; samus_x_extra_run_speed=2;
    samus_anim_frame_timer=1; samus_x_speed_table_pointer=0x9f55;
    samus_input_handler=0xe913; samus_movement_handler=0xa337; grapple_beam_function=0xc4f0;
    button_config_run_b=0x8000; button_config_jump_a=0x80; button_config_shoot_x=0x40;
    button_config_aim_up_R=0x10; button_config_aim_down_L=0x20;
    button_config_itemcancel_y=0x4000; button_config_itemswitch=0x2000;
    uint16 previous=0;
    for(int frame=0;frame<=jump+8;frame++) {
      nmi_frame_counter_word=nmi_frame_counter_byte=frame+2;
      uint16 input=0x8100 | (frame>=firstJump && frame!=jump-1?0x80:0);
      joypad1_lastkeys=input; joypad1_newkeys=input&~previous; previous=input;
      ProbeRunBounded(0x90e695); ProbeRunBounded(0xa09785);
      samus_contact_damage_index=0;
      ProbeRunBounded(0x900000|samus_movement_handler);
      ProbeRunBounded(0x908000); ProbeRunBounded(0x90dde9);
      ProbeRunBounded(0x91e8b6); ProbeRunBounded(0x91eb88);
      ProbeRunBounded(0x90eab3); ProbeRunBounded(0x90e9ce); ProbeRunBounded(0xa09169);
      if(samus_movement_type==20) {
        fprintf(f,"%d,%d,%d,%04X%04X,%04X%04X,%04X%04X,%u\n",firstJump,jump,frame,
          samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
          samus_x_extra_run_speed,samus_x_extra_run_subspeed,samus_movement_type);
        if(samus_movement_type==20) break;
      }
    }
  }
  fclose(f); return 0;
}
int DiagnosticMoatCwjGeometry(const char *rom,const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
  g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
  room_ptr=0x95ff;
  ProbeRunBounded(0x82de6f); ProbeRunBounded(0x82def2); ProbeRunBounded(0x82ea73);
  fprintf(f,"width=%u,height=%u\n",room_width_in_blocks,room_height_in_blocks);
  for(int y=0;y<room_height_in_blocks;y++) {
    fprintf(f,"%02d ",y);
    for(int x=0;x<room_width_in_blocks;x++) fprintf(f,"%X",level_data[y*room_width_in_blocks+x]>>12);
    fprintf(f,"\n");
  }
  fclose(f); return 0;
}
