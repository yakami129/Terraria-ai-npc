using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;

namespace terraguardians
{
    public class NpcMod : GlobalNPC
    {
        /*public override void SetDefaults(NPC npc)
        {
            if (npc.type == NPCID.Nurse)
            {
                npc.housingCategory = 2;
            }
        }*/

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            
        }
    }
}