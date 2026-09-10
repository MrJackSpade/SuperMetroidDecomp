#include "native-bounded-cpu.h"

// Entire alpha HUD/cooldown/projectile dispatcher. Poses are externally scripted
// to isolate admission from movement physics; no enemy or nonempty terrain.
int DiagnosticComboInput(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"beam,ammo,script,frame,charge,previous,pb,hud,count,cooldown,types,bombs,bomb_types\n");
  for(int beam=0;beam<12;beam++) for(int ammo=0;ammo<3;ammo++) for(int script=0;script<5;script++) {
    if(script==4 && (ammo!=2 || !(beam==1||beam==2||beam==4||beam==8))) continue;
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=16; room_height_in_blocks=32; interactive_enemy_indexes[0]=0xffff;
    room_width_in_scrolls=1; room_height_in_scrolls=2;
    samus_x_pos=samus_y_pos=128; samus_x_radius=5; samus_y_radius=16; samus_pose_x_dir=8;
    equipped_beams=0x1000|beam; samus_power_bombs=ammo; hud_item_index=3;
    equipped_items=0x1004;
    button_config_shoot_x=0x40; grapple_beam_function=0xc4f0; flare_counter=119;
    for(int frame=0;frame<160;frame++) {
      samus_pose=script==1&&frame<20?0x19:script==2&&frame<20?0x25:1;
      samus_movement_type=script==1&&frame<20?3:script==2&&frame<20?14:0;
      joypad1_lastkeys=script==3&&frame==0?0:0x40;
      joypad1_newkeys=(script==3?frame==1:frame==0)?0x40:0;
      if(script==4 && frame>=2) {
        samus_pose=0x1d; samus_movement_type=4; hud_item_index=0;
        joypad1_lastkeys=joypad1_newkeys=(frame&1)?0:0x40;
      }
      sfx_readpos[0]=sfx_writepos[0]=0; memset(sfx1_queue,0,16);
      ProbeRunBounded(0x90dcdd);
      fprintf(f,"%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,",beam,ammo,script,frame,
        flare_counter,prev_beam_charge_counter,samus_power_bombs,hud_item_index,projectile_counter,cooldown_timer);
      for(int i=0;i<5;i++) fprintf(f,"%04X",projectile_type[i]);
      fprintf(f,",%04X,",bomb_counter);
      for(int i=5;i<10;i++) fprintf(f,"%04X",projectile_type[i]);
      fprintf(f,"\n");
    }
  }
  fclose(f); return 0;
}
