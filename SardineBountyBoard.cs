using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;
using System.Linq;
using Terraria.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.WorldBuilding;
using Terraria.ModLoader.IO;

namespace terraguardians
{
    public class SardineBountyBoard
    {
        public static bool TalkedAboutBountyBoard = false;
        public static int SignID = -1;
        public static bool IsAnnouncementBox = false;
        public const int NewRequestMinTime = 10 * 3600, NewRequestMaxTime = 20 * 3600,
            RequestEndMinTime = 20 * 3600, RequestEndMaxTime = 35 * 3600;
        public static int CoinReward = 0;
        public static Item[] RewardList = new Item[0];
        public const string NoRequestText = "No bounty right now.";
        public static int TargetMonsterID = 0;
        public static int TargetMonsterPosition = -1;
        public static string TargetName = "";
        public static string TargetSuffix = "";
        public static int ActionCooldown = 0;
        public static BountyRegion bountyRegion = null;
        public static int SpawnStress = 0;
        private static byte BoardUpdateTime = 0;
        public static List<BountyRegion> Regions = new List<BountyRegion>();

        public static void Load()
        {
            Regions.Add(new SkyBounty());
            Regions.Add(new SnowBounty());
            Regions.Add(new NightBounty());
            Regions.Add(new OceanBounty());
            Regions.Add(new HallowBounty());
            Regions.Add(new JungleBounty());
            Regions.Add(new CrimsonBounty());
            Regions.Add(new DungeonBounty());
            Regions.Add(new CrimsonBounty());
            Regions.Add(new CorruptionBounty());
            Regions.Add(new GoblinArmyBounty());
            Regions.Add(new PirateArmyBounty());
            Regions.Add(new UnderworldBounty());
            Regions.Add(new FrostLegionBounty());
            Regions.Add(new UndergroundBounty());
            Regions.Add(new MartianMadnessBounty());
            Regions.Add(new LihzahrdDungeonBounty());
        }

        internal static void Unload()
        {
            for(int i = 0; i < RewardList.Length; i++)
                RewardList[i] = null;
            RewardList = null;
            TargetName = null;
            TargetSuffix = null;
            Regions.Clear();
            Regions = null;
        }

        public static void Reset()
        {
            TargetMonsterID = 0;
            TargetMonsterPosition = -1;
            SetDefaultCooldown();
        }

        public static void Update()
        {
            if (SignID == -1)
            {
                if (Main.rand.Next(200) == 0)
                    TryFindingASign();
            }
            else
            {
                if(BoardUpdateTime == 0)
                {
                    BoardUpdateTime = 60;
                    UpdateBountyBoardText();
                }
                BoardUpdateTime--;
                if (ActionCooldown <= 0)
                {
                    TryFindingASign();
                    if (SignID > -1)
                    {
                        if (!TalkedAboutBountyBoard)
                        {
                            SetDefaultCooldown();
                        }
                        else
                        {
                            if (TargetMonsterID == 0)
                            {
                                GenerateRequest();
                            }
                            else
                            {
                                SetDefaultCooldown();
                                TargetMonsterID = 0;
                                UpdateBountyBoardText();
                                Main.NewText("Bounty Hunting Ended.", Color.MediumPurple);
                            }
                        }
                    }
                    else
                    {
                        SetDefaultCooldown();
                    }
                }
                ActionCooldown--;
            }
        }

        private static void GenerateRequest()
        {
            {
                List<int> PossibleRegions = new List<int>();
                float TotalStack = 0;
                for(int i = 0; i < Regions.Count; i++)
                {
                    BountyRegion region = Regions[i];
                    if (region.CanSpawnBounty(MainMod.GetLocalPlayer))
                    {
                        TotalStack += region.Chance;
                        PossibleRegions.Add(i);
                    }
                }
                if (PossibleRegions.Count == 0)
                {
                    SetDefaultCooldown();
                    return;
                }
                float Picked = Main.rand.NextFloat() * TotalStack;
                float Sum = 0;
                BountyRegion winnerRegion = null;
                foreach(int r in PossibleRegions)
                {
                    if(Picked < Sum + Regions[r].Chance)
                    {
                        winnerRegion = Regions[r];
                    }
                }
                if(winnerRegion == null)
                {
                    SetDefaultCooldown();
                    return;
                }
                bountyRegion = winnerRegion;
                TargetMonsterID = winnerRegion.GetBountyMonster(MainMod.GetLocalPlayer);
                TargetName = winnerRegion.GetBountyName(TargetMonsterID);
                TargetSuffix = winnerRegion.GetBountySuffix(TargetMonsterID);
            }

        }

        private static void UpdateBountyBoardText()
        {

        }

        public static void TryFindingASign()
        {
            bool LastHadSign = SignID >= 0;
            SignID = -1;
            Companion Sardine = WorldMod.GetCompanionNpc(CompanionDB.Sardine);
            if(Sardine == null)
            {
                return;
            }
            CompanionTownNpcState tns = Sardine.GetTownNpcState;
            if (tns == null || tns.Homeless) return;
            if(tns.HouseInfo == null) return;
            int HomeX = tns.HomeX, HomeY = tns.HomeY;
            foreach (FurnitureInfo furniture in tns.HouseInfo.Furnitures)
            {
                bool AnnouncementBox;
                if (AnnouncementBox = (furniture.FurnitureID == TileID.AnnouncementBox) || furniture.FurnitureID == TileID.Signs)
                {
                    SignID = Sign.ReadSign(furniture.FurnitureX, furniture.FurnitureY);
                    if (SignID > -1)
                    {
                        IsAnnouncementBox = AnnouncementBox;
                        break;
                    }
                }
            }
            if (!LastHadSign && SignID > -1)
            {
                Main.sign[SignID].text = "Request coming soon...";
            }
        }

        public static void ResetSpawnStress(bool OnGeneration)
        {
            if (OnGeneration)
                SpawnStress = Main.rand.Next(20, 41);
            else
                SpawnStress = Main.rand.Next(10, 21);
            if (Main.hardMode) SpawnStress += 20;
        }

        public static void CreateRewards(float RewardMod)
        {
            List<Item> Rewards = new List<Item>();
            Item i;
            /*if (Main.rand.NextDouble() < 0.03 * RewardMod)
            {
                i = new Item();
                switch (Main.rand.Next(4))
                {
                    case 0:
                        i.SetDefaults(ModContent.ItemType<Items.Accessories.PackLeaderNecklace>(), true);
                        break;
                    case 1:
                        i.SetDefaults(ModContent.ItemType<Items.Accessories.GoldenShowerPapyrus>(), true);
                        break;
                    case 2:
                        i.SetDefaults(ModContent.ItemType<Items.Accessories.FirstSymbol>(), true);
                        break;
                    case 3:
                        i.SetDefaults(ModContent.ItemType<Items.Accessories.TwoHandedMastery>(), true);
                        break;
                }
                Rewards.Add(i);
            }*/
            if (Main.rand.NextDouble() < 0.0667f * RewardMod)
            {
                i = new Item();
                i.SetDefaults(ItemID.FuzzyCarrot);
                Rewards.Add(i);
            }
                if (Main.rand.NextDouble() < 0.1 * RewardMod)
            {
                i = new Item();
                switch (Main.rand.Next(3))
                {
                    case 0:
                        i.SetDefaults(ItemID.FishermansGuide, true);
                        break;
                    case 1:
                        i.SetDefaults(ItemID.WeatherRadio, true);
                        break;
                    case 2:
                        i.SetDefaults(ItemID.Sextant, true);
                        break;
                }
                Rewards.Add(i);
            }
            if (Main.rand.NextDouble() < 0.2 * RewardMod)
            {
                i = new Item();
                switch (Main.rand.Next(3))
                {
                    case 0:
                        i.SetDefaults(ItemID.LifeformAnalyzer, true);
                        break;
                    case 1:
                        i.SetDefaults(ItemID.MetalDetector, true);
                        break;
                    case 2:
                        i.SetDefaults(ItemID.Radar, true);
                        break;
                }
                Rewards.Add(i);
            }
            if (Main.rand.NextDouble() < 0.05f * RewardMod)
            {
                i = new Item();
                if (Main.rand.NextDouble() < 0.75)
                {
                    i.SetDefaults(ItemID.MagicMirror, true);
                }
                else
                {
                    i.SetDefaults(ItemID.PocketMirror, true);
                }
                Rewards.Add(i);
            }
            if (Main.rand.NextDouble() < 0.1f * RewardMod)
            {
                i = GetRandomAccessory();
                if (i != null)
                    Rewards.Add(i);
            }
            if (Main.rand.NextDouble() < 0.15 * RewardMod)
            {
                i = GetRandomWeaponID();
                if (i != null)
                {
                    Rewards.Add(i);
                }
            }
            if (Main.rand.NextDouble() < 0.25 * RewardMod)
            {
                i = new Item();
                List<int> BossSpawnItems = new List<int>();
                //Check each boss that has been killed, then add the respective item to the boss spawn item list.
                if (NPC.downedBoss1)
                    BossSpawnItems.Add(ItemID.SuspiciousLookingEye);
                if (NPC.downedBoss2)
                {
                    if (WorldGen.crimson)
                        BossSpawnItems.Add(ItemID.BloodySpine);
                    else
                        BossSpawnItems.Add(ItemID.WormFood);
                }
                if (NPC.downedBoss3)
                    BossSpawnItems.Add(ItemID.ClothierVoodooDoll);
                if (NPC.downedSlimeKing)
                    BossSpawnItems.Add(ItemID.SlimeCrown);
                if (NPC.downedQueenBee)
                    BossSpawnItems.Add(ItemID.Abeemination);
                if (Main.hardMode)
                    BossSpawnItems.Add(ItemID.GuideVoodooDoll);
                if (NPC.downedMechBossAny)
                {
                    BossSpawnItems.Add(ItemID.MechanicalEye);
                    BossSpawnItems.Add(ItemID.MechanicalSkull);
                    BossSpawnItems.Add(ItemID.MechanicalWorm);
                }
                if (NPC.downedGolemBoss)
                    BossSpawnItems.Add(ItemID.LihzahrdPowerCell);
                if (NPC.downedFishron)
                    BossSpawnItems.Add(ItemID.TruffleWorm);
                if (NPC.downedMoonlord)
                    BossSpawnItems.Add(ItemID.CelestialSigil);
                if (BossSpawnItems.Count > 0)
                {
                    i.SetDefaults(BossSpawnItems[Main.rand.Next(BossSpawnItems.Count)], true);
                    /*if (i.maxStack > 0)
                    {
                        i.stack += Main.rand.Next((int)(3 * RewardMod));
                    }*/
                    Rewards.Add(i);
                }
            }
            if (Main.hardMode && NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3 && Main.rand.Next(3) == 0)
            {
                i = new Item();
                i.SetDefaults(ItemID.LifeFruit, true);
                if (Main.rand.NextDouble() < 0.6 * RewardMod)
                    i.stack += Main.rand.Next((int)(2 * RewardMod));
                Rewards.Add(i);
            }
            if (Main.rand.Next(3) == 0)
            {
                i = new Item();
                i.SetDefaults(ItemID.LifeCrystal, true);
                //if (Main.rand.NextDouble() < 0.4 * RewardMod)
                //    i.stack += Main.rand.Next((int)(RewardMod));
                Rewards.Add(i);
            }
            bountyRegion.SetupLoot(MainMod.GetLocalPlayer, Rewards, RewardMod);
            if (Main.rand.Next(5) == 0)
            {
                i = new Item();
                i.SetDefaults(ItemID.HerbBag);
                i.stack += Main.rand.Next((int)(3 * RewardMod));
                Rewards.Add(i);
            }
            foreach (Item item in Rewards)
            {
                PrefixAnItem(item);
            }
            RewardList = Rewards.ToArray();
        }

        public static Item GetRandomWeaponID()
        {
            int WeaponID = 0;
            {
                List<int> RewardToGet = new List<int>();
                if (!Main.hardMode)
                {
                    float LootRate = Main.rand.NextFloat();
                    RewardToGet.AddRange(new int[] { ItemID.CopperShortsword, ItemID.CopperBroadsword, ItemID.CopperBow,
                        ItemID.TinShortsword, ItemID.TinBroadsword, ItemID.TinBow});
                    //if (LootRate < 0.667)
                    {
                        RewardToGet.AddRange(new int[] { ItemID.SilverShortsword, ItemID.SilverBroadsword, ItemID.SilverBow,
                            ItemID.TungstenShortsword, ItemID.TungstenBroadsword, ItemID.TungstenBow});
                    }
                    //if (LootRate < 0.333)
                    {
                        RewardToGet.AddRange(new int[] { ItemID.GoldShortsword, ItemID.GoldBroadsword, ItemID.GoldBow,
                            ItemID.PlatinumShortsword, ItemID.PlatinumBroadsword, ItemID.PlatinumBow});
                    }
                }
                else
                {
                    float LootRate = Main.rand.NextFloat();
                    RewardToGet.AddRange(new int[] { ItemID.CobaltSword, ItemID.CobaltNaginata, ItemID.CobaltRepeater,
                        ItemID.PalladiumSword, ItemID.PalladiumPike, ItemID.PalladiumRepeater});
                    //if (LootRate < 0.667)
                    {
                        RewardToGet.AddRange(new int[] {ItemID.MythrilSword, ItemID.MythrilHalberd, ItemID.MythrilRepeater,
                            ItemID.OrichalcumSword, ItemID.OrichalcumHalberd, ItemID.OrichalcumRepeater });
                    }
                    //if (LootRate < 0.333)
                    {
                        RewardToGet.AddRange(new int[] { ItemID.AdamantiteSword, ItemID.AdamantiteGlaive, ItemID.AdamantiteRepeater,
                            ItemID.TitaniumSword, ItemID.TitaniumTrident, ItemID.TitaniumRepeater});
                    }
                }
                if(NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
                {
                    RewardToGet.AddRange(new int[] { ItemID.Excalibur, ItemID.Gungnir, ItemID.HallowedRepeater });
                }
                if (RewardToGet.Count > 0)
                    WeaponID = RewardToGet[Main.rand.Next(RewardToGet.Count)];
            }
            if (WeaponID > 0)
            {
                Item i = new Item();
                i.SetDefaults(WeaponID, true);
                return i;
            }
            return null;
        }

        public static void PrefixAnItem(Item item)
        {
            if (Main.rand.NextFloat() >= 0.8f) return;
            byte prefix = 0;
            if (item.accessory)
            {
                switch (Main.rand.Next(12))
                {
                    case 0:
                        prefix = Terraria.ID.PrefixID.Armored;
                        break;
                    case 1:
                        prefix = Terraria.ID.PrefixID.Warding;
                        break;
                    case 2:
                        prefix = Terraria.ID.PrefixID.Precise;
                        break;
                    case 3:
                        prefix = Terraria.ID.PrefixID.Lucky;
                        break;
                    case 4:
                        prefix = Terraria.ID.PrefixID.Angry;
                        break;
                    case 5:
                        prefix = Terraria.ID.PrefixID.Menacing;
                        break;
                    case 6:
                        prefix = Terraria.ID.PrefixID.Hasty;
                        break;
                    case 7:
                        prefix = Terraria.ID.PrefixID.Quick;
                        break;
                    case 8:
                        prefix = Terraria.ID.PrefixID.Intrepid;
                        break;
                    case 9:
                        prefix = Terraria.ID.PrefixID.Violent;
                        break;
                    case 10:
                        prefix = Terraria.ID.PrefixID.Arcane;
                        break;

                }
            }
            else
            {
                if (item.DamageType is MeleeDamageClass)
                {
                    switch (Main.rand.Next(10))
                    {
                        case 1:
                            prefix = Terraria.ID.PrefixID.Dangerous;
                            break;
                        case 2:
                            prefix = Terraria.ID.PrefixID.Savage;
                            break;
                        case 3:
                            prefix = Terraria.ID.PrefixID.Deadly; //Common
                            break;
                        case 4:
                            prefix = Terraria.ID.PrefixID.Ruthless; //Universal
                            break;
                        case 5:
                            prefix = Terraria.ID.PrefixID.Godly; //Universal
                            break;
                        case 6:
                            prefix = Terraria.ID.PrefixID.Demonic; //Universal
                            break;
                        case 7:
                            prefix = Terraria.ID.PrefixID.Savage;
                            break;
                        case 8:
                            prefix = Terraria.ID.PrefixID.Legendary;
                            break;
                        case 9:
                            prefix = Terraria.ID.PrefixID.Superior;
                            break;
                    }
                }
                if (item.DamageType is RangedDamageClass)
                {
                    switch (Main.rand.Next(10))
                    {
                        case 1:
                            prefix = Terraria.ID.PrefixID.Deadly2;
                            break;
                        case 2:
                            prefix = Terraria.ID.PrefixID.Rapid;
                            break;
                        case 3:
                            prefix = Terraria.ID.PrefixID.Deadly; //Common
                            break;
                        case 4:
                            prefix = Terraria.ID.PrefixID.Ruthless; //Universal
                            break;
                        case 5:
                            prefix = Terraria.ID.PrefixID.Godly; //Universal
                            break;
                        case 6:
                            prefix = Terraria.ID.PrefixID.Demonic; //Universal
                            break;
                        case 7:
                            prefix = Terraria.ID.PrefixID.Powerful;
                            break;
                        case 8:
                            prefix = Terraria.ID.PrefixID.Unreal;
                            break;
                        case 9:
                            prefix = Terraria.ID.PrefixID.Superior;
                            break;
                    }
                }
                if (item.DamageType is MagicDamageClass)
                {
                    switch (Main.rand.Next(10))
                    {
                        case 1:
                            prefix = Terraria.ID.PrefixID.Masterful;
                            break;
                        case 2:
                            prefix = Terraria.ID.PrefixID.Celestial;
                            break;
                        case 3:
                            prefix = Terraria.ID.PrefixID.Deadly; //Common
                            break;
                        case 4:
                            prefix = Terraria.ID.PrefixID.Ruthless; //Universal
                            break;
                        case 5:
                            prefix = Terraria.ID.PrefixID.Godly; //Universal
                            break;
                        case 6:
                            prefix = Terraria.ID.PrefixID.Demonic; //Universal
                            break;
                        case 7:
                            prefix = Terraria.ID.PrefixID.Mystic;
                            break;
                        case 8:
                            prefix = Terraria.ID.PrefixID.Mythical;
                            break;
                        case 9:
                            prefix = Terraria.ID.PrefixID.Superior;
                            break;
                    }
                }
                if (item.DamageType is SummonDamageClass)
                {
                    switch (Main.rand.Next(9))
                    {
                        case 1:
                            prefix = Terraria.ID.PrefixID.Deadly; //Common
                            break;
                        case 2:
                            prefix = Terraria.ID.PrefixID.Ruthless; //Universal
                            break;
                        case 3:
                            prefix = Terraria.ID.PrefixID.Godly; //Universal
                            break;
                        case 4:
                            prefix = Terraria.ID.PrefixID.Demonic; //Universal
                            break;
                        case 5:
                            prefix = Terraria.ID.PrefixID.Murderous;
                            break;
                        case 6:
                            prefix = Terraria.ID.PrefixID.Hurtful;
                            break;
                        case 7:
                            prefix = Terraria.ID.PrefixID.Unpleasant;
                            break;
                        case 8:
                            prefix = Terraria.ID.PrefixID.Superior;
                            break;
                    }
                }
            }
            item.Prefix(prefix);
        }

        public static Item GetRandomAccessory()
        {
            List<int> ItemIDs = new List<int>();
            ItemIDs.Add(ItemID.Aglet);
            ItemIDs.Add(ItemID.HermesBoots);
            ItemIDs.Add(ItemID.ClimbingClaws);
            ItemIDs.Add(ItemID.ShoeSpikes);
            ItemIDs.Add(ItemID.Flipper);
            ItemIDs.Add(ItemID.LuckyHorseshoe);
            ItemIDs.Add(ItemID.ShinyRedBalloon);
            //
            ItemIDs.Add(ItemID.BandofRegeneration);
            if (ItemIDs.Count > 0)
            {
                Item i = new Item();
                i.SetDefaults(ItemIDs[Main.rand.Next(ItemIDs.Count)], true);
                return i;
            }
            return null;
        }

        public static int ExtraCoinRewardFromProgress()
        {
            int Extra = 0;
            if (NPC.downedBoss1)
                Extra += 125;
            if (NPC.downedBoss2)
                Extra += 250;
            if (NPC.downedBoss3)
                Extra += 500;
            if (NPC.downedSlimeKing)
                Extra += 350;
            if (NPC.downedQueenBee)
                Extra += 400;
            if (Main.hardMode)
                Extra += 1000;
            if (NPC.downedMechBoss1)
                Extra += 1150;
            if (NPC.downedMechBoss2)
                Extra += 1400;
            if (NPC.downedMechBoss3)
                Extra += 1850;
            if (NPC.downedPlantBoss)
                Extra += 2220;
            if (NPC.downedGolemBoss)
                Extra += 2650;
            if (NPC.downedAncientCultist)
                Extra += 3500;
            if (NPC.downedFishron)
                Extra += 4000;
            if (NPC.downedMoonlord)
                Extra += 5000;
            return Extra;
        }
        
        public static string GetRandomString(string[] List)
        {
            return Utils.SelectRandom(Main.rand, List);
        }

        public static string NameGen(string[] Syllabes)
        {
            string NewName = "";
            double Chance = 2f;
            bool First = true;
            List<int> UsedSyllabes = new List<int>();
            while (Main.rand.NextDouble() < Chance)
            {
                int SelectedSyllabe = Main.rand.Next(Syllabes.Length);
                int SyllabesDisponible = 0;
                for (int s = 0; s < Syllabes.Length; s++)
                {
                    if (!UsedSyllabes.Contains(s))
                    {
                        SyllabesDisponible++;
                    }
                }
                if (SyllabesDisponible == 0)
                    break;
                if (UsedSyllabes.Contains(SelectedSyllabe))
                    continue;
                UsedSyllabes.Add(SelectedSyllabe);
                string Syllabe = Syllabes[SelectedSyllabe].ToLower();
                foreach (char Letter in Syllabe)
                {
                    NewName += Letter;
                    if (First)
                    {
                        NewName = NewName.ToUpper();
                        First = false;
                    }
                }
                if (Chance > 1f)
                    Chance--;
                else if (Chance > 0.5f)
                    Chance -= 0.2f;
                else
                {
                    Chance *= 0.5f;
                }
            }
            return NewName;
        }

        public static void SetDefaultCooldown()
        {
            ActionCooldown = 5 * 3600;
        }

        #region Bounty Types
        public class CrimsonBounty : BountyRegion
        {
            public override string Name => "Crimson";
            public override float Chance => 2;
            public override bool CanSpawnBounty(Player player)
            {
                return NPC.downedBoss1 && WorldGen.crimson;
            }

            public override int GetBountyMonster(Player player)
            {
                if (Main.hardMode)
                {
                    switch (Main.rand.Next(4))
                    {
                        default:
                            return NPCID.Crimslime;
                        case 1:
                            return NPCID.Herpling;
                        case 2:
                            return NPCID.CrimsonAxe;
                        case 3:
                            return NPCID.BigMimicCrimson;
                    }
                }
                else
                {
                    if (Main.rand.Next(2) == 0)
                    {
                        return NPCID.FaceMonster;
                    }
                    else
                    {
                        return NPCID.Crimera;
                    }
                }
            }

            public override void SetupLoot(Player player, List<Item> PossibleRewards, float RewardMod)
            {
                if (Main.rand.Next(5) == 0)
                {
                    Item i = new Item(Main.hardMode ? 
                        ItemID.CrimsonFishingCrateHard : 
                        ItemID.CrimsonFishingCrate, Main.rand.Next(1, 1 + (int)(3 * RewardMod)));
                    PossibleRewards.Add(i);
                }
                if (Main.rand.NextFloat() < 0.1f * RewardMod)
                {
                    PossibleRewards.Add(new Item(ItemID.PanicNecklace));
                }
                if (Main.rand.NextFloat() < 0.15f * RewardMod)
                {
                    List<int> Items = new List<int>();
                    Items.AddRange(new int[] { ItemID.TheRottedFork, ItemID.TheUndertaker, ItemID.TheMeatball, ItemID.TendonBow, ItemID.CrimsonRod, ItemID.BloodButcherer });
                    if (Main.hardMode)
                        Items.AddRange(new int[] { ItemID.Bladetongue, ItemID.DartPistol, ItemID.GoldenShower });
                    PossibleRewards.Add(new Item(Items[Main.rand.Next(Items.Count)]));
                    Items.Clear();
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return player.ZoneCrimson;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] { "crim", "son", "blo", "od", "cri", "me", "ra", "fa", "ce", "mons", "ter", "herp", "ling" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Blood Feaster", "Defiler", "Goremancer", "Arterial Traveller", "Heart Breaker", "Whose Blood Drips from the Mouth" });
            }
        }

        public class CorruptionBounty : BountyRegion
        {
            public override string Name => "Corruption";
            public override float Chance => 2;
            public override bool CanSpawnBounty(Player player)
            {
                return NPC.downedBoss1 && !WorldGen.crimson;
            }

            public override int GetBountyMonster(Player player)
            {
                if (Main.hardMode)
                {
                    switch (Main.rand.Next(5))
                    {
                        default:
                            return NPCID.Corruptor;
                        case 1:
                            return NPCID.Slimer;
                        case 2:
                            return NPCID.SeekerHead;
                        case 3:
                            return NPCID.CursedHammer;
                        case 4:
                            return NPCID.BigMimicCorruption;
                    }
                }
                else
                {
                    if (Main.rand.Next(2) == 0)
                    {
                        return NPCID.DevourerHead;
                    }
                    else
                    {
                        return NPCID.EaterofSouls;
                    }
                }
            }

            public override void SetupLoot(Player player, List<Item> PossibleRewards, float RewardMod)
            {
                if (Main.rand.Next(5) == 0)
                {
                    Item i = new Item(Main.hardMode ? 
                        ItemID.CorruptFishingCrateHard : 
                        ItemID.CorruptFishingCrate, Main.rand.Next(1, 1 + (int)(3 * RewardMod)));
                    PossibleRewards.Add(i);
                }
                if (Main.rand.NextFloat() < 0.1f * RewardMod)
                {
                    PossibleRewards.Add(new Item(ItemID.BandofRegeneration));
                }
                if (Main.rand.NextFloat() < 0.15f * RewardMod)
                {
                    List<int> Items = new List<int>();
                    Items.AddRange(new int[] { ItemID.BallOHurt, ItemID.DemonBow, ItemID.Musket, ItemID.Vilethorn, ItemID.LightsBane, ItemID.DemonBow });
                    if (Main.hardMode)
                        Items.AddRange(new int[] { ItemID.Toxikarp, ItemID.DartRifle, ItemID.CursedFlames, ItemID.ClingerStaff });
                    PossibleRewards.Add(new Item(Items[Main.rand.Next(Items.Count)]));
                    Items.Clear();
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return player.ZoneCorrupt;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[]{"di", "sea", "sed", "ea", "ter", "of", "so", "ul", "de", "vou", "rer", "cor", "rup", "tor", "sli", "mer", "see", "ker"});
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Plague Bearer", "Thousand Souls", "Corruption Spreader", "World Destroyer", "Reaper", "Who Ends Lives Swiftly" });
            }
        }

        public class HallowBounty : BountyRegion
        {
            public override string Name => "Hallow";
            public override float Chance => 2;
            public override bool CanSpawnBounty(Player player)
            {
                return Main.hardMode;
            }

            public override int GetBountyMonster(Player player)
            {
                switch (Main.rand.Next(5))
                {
                    default:
                        return NPCID.Pixie;
                    case 1:
                        return NPCID.Unicorn;
                    case 2:
                        return NPCID.ChaosElemental;
                    case 3:
                        return NPCID.EnchantedSword;
                    case 4:
                        return NPCID.BigMimicHallow;
                }
            }

            public override void SetupLoot(Player player, List<Item> PossibleRewards, float RewardMod)
            {
                if (Main.rand.Next(5) == 0)
                {
                    Item i = new Item(Main.hardMode ? 
                        ItemID.HallowedFishingCrateHard : 
                        ItemID.HallowedFishingCrate, Main.rand.Next(1, 1 + (int)(3 * RewardMod)));
                    PossibleRewards.Add(i);
                }
                if (Main.rand.NextFloat() < 0.15f * RewardMod)
                {
                    List<int> Items = new List<int>();
                    Items.AddRange(new int[] { ItemID.PearlwoodSword });
                    if (Main.hardMode)
                        Items.AddRange(new int[] { ItemID.CrystalStorm, ItemID.CrystalSerpent });
                    PossibleRewards.Add(new Item(Items[Main.rand.Next(Items.Count)]));
                    Items.Clear();
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return player.ZoneHallow;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] { "pi", "xie", "cha", "os","ele", "men", "tal", "uni", "corn", "en", "chan", "ted", "po", "ny" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Hallowed Inquisitioner", "Rainbow of Suffering", "Nausea Inducer", "Annoying Thing", "Prismatic Ray", "Who Believes Friendship Is Magic" });
            }
        }

        public class DesertBounty : BountyRegion
        {
            public override string Name => "Desert";
            public override float Chance => 0.8f;
            public override bool CanSpawnBounty(Player player)
            {
                return player.statDefense >= 2;
            }

            public override int GetBountyMonster(Player player)
            {
                if(Main.hardMode && Main.rand.NextDouble() < 0.6f)
                {
                    switch (Main.rand.Next(4))
                    {
                        default:
                            return 78;
                        case 1:
                            return 532;
                        case 2:
                            return Main.rand.NextDouble() < 0.5 ? 528 : 529;
                        case 3:
                            return 533;
                    }
                }
                switch (Main.rand.Next(4))
                {
                    default:
                        return 69;
                    case 1:
                        return 61;
                    case 2:
                        return 508;
                    case 3:
                        return 509;
                }
            }

            public override void SetupLoot(Player player, List<Item> PossibleRewards, float RewardMod)
            {
                if (Main.rand.Next(5) == 0)
                {
                    Item i = new Item(Main.hardMode ? 
                        ItemID.OasisCrateHard : 
                        ItemID.OasisCrate, Main.rand.Next(1, 1 + (int)(3 * RewardMod)));
                    PossibleRewards.Add(i);
                }
                if (Main.rand.NextFloat() < 0.1f * RewardMod)
                {
                    PossibleRewards.Add(new Item(4276)); //The Bast Statue in the game.
                }
                if (Main.rand.NextFloat() < 0.15f * RewardMod)
                {
                    List<int> Items = new List<int>();
                    Items.AddRange(new int[] { ItemID.AntlionMandible, ItemID.AmberStaff, ItemID.ThunderSpear, ItemID.ThunderStaff });
                    PossibleRewards.Add(new Item(Items[Main.rand.Next(Items.Count)]));
                    Items.Clear();
                }
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] { "ant", "li", "on", "ba", "si", "lisk", "poa", "cher", "la", "mia", "gho", "ul" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Elusive", "Burier", "Forgotten", "Sandman", "Who Buries Their Victims" });
            }
        }

        public class DungeonBounty : BountyRegion
        {
            public override string Name => "Dungeon";
            public override float Chance => 1.5f;
            public override bool CanSpawnBounty(Player player)
            {
                return NPC.downedBoss3;
            }

            public override int GetBountyMonster(Player player)
            {
                bool IsHardmodeDungeon = Main.hardMode && NPC.downedPlantBoss;
                if (IsHardmodeDungeon)
                {
                    switch (Main.rand.Next(12))
                    {
                        default:
                            return NPCID.BlueArmoredBonesSword;
                        case 1:
                            return NPCID.RustyArmoredBonesAxe;
                        case 2:
                            return NPCID.HellArmoredBonesMace;
                        case 3:
                            return NPCID.Necromancer;
                        case 4:
                            return NPCID.RaggedCaster;
                        case 5:
                            return NPCID.DiabolistRed;
                        case 6:
                            return NPCID.SkeletonCommando;
                        case 7:
                            return NPCID.SkeletonSniper;
                        case 8:
                            return NPCID.TacticalSkeleton;
                        case 9:
                            return NPCID.GiantCursedSkull;
                        case 10:
                            return NPCID.BoneLee;
                        case 11:
                            return NPCID.Paladin;
                    }
                }
                else
                {
                    switch (Main.rand.Next(4))
                    {
                        default:
                            return NPCID.AngryBones;
                        case 1:
                            return NPCID.DarkCaster;
                        case 2:
                            return NPCID.CursedSkull;
                        case 3:
                            return NPCID.DungeonSlime;
                    }
                }
            }

            public override void SetupLoot(Player player, List<Item> PossibleRewards, float RewardMod)
            {
                if (Main.rand.Next(5) == 0)
                {
                    Item i = new Item(Main.hardMode ? 
                        ItemID.DungeonFishingCrateHard : 
                        ItemID.DungeonFishingCrate, Main.rand.Next(1, 1 + (int)(3 * RewardMod)));
                    PossibleRewards.Add(i);
                }
                if (Main.rand.NextFloat() < 0.1f * RewardMod)
                {
                    PossibleRewards.Add(new Item(ItemID.CobaltShield));
                }
                if (Main.rand.NextFloat() < 0.15f * RewardMod)
                {
                    List<int> Items = new List<int>();
                    Items.AddRange(new int[] { ItemID.Muramasa, ItemID.Handgun, ItemID.AquaScepter, ItemID.MagicMissile, ItemID.BlueMoon });
                    if (NPC.downedPlantBoss)
                        Items.AddRange(new int[] { ItemID.TacticalShotgun, ItemID.SniperRifle, ItemID.Keybrand, ItemID.RocketLauncher, ItemID.SpectreStaff, ItemID.InfernoFork, ItemID.ShadowbeamStaff, ItemID.MagnetSphere });
                    PossibleRewards.Add(new Item(Items[Main.rand.Next(Items.Count)]));
                    Items.Clear();
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return player.ZoneDungeon;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] { "ske", "le", "ton", "an","gry","bo", "nes", "cas", "ter","cur","sed","dun", "ge", "on", "pa", "la", "din" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Scary Spooky", "Dead Awakener", "Enemy of World", "He-Man's Nemesis", "Bone Breaker", "Who Killed 500 Adventurers" });
            }
        }

        public class UnderworldBounty : BountyRegion
        {
            public override string Name => "Underworld";
            public override float Chance => 1.15f;
            public override bool CanSpawnBounty(Player player)
            {
                return NPC.downedBoss2 || NPC.downedSlimeKing || NPC.downedQueenBee || NPC.downedGoblins;
            }

            public override int GetBountyMonster(Player player)
            {
                if (Main.hardMode)
                {
                    switch (Main.rand.Next(2))
                    {
                        default:
                            return NPCID.RedDevil;
                        case 1:
                            return NPCID.Lavabat;
                    }
                }
                else
                {
                    switch (Main.rand.Next(3))
                    {
                        default:
                            return NPCID.Demon;
                        case 1:
                            return NPCID.BoneSerpentHead;
                        case 2:
                            return NPCID.FireImp;
                    }
                }
            }

            public override void SetupLoot(Player player, List<Item> PossibleRewards, float RewardMod)
            {
                if (Main.rand.Next(5) == 0)
                {
                    Item i = new Item(Main.hardMode ? 
                        ItemID.LavaCrateHard : 
                        ItemID.LavaCrate, Main.rand.Next(1, 1 + (int)(3 * RewardMod)));
                    PossibleRewards.Add(i);
                }
                if (Main.rand.NextFloat() < 0.1f * RewardMod)
                {
                    PossibleRewards.Add(new Item(Utils.SelectRandom<int>(Main.rand, ItemID.LavaCharm, ItemID.ObsidianRose, ItemID.ObsidianSkull, ItemID.MagmaStone)));
                }
                if (Main.rand.NextFloat() < 0.15f * RewardMod)
                {
                    List<int> Items = new List<int>();
                    Items.AddRange(new int[] { ItemID.FieryGreatsword, ItemID.DarkLance, ItemID.Sunfury, ItemID.FlowerofFire, ItemID.Flamelash, ItemID.HellwingBow, ItemID.ImpStaff });
                    if (NPC.downedMechBossAny)
                        Items.AddRange(new int[] { ItemID.UnholyTrident, ItemID.ObsidianSwordfish });
                    PossibleRewards.Add(new Item(Items[Main.rand.Next(Items.Count)]));
                    Items.Clear();
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return player.ZoneUnderworldHeight;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] { "de", "mon", "bo", "ne", "ser", "pent", "he" , "ad", "fi", "re", "imp", "red", "vil", "la", "va", "bat" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Pact Maker", "Hell Breaker", "Torturer", "Lava Eater", "Human Buster", "Who Bathe in Blood" });
            }
        }

        public class JungleBounty : BountyRegion
        {
            public override string Name => "Jungle";
            public override float Chance => 1.35f;
            public override bool CanSpawnBounty(Player player)
            {
                return (!Main.hardMode && (NPC.downedQueenBee || NPC.downedBoss2)) || (Main.hardMode && NPC.downedPlantBoss);
            }

            public override int GetBountyMonster(Player player)
            {
                if (Main.hardMode)
                {
                    switch (Main.rand.Next(6))
                    {
                        default:
                            return NPCID.Derpling;
                        case 1:
                            return NPCID.GiantTortoise;
                        case 2:
                            return NPCID.GiantFlyingFox;
                        case 3:
                            return NPCID.MossHornet;
                        case 4:
                            return NPCID.Moth;
                        case 5:
                            return NPCID.BigMimicJungle;
                    }
                }
                else
                {
                    if (Main.rand.Next(2) == 0)
                    {
                        return NPCID.Hornet;
                    }
                    else
                    {
                        return NPCID.JungleBat;
                    }
                }
            }

            public override void SetupLoot(Player player, List<Item> PossibleRewards, float RewardMod)
            {
                if (Main.rand.Next(5) == 0)
                {
                    Item i = new Item(Main.hardMode ? 
                        ItemID.JungleFishingCrateHard : 
                        ItemID.JungleFishingCrate, Main.rand.Next(1, 1 + (int)(3 * RewardMod)));
                    PossibleRewards.Add(i);
                }
                if (Main.rand.NextFloat() < 0.1f * RewardMod)
                {
                    PossibleRewards.Add(new Item(Utils.SelectRandom<int>(Main.rand, ItemID.AnkletoftheWind, ItemID.FeralClaws, ItemID.FlowerBoots)));
                }
                if (Main.rand.NextFloat() < 0.15f * RewardMod)
                {
                    List<int> Items = new List<int>();
                    Items.AddRange(new int[] { ItemID.BladeofGrass, ItemID.ThornChakram, ItemID.BeeKeeper, ItemID.BeesKnees, ItemID.BeeGun, ItemID.HornetStaff });
                    if (Main.hardMode)
                        Items.AddRange(new int[] { ItemID.ChlorophyteClaymore, ItemID.ChlorophytePartisan, ItemID.ChlorophyteSaber, ItemID.ChlorophyteShotbow, ItemID.Uzi });
                    PossibleRewards.Add(new Item(Items[Main.rand.Next(Items.Count)]));
                    Items.Clear();
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return player.ZoneJungle;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] { "fox", "hor", "net", "jun", "gle", "fly", "ing", "tor", "toi", "se", "moss", "derp", "ling" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Deadly Poison", "Arms Dealer Hunter", "Catnapper", "Armor Piercer", "Home Wrecker" });
            }
        }

        public class OceanBounty : BountyRegion
        {
            public override string Name => "Ocean";
            public override float Chance => 0.75f;
            public override bool CanSpawnBounty(Player player)
            {
                return NPC.downedSlimeKing;
            }

            public override int GetBountyMonster(Player player)
            {
                switch (Main.rand.Next(2))
                {
                    default:
                        return 65;
                    case 1:
                        return 67;
                }
            }

            public override void SetupLoot(Player player, List<Item> PossibleRewards, float RewardMod)
            {
                if (Main.rand.Next(5) == 0)
                {
                    Item i = new Item(Main.hardMode ? 
                        ItemID.OceanCrateHard : 
                        ItemID.OceanCrate, Main.rand.Next(1, 1 + (int)(3 * RewardMod)));
                    PossibleRewards.Add(i);
                }
                if (Main.rand.NextFloat() < 0.15f * RewardMod)
                {
                    List<int> Items = new List<int>();
                    Items.AddRange(new int[] { ItemID.Swordfish, ItemID.Trident });
                    PossibleRewards.Add(new Item(Items[Main.rand.Next(Items.Count)]));
                    Items.Clear();
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return player.ZoneBeach;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] {"me", "ga", "lo", "don", "shark", "crab", "de", "vo", "ur", "de", "ath", "bre" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] {"Man-Eater", "Menace", "Drowner" });
            }
        }

        public class SnowBounty : BountyRegion
        {
            public override string Name => "Snowland";
            public override float Chance => 1f;
            public override bool CanSpawnBounty(Player player)
            {
                return true;
            }

            public override int GetBountyMonster(Player player)
            {
                if(Main.hardMode && Main.rand.NextDouble() < 0.6)
                {
                    if (Main.rand.NextDouble() < 0.25)
                    {
                        switch (Main.rand.Next(3))
                        {
                            default:
                                return 170;
                            case 1:
                                return 171;
                            case 2:
                                return 180;
                        }
                    }
                    else
                    {
                        switch (Main.rand.Next(3))
                        {
                            default:
                                return 154;
                            case 1:
                                return 169;
                            case 2:
                                return 206;
                        }
                    }
                }
                switch (Main.rand.Next(3))
                {
                    default:
                        return 150;
                    case 1:
                        return 147;
                    case 2:
                        return 197;
                }
            }

            public override void SetupLoot(Player player, List<Item> PossibleRewards, float RewardMod)
            {
                if (Main.rand.Next(5) == 0)
                {
                    Item i = new Item(Main.hardMode ? 
                        ItemID.FrozenCrateHard : 
                        ItemID.FrozenCrate, Main.rand.Next(1, 1 + (int)(3 * RewardMod)));
                    PossibleRewards.Add(i);
                }
                if (Main.rand.NextFloat() < 0.15f * RewardMod)
                {
                    List<int> Items = new List<int>();
                    Items.AddRange(new int[] { ItemID.IceBlade, ItemID.IceBoomerang, ItemID.SnowballCannon });
                    if (Main.hardMode)
                        Items.AddRange(new int[] { ItemID.Frostbrand, ItemID.IceBow, ItemID.FlowerofFrost, ItemID.FrostStaff, ItemID.IceRod });
                    PossibleRewards.Add(new Item(Items[Main.rand.Next(Items.Count)]));
                    Items.Clear();
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return player.ZoneSnow;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] { "ice", "bat", "snow", "flinx", "un", "de", "ad", "vi", "king", "tor", "toi", "se", "ele", "men", "tal", "mer", "man"});
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Cold Heart", "Shiver Causer", "Chilled One", "One Who Paints Snow in Red" });
            }
        }

        public class NightBounty : BountyRegion
        {
            public override string Name => "Night";
            public override float Chance => 1;
            public override bool CanSpawnBounty(Player player)
            {
                return true;
            }

            public override int GetBountyMonster(Player player)
            {
                if(Main.hardMode && Main.rand.NextDouble() < 0.6)
                {
                    switch (Main.rand.Next(3))
                    {
                        default:
                            return 140;
                        case 1:
                            return 82;
                        case 2:
                            return 104;
                    }
                }
                if(Main.rand.Next(2) == 0)
                {
                    return 3;
                }
                else
                {
                    return 2;
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return !Main.dayTime && player.ZoneOverworldHeight;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] { "zom", "bie", "de", "mon", "eye", "ra", "ven", "pos", "ses", "ed", "wra", "ith", "we", "re", "wolf" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Night Crawler", "Restless Soul", "Prowler" });
            }
        }

        public class UndergroundBounty : BountyRegion
        {
            public override string Name => "Underground";
            public override float Chance => 0.8f;
            public override bool CanSpawnBounty(Player player)
            {
                return true;
            }

            public override int GetBountyMonster(Player player)
            {
                if (Main.hardMode && Main.rand.NextFloat() < 0.6f)
                {
                    switch (Main.rand.Next(4))
                    {
                        default:
                            return 77;
                        case 1:
                            return 93;
                        case 2:
                            return 110;
                        case 3:
                            return 172;
                    }
                }
                switch (Main.rand.Next(6))
                {
                    default:
                        return 21;
                    case 1:
                        return 49;
                    case 2:
                        return Main.rand.Next(498, 507);
                    case 3:
                        return Main.rand.Next(404, 406);
                    case 4:
                        return Main.rand.Next(496, 498);
                    case 5:
                        return 196;
                }
            }

            public override void SetupLoot(Player player, List<Item> PossibleRewards, float RewardMod)
            {
                if (Main.rand.NextFloat() < 0.15f * RewardMod)
                {
                    List<int> Items = new List<int>();
                    Items.AddRange(new int[] { ItemID.ChainKnife, ItemID.Spear, ItemID.WoodenBoomerang, ItemID.EnchantedBoomerang, ItemID.Mace });
                    if (Main.hardMode)
                        Items.AddRange(new int[] { ItemID.BeamSword, ItemID.Marrow, ItemID.PoisonStaff, ItemID.SpiderStaff, ItemID.QueenSpiderStaff });
                    PossibleRewards.Add(new Item(Items[Main.rand.Next(Items.Count)]));
                    Items.Clear();
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight) && !player.ZoneDungeon;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] { "ske", "le", "ton", "sa", "la", "man", "der", "craw", "dad", "gi", "ant", "shel", "ly", "ca", "ve", "bat" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Bane of the Living", "Impaler", "Human-Hunter", "Dark Stalker" });
            }
        }

        public class SkyBounty : BountyRegion
        {
            public override string Name => "Sky";
            public override float Chance => 0.85f;
            public override bool CanSpawnBounty(Player player)
            {
                return NPC.downedBoss2 || NPC.downedBoss3 || NPC.downedGoblins|| Main.hardMode;
            }

            public override int GetBountyMonster(Player player)
            {
                return 48;
            }

            public override void SetupLoot(Player player, List<Item> PossibleRewards, float RewardMod)
            {
                if (Main.rand.Next(5) == 0)
                {
                    Item i = new Item(Main.hardMode ? 
                        ItemID.FloatingIslandFishingCrateHard : 
                        ItemID.FloatingIslandFishingCrate, Main.rand.Next(1, 1 + (int)(3 * RewardMod)));
                    PossibleRewards.Add(i);
                }
                if (Main.rand.NextFloat() < 0.15f * RewardMod)
                {
                    List<int> Items = new List<int>();
                    Items.AddRange(new int[] { ItemID.Starfury, ItemID.DaedalusStormbow });
                    if (Main.hardMode)
                        Items.AddRange(new int[] { ItemID.NimbusRod });
                    PossibleRewards.Add(new Item(Items[Main.rand.Next(Items.Count)]));
                    Items.Clear();
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return player.ZoneSkyHeight;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] {"mar", "le", "ne", "har", "py", "da", "ria", "ki", "ra" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Siren", "Matriarch", "Human Snatcher", "Sky Guardian", "Who Preys on Humans" });
            }
        }

        public class GoblinArmyBounty : BountyRegion
        {
            public override string Name => "Goblin Army";
            public override float Chance => 0.65f;
            public override bool CanSpawnBounty(Player player)
            {
                return NPC.downedGoblins;
            }

            public override int GetBountyMonster(Player player)
            {
                switch (Main.rand.Next(3))
                {
                    default:
                        return 28;
                    case 1:
                        return 111;
                    case 2:
                        return 29;
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return Main.invasionType == InvasionID.GoblinArmy && player.ZoneOverworldHeight;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] { "dex", "ter", "try", "xa", "bi", "dri", "dab", "bad", "chun", "gus" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Leader", "Raider", "Looter", "Slaver" });
            }
        }

        public class PirateArmyBounty : BountyRegion
        {
            public override string Name => "Pirate Army";
            public override float Chance => 0.55f;
            public override bool CanSpawnBounty(Player player)
            {
                return NPC.downedPirates;
            }

            public override int GetBountyMonster(Player player)
            {
                switch (Main.rand.Next(3))
                {
                    default:
                        return 212;
                    case 1:
                        return 215;
                    case 2:
                        return 216;
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return Main.invasionType == InvasionID.PirateInvasion && player.ZoneOverworldHeight;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] { "yar", "bla", "ho", "rum", "ha", "scur", "vy", "sea", "bleh", "yo", "bot", "tle" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Yaar!", "Aaargh!", "Yo ho ho", "Scurvy" } );
            }
        }

        public class MartianMadnessBounty : BountyRegion
        {
            public override string Name => "Martian Madness";
            public override float Chance => 0.7f;
            public override bool CanSpawnBounty(Player player)
            {
                return NPC.downedMartians;
            }

            public override int GetBountyMonster(Player player)
            {
                switch (Main.rand.Next(3))
                {
                    default:
                        return 391;
                    case 1:
                        return 520;
                    case 2:
                        return 383;
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return Main.invasionType == InvasionID.MartianMadness && player.ZoneOverworldHeight;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] { "mar", "vin", "ti", "an", "scut", "lix", "gun", "ner", "scram", "bler", "gi", "ga", "zap", "per", "tes", "la" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Dissector", "Abductor", "World Conqueror", "Who Resents Humans" });
            }
        }

        public class FrostLegionBounty : BountyRegion
        {
            public override string Name => "Frost Legion";
            public override float Chance => 0.85f;
            public override bool CanSpawnBounty(Player player)
            {
                return NPC.downedFrost && Main.xMas;
            }

            public override int GetBountyMonster(Player player)
            {
                switch (Main.rand.Next(3))
                {
                    default:
                        return 144;
                    case 1:
                        return 143;
                    case 2:
                        return 145;
                }
            }

            public override bool InBountyRegion(Player player)
            {
                return Main.invasionType == InvasionID.SnowLegion && player.ZoneOverworldHeight;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] { "dan", "den", "din", "don", "dun", "frost", "stab", "by", "gang", "sta", "bal", "la", "thomp", "son" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "The Godfather", "Abductor", "World Conqueror", "Who Resents Humans", "Who Ruined Many Christmas" });
            }
        }

        public class LihzahrdDungeonBounty : BountyRegion
        {
            public override string Name => "Lihzahrd Dungeon";
            public override float Chance => 0.95f;
            public override bool CanSpawnBounty(Player player)
            {
                return NPC.downedPlantBoss;
            }

            public override int GetBountyMonster(Player player)
            {
                switch (Main.rand.Next(2))
                {
                    default:
                        return 198;
                    case 1:
                        return 226;
                }
            }

            public override bool InBountyRegion(Player player)
            {
                Tile tile = Main.tile[(int)(player.Center.X * TerraGuardian.DivisionBy16), (int)(player.Center.Y * TerraGuardian.DivisionBy16)];
                return tile != null && tile.WallType == Terraria.ID.WallID.LihzahrdBrickUnsafe;
            }

            public override string GetBountyName(int BountyID)
            {
                return NameGen(new string[] { "lih", "zah", "rd", "fly", "ing", "sna", "ke", "go", "lem" });
            }

            public override string GetBountySuffix(int BountyID)
            {
                return GetRandomString(new string[] { "Ancient", "Sun Cultist", "Mechanic", "Who Praises the Sun", "The One Who Summoned the Eclipse" });
            }
        }
        #endregion

        public class BountyRegion
        {
            public virtual string Name => "?";
            public virtual float Chance => 1;

            public virtual void SetupLoot(Player player, List<Item> PossibleRewards, float RewardMod)
            {

            }

            public virtual bool CanSpawnBounty(Player player)
            {
                return true;
            }

            public virtual int GetBountyMonster(Player player)
            {
                return 1;
            }

            public virtual bool InBountyRegion(Player player)
            {
                return false;
            }

            public virtual string GetBountyName(int BountyID)
            {
                return "Bounty";
            }

            public virtual string GetBountySuffix(int BountyID)
            {
                return "Dangerous";
            }

            public string NameGen(string[] Syllabes)
            {
                return MainMod.NameGenerator(Syllabes, false);
            }

            public string GetRandomString(string[] Texts)
            {
                return Texts[Main.rand.Next(Texts.Length)];
            }
        }
        
        public enum SpawnBiome : byte
        {
            Corruption, 
            Crimson, 
            Dungeon, 
            Jungle, 
            Underworld, 
            Hallow,
            Ocean, 
            Underground, 
            Sky, 
            OldOneArmy, 
            GoblinArmy, 
            Night, 
            Snow, 
            Desert,
            PirateArmy, 
            MartianMadness, 
            SnowLegion, 
            LihzahrdTemple
        }

        public enum Modifiers : byte
        {
            ExtraHealth,
            ExtraDamage,
            ExtraDefense,
            Armored,
            HealthRegen,
            Boss,
            Noob,
            Petrifying,
            Leader,
            FireRain,
            Fatal,
            Imobilizer,
            MeleeDefense,
            RangedDefense,
            MagicDefense,
            Sapping,
            Osmose,
            Retaliate,
            ManaBurn,
            Count
        }

        public enum DangerousModifier : byte
        {
            None,
            Reaper,
            Cyclops,
            GoldenShower,
            Haunted,
            Alchemist,
            Sharknado,
            Sapper,
            Cursed,
            Invoker
        }
    }
}