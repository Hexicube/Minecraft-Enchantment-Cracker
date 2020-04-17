using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft_Enchantment_Cracker {
    public static class JavaRandom {
        private const long multiplier = 0x5DEECE66DL;
        private const long mask = (1L << 48) - 1;
        
        public static long GetSeed(long s) {
            return (s ^ multiplier) & mask;
        }

        public static int NextInt(this ref long seed, int max) {
            int r;
            seed = (seed * multiplier + 0xBL) & mask;
            r = (int)(seed >> 17) | (int)(seed << (64-17));
            int m = max - 1;
            if ((max & m) == 0) return (int)((max * (long)r) >> 31);
            int u = r;
            while (u - (r = u % max) + m < 0) {
                seed = (seed * multiplier + 0xBL) & mask;
                u = (int)(seed >> 17) | (int)(seed << (64-17));
            }
            return r;
        }
    }

    public static class Cracker {
        public class IntArray {
            private int[] values;
            private int size = 0;

            public IntArray(int startSize = 1000000) {
                values = new int[startSize];
            }

            public void AddValue(int v) {
                if (size == values.Length) {
                    Debug.WriteLine($"Exceeded size: {size}");
                    int[] newValues = new int[values.Length + 1000000];
                    Array.Copy(values, newValues, values.Length);
                    values = newValues;
                }
                values[size++] = v;
            }

            public void AddAllValues(int[] v) {
                if (size + v.Length > values.Length) {
                    int[] newValues = new int[values.Length + v.Length];
                    Array.Copy(values, newValues, values.Length);
                    Array.Copy(v, 0, newValues, values.Length, v.Length);
                    values = newValues;
                }
                else Array.Copy(v, 0, values, size, v.Length);
                size += v.Length;
            }

            public int[] GetValues() {
                int[] ret = new int[size];
                Array.Copy(values, ret, size);
                return ret;
            }
        }

        private static int GetGenericEnchantability(ref long random, int shelves) {
            int first = random.NextInt(8);
            int second = random.NextInt(shelves+1);
            return first + 1 + (shelves >> 1) + second;
        }

        private static int GetLevelsSlot1(ref long random, int shelves) {
            int enchantability = GetGenericEnchantability(ref random, shelves) / 3;
            return enchantability < 1 ? 1 : enchantability;
        }

        private static int GetLevelsSlot2(ref long random, int shelves) {
            return GetGenericEnchantability(ref random, shelves) * 2 / 3 + 1;
        }

        private static int GetLevelsSlot3(ref long random, int shelves) {
            int enchantability = GetGenericEnchantability(ref random, shelves);
            int twiceShelves = shelves * 3;
            return enchantability < twiceShelves ? twiceShelves : enchantability;
        }

        public static int[] GetSeeds(int shelves, int slot1, int slot2, int slot3, int[] priorSeeds, ref float progress) {
            IntArray result;

            // useful pre-computes
            int twoShelves = shelves * 2;
            int halfShelves = shelves / 2 + 1;
            int shelvesPlusOne = shelves + 1;
            int firstEarly = slot1 * 3 + 1;
            int secondEarly = slot2 * 3 / 2;
            int secondSubOne = slot2 - 1;

            // temp values
            int ench1r1, ench1, ench2r1, ench2, ench3;
            
            long seed;

            if (priorSeeds == null) {
                // reasonable guess (based on 15:7/17/30)
                result = new IntArray(100000000);

                float total = (float)int.MaxValue - (float)int.MinValue;
                int s = int.MinValue;
                do {
                    progress = ((float)s - (float)int.MinValue) / total;

                    seed = JavaRandom.GetSeed(s);

                    ench1r1 = seed.NextInt(8) + halfShelves;
                    if (ench1r1 > firstEarly) { s++; continue; }
                    ench1 = (ench1r1 + seed.NextInt(shelvesPlusOne)) / 3;
                    if (ench1 < 1) { if (slot1 != 1) { s++; continue; } }
                    if (ench1 != slot1) { s++; continue; }

                    ench2r1 = seed.NextInt(8) + halfShelves;
                    if (ench2r1 > secondEarly) { s++; continue; }
                    ench2 = (ench2r1 + seed.NextInt(shelvesPlusOne)) * 2 / 3;
                    if (ench2 != secondSubOne) { s++; continue; }

                    ench3 = (seed.NextInt(8) + halfShelves + seed.NextInt(shelvesPlusOne));
                    if (Math.Max(ench3, twoShelves) != slot3) { s++; continue; }

                    result.AddValue(s++);
                }
                while (s != int.MinValue);
            }
            else {
                // reasonable guess (+5 is for small quantities)
                result = new IntArray(priorSeeds.Length / 20 + 5);

                float total = (float)priorSeeds.Length;
                foreach (int s in priorSeeds) {
                    progress = (float)s / total;

                    seed = JavaRandom.GetSeed(s);

                    ench1r1 = seed.NextInt(8) + halfShelves;
                    if (ench1r1 > firstEarly) continue;
                    ench1 = (ench1r1 + seed.NextInt(shelvesPlusOne)) / 3;
                    if (ench1 < 1) { if (slot1 != 1) continue; }
                    if (ench1 != slot1) continue;

                    ench2r1 = seed.NextInt(8) + halfShelves;
                    if (ench2r1 > secondEarly) continue;
                    ench2 = (ench2r1 + seed.NextInt(shelvesPlusOne)) * 2 / 3;
                    if (ench2 != secondSubOne) continue;

                    ench3 = (seed.NextInt(8) + halfShelves + seed.NextInt(shelvesPlusOne));
                    if (Math.Max(ench3, twoShelves) != slot3) continue;

                    result.AddValue(s);
                }
            }

            return result.GetValues();
        }
    }
}
