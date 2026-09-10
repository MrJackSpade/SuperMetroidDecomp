#include "native-bounded-cpu.h"

// Combo updates leave zero charge and cooldown two. Include adjacent admission
// values to distinguish the first-bomb exception from a blanket combo ban.
int DiagnosticBombAdmission(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"selected,count,cooldown,ammo,edge,bombs,armed,result_count,result_cooldown,pb,hud,result_armed,types\n");
  const int counts[]={0,1,4,5}, cooldowns[]={0,1,2,256};
  for(int selected=0;selected<2;selected++) for(int n=0;n<4;n++)
  for(int c=0;c<4;c++) for(int ammo=0;ammo<2;ammo++) for(int edge=0;edge<2;edge++)
  for(int bombs=0;bombs<2;bombs++) for(int armed=0;armed<2;armed++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    samus_x_pos=samus_y_pos=128; hud_item_index=selected?3:0;
    samus_power_bombs=ammo; equipped_items=bombs?0x1000:0;
    button_config_shoot_x=joypad1_lastkeys=0x40; joypad1_newkeys=edge?0x40:0;
    bomb_counter=counts[n]; cooldown_timer=cooldowns[c]; power_bomb_flag=armed?0xffff:0;
    for(int i=0;i<counts[n];i++) projectile_type[i+5]=0x500;
    ProbeRunBounded(0x90bf9d);
    fprintf(f,"%d,%d,%d,%d,%d,%d,%d,%04X,%04X,%04X,%04X,%d,",
      selected,counts[n],cooldowns[c],ammo,edge,bombs,armed,bomb_counter,cooldown_timer,
      samus_power_bombs,hud_item_index,(power_bomb_flag&0x8000)!=0);
    for(int i=5;i<10;i++) fprintf(f,"%04X",projectile_type[i]);
    fprintf(f,"\n");
  }
  fclose(f); return 0;
}
