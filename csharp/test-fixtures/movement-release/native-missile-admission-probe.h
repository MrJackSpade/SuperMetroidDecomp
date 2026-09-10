#include "native-bounded-cpu.h"

// Shared firing boundary relevant to a combo occupying four ordinary slots.
int DiagnosticMissileAdmission(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"selected,count,cooldown,ammo,edge,result_count,result_cooldown,missiles,supers,types\n");
  const int cooldowns[]={0,1,2,256};
  for(int selected=1;selected<=2;selected++) for(int count=3;count<=5;count++)
  for(int c=0;c<4;c++) for(int ammo=0;ammo<2;ammo++) for(int edge=0;edge<2;edge++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    samus_pose=1; samus_pose_x_dir=8; samus_x_pos=samus_y_pos=128;
    samus_x_radius=5; samus_y_radius=16; hud_item_index=selected;
    samus_missiles=samus_super_missiles=ammo; button_config_shoot_x=0x40;
    joypad1_newkeys=edge?0x40:0; projectile_counter=count; cooldown_timer=cooldowns[c];
    for(int i=0;i<count;i++) { projectile_damage[i]=300; projectile_type[i]=0x9011; }
    ProbeRunBounded(0x90be62);
    fprintf(f,"%d,%d,%d,%d,%d,%04X,%04X,%04X,%04X,",selected,count,cooldowns[c],ammo,edge,
      projectile_counter,cooldown_timer,samus_missiles,samus_super_missiles);
    for(int i=0;i<5;i++) fprintf(f,"%04X",projectile_type[i]);
    fprintf(f,"\n");
  }
  fclose(f); return 0;
}
