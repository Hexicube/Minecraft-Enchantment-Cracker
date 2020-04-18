using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft_Enchantment_Cracker {
    public static class MinecraftData {
        public sealed class MinecraftVersion {
            private MinecraftVersion() { }

            public static MinecraftVersion[] VERSIONS = {
                new MinecraftVersion { Name = "1.8", Sort = 1 },
                new MinecraftVersion { Name = "1.9 - 1.10", Sort = 2 },
                new MinecraftVersion { Name = "1.11.0", Sort = 3 },
                new MinecraftVersion { Name = "1.11.1 - 1.12", Sort = 4 },
                new MinecraftVersion { Name = "1.13", Sort = 5 },
                new MinecraftVersion { Name = "1.14.0 - 1.14.2", Sort = 6 },
                new MinecraftVersion { Name = "1.14.3 - 1.15", Sort = 7 },
                new MinecraftVersion { Name = "1.16", Sort = 8 },
            };

            // used for item list
            public static bool VerAtLeast(MinecraftVersion thisVer, int targetSort) => thisVer.Sort >= targetSort;

            public string Name;
            private int Sort;
        }

        public static string[][] ItemNames = new string[][] {
            new string[] { "netherite_helmet",  "netherite_chestplate", "netherite_leggings", "netherite_boots", "netherite_sword", "netherite_pickaxe", "netherite_axe", "netherite_shovel", "netherite_hoe" },
            new string[] { "diamond_hemlet",    "diamond_chestplate",   "diamond_leggings",   "diamond_boots",   "diamond_sword",   "diamond_pickaxe",   "diamond_axe",   "diamond_shovel",   "diamond_hoe" },
            new string[] { "gold_hemlet",       "golden_chestplate",    "golden_leggings",    "golden_boots",    "golden_sword",    "golden_pickaxe",    "golden_axe",    "golden_shovel",    "golden_hoe" },
            new string[] { "iron_hemlet",       "iron_chestplate",      "iron_leggings",      "iron_boots",      "iron_sword",      "iron_pickaxe",      "iron_axe",      "iron_shovel",      "iron_hoe" },
            new string[] { "leather_hemlet",    "leather_chestpiece",   "leather_leggings",   "leather_boots",   "stone_sword",     "stone_pickaxe",     "stone_axe",     "stone_shovel",     "stone_hoe" },
            new string[] { "turtle_hemlet",     null,                   null,                 null,              "wooden_sword",    "wooden_pickaxe",    "wooden_axe",    "wooden_shovel",    "wooden_hoe" },
            new string[] { null,                null,                   null,                 null,              "trident",         "book",              "bow",           "fishing_rod",      "crossbow" },
        };

        public static bool[][] GetAvailability(MinecraftVersion ver) {
            bool turtle = MinecraftVersion.VerAtLeast(ver, 5);
            bool crossbow = MinecraftVersion.VerAtLeast(ver, 6);
            bool netherite = MinecraftVersion.VerAtLeast(ver, 8);
            return new bool[][] {
                new bool[] { netherite, netherite, netherite, netherite, netherite, netherite, netherite, netherite, netherite },
                new bool[] { true, true, true, true, true, true, true, true, true },
                new bool[] { true, true, true, true, true, true, true, true, true },
                new bool[] { true, true, true, true, true, true, true, true, true },
                new bool[] { true, true, true, true, true, true, true, true, true },
                new bool[] { turtle, false, false, false, true, true, true, true, true },
                new bool[] { false, false, false, false, turtle, true, true, true, crossbow }
            };
        }
    }
}
