using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft_Enchantment_Cracker {
    public static class JavaRandom {
        public const long M = 0x5DEECE66DL;
        public const long Mask = (1L << 48) - 1;
        
        public static long GetSeed(int s) {
            return (s ^ M) & Mask;
        }

        public static int NextInt(this ref long seed, int max) {
            seed = (seed * M + 0xBL) & Mask;
            int r = (int)((ulong)seed >> 17);
            int m = max - 1;
            if ((max & m) == 0) return (int)((max * (long)r) >> 31);
            int u = r;
            while (u - (r = u % max) + m < 0) {
                seed = (seed * M + 0xBL) & Mask;
                u = (int)((ulong)seed >> 17);
            }
            return r;
        }

        public static int NextTwoInt(this ref long seed, int shelvesPlusOne) {
            seed = (seed * M + 0xBL) & Mask;
            int ret = (int)((ulong)seed >> 45) & 7;
            
            seed = (seed * M + 0xBL) & Mask;
            int r = (int)((ulong)seed >> 17);
            int m = shelvesPlusOne - 1;
            if ((shelvesPlusOne & m) == 0) r = (int)((shelvesPlusOne * (long)r) >> 31);
            else {
                int u = r;
                while (u - (r = u % shelvesPlusOne) + m < 0) {
                    seed = (seed * M + 0xBL) & Mask;
                    u = (int)((ulong)seed >> 17);
                }
            }

            return ret + r;
        }
        
        public static int NextTwoInt16(this ref long seed) {
            seed = (seed * M + 0xBL) & Mask;
            int first = (int)((ulong)seed >> 45) & 7;
            seed = (seed * M + 0xBL) & Mask;
            return first + ( (int)((ulong)seed >> 44) & 15);
        }
        public static int NextTwoIntP2(this ref long seed, int max) {
            seed = (seed * M + 0xBL) & Mask;
            int first = (int)((ulong)seed >> 45) & 7;

            seed = (seed * M + 0xBL) & Mask;
            return first + (int)((max * (long)((ulong)seed >> 17)) >> 31);
        }
        public static int NextTwoIntNotP2(this ref long seed, int max) {
            seed = (seed * M + 0xBL) & Mask;
            int first = (int)((ulong)seed >> 45) & 7;

            seed = (seed * M + 0xBL) & Mask;
            int r = (int)((ulong)seed >> 17);
            int val;
            do {
                seed = (seed * M + 0xBL) & Mask;
                r = (int)((ulong)seed >> 17);
                val = r % max;
            }
            while (r - val + max < -1);
            return first + val;
        }
    }

    public class CrackerTask : IProgressiveTask {
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

        private int progressAmt, progressMax;
        public float Progress { get {
            if (progressMax == -1) return (float)((long)progressAmt - (long)int.MinValue) / (float)((long)int.MaxValue - (long)int.MinValue);
            return (float)progressAmt / (float)progressMax;
        } }
        public bool Success => LastSeedsFound != 0;
        private int LastSeedsFound = -1;
        private int TheSeed;
        private long LastSearchTime = 0;
        private string LongToQty(long v) {
            if (v > 1000000000) return $"{(v/1000000000f).ToString("0.00")}B";
            if (v > 100000000) return $"{(v/1000000f).ToString("000")}M";
            if (v > 10000000) return $"{(v/1000000f).ToString("00.0")}M";
            if (v > 1000000) return $"{(v/1000000f).ToString("0.00")}M";
            if (v > 100000) return $"{(v/1000f).ToString("000")}K";
            if (v > 10000) return $"{(v/1000f).ToString("00.0")}K";
            if (v > 1000) return $"{(v/1000f).ToString("0.00")}K";
            return $"{v}";
        }
        public string ProgressText { get {
            if (LastSeedsFound == -1) {
                if (progressMax == -1) return $"{LongToQty((long)progressAmt - (long)int.MinValue)} / {LongToQty((long)int.MaxValue - (long)int.MinValue)}";
                return $"{LongToQty(progressAmt)} / {LongToQty(progressMax)}";
            }
            if (LastSeedsFound == 0) return "No seeds found";
            if (LastSeedsFound == 1) return $"Seed: {TheSeed.ToString("X8")}";
            return $"Seeds found: {LongToQty(LastSeedsFound)}";
        } }
        public string ProgressText2 { get {
            if (LastSeedsFound == -1) return $"{((int)(Progress*100)).ToString("00")}%";
            return $"Took {(LastSearchTime/1000f).ToString("0.0")}s";
        } }
        public int[] GetSeeds(int shelves, int slot1, int slot2, int slot3, int[] priorSeeds) {
            LastSeedsFound = -1;
            long start = Environment.TickCount;

            progressAmt = 0;
            progressMax = 1;

            IntArray result;

            // useful pre-computes
            int twoShelves = shelves * 2;
            int halfShelves = shelves / 2 + 1;
            int shelvesPlusOne = shelves + 1;

            // pre-computes for tests
            int slot1low = slot1 * 3 - halfShelves;
            int slot1high = slot1 * 3 + 2 - halfShelves;
            int threeSubHalf = 3 - halfShelves;
            int secondSubOne = slot2 - 1;
            bool early3 = (slot3 == twoShelves) && ((shelves + 7 + halfShelves) <= twoShelves);

            // temp values
            int ench1, ench2, ench3;
            
            long seed;

            if (priorSeeds == null) {
                // reasonable guess (based on 15:7/17/30 being ~80M)
                result = new IntArray(100000000);
                
                progressAmt = int.MinValue;
                progressMax = -1; // progress has explicit check for this
                if (shelvesPlusOne == 16) {
                    do {
                        seed = JavaRandom.GetSeed(progressAmt);
                    
                        ench1 = seed.NextTwoInt16();
                        if (ench1 < slot1low || ench1 > slot1high) { progressAmt++; continue; }

                        ench2 = (seed.NextTwoInt16() + halfShelves) * 2 / 3;
                        if (ench2 != secondSubOne) { progressAmt++; continue; }

                        result.AddValue(progressAmt++);
                    }
                    while (progressAmt != int.MinValue);
                    progressAmt = int.MaxValue;
                }
                else if ((shelvesPlusOne & -shelvesPlusOne) == shelvesPlusOne) {
                    do {
                        seed = JavaRandom.GetSeed(progressAmt);
                    
                        ench1 = seed.NextTwoIntP2(shelvesPlusOne);
                        if (ench1 < threeSubHalf) { if (slot1 != 1) { progressAmt++; continue; } }
                        if (ench1 < slot1low || ench1 > slot1high) { progressAmt++; continue; }

                        ench2 = (seed.NextTwoIntP2(shelvesPlusOne) + halfShelves) * 2 / 3;
                        if (ench2 != secondSubOne) { progressAmt++; continue; }

                        if (!early3) {
                            ench3 = (seed.NextTwoIntP2(shelvesPlusOne) + halfShelves);
                            if (Math.Max(ench3, twoShelves) != slot3) { progressAmt++; continue; }
                        }

                        result.AddValue(progressAmt++);
                    }
                    while (progressAmt != int.MinValue);
                    progressAmt = int.MaxValue;
                }
                else {
                    do {
                        seed = JavaRandom.GetSeed(progressAmt);
                    
                        ench1 = seed.NextTwoIntNotP2(shelvesPlusOne);
                        if (ench1 < threeSubHalf) { if (slot1 != 1) { progressAmt++; continue; } }
                        if (ench1 < slot1low || ench1 > slot1high) { progressAmt++; continue; }

                        ench2 = (seed.NextTwoIntNotP2(shelvesPlusOne) + halfShelves) * 2 / 3;
                        if (ench2 != secondSubOne) { progressAmt++; continue; }

                        if (!early3) {
                            ench3 = (seed.NextTwoIntNotP2(shelvesPlusOne) + halfShelves);
                            if (Math.Max(ench3, twoShelves) != slot3) { progressAmt++; continue; }
                        }

                        result.AddValue(progressAmt++);
                    }
                    while (progressAmt != int.MinValue);
                    progressAmt = int.MaxValue;
                }
            }
            else {
                // reasonable guess (+5 is for small quantities)
                result = new IntArray(priorSeeds.Length / 20 + 5);

                float total = (float)priorSeeds.Length;
                progressMax = priorSeeds.Length;
                foreach (int s in priorSeeds) {
                    // less efficient, but runs on far fewer seeds
                    seed = JavaRandom.GetSeed(s);
                    
                    ench1 = seed.NextTwoInt(shelvesPlusOne);
                    if (ench1 < threeSubHalf) { if (slot1 != 1) { progressAmt++; continue; } }
                    if (ench1 < slot1low || ench1 > slot1high) { progressAmt++; continue; }

                    ench2 = (seed.NextTwoInt(shelvesPlusOne) + halfShelves) * 2 / 3;
                    if (ench2 != secondSubOne) { progressAmt++; continue; }

                    if (!early3) {
                        ench3 = (seed.NextTwoInt(shelvesPlusOne) + halfShelves);
                        if (Math.Max(ench3, twoShelves) != slot3) { progressAmt++; continue; }
                    }

                    progressAmt++;
                    result.AddValue(s);
                }
            }

            int[] values = result.GetValues();
            LastSeedsFound = values.Length;
            if (values.Length == 1) TheSeed = values[0];
            LastSearchTime = Environment.TickCount - start;
            return values;
        }
    }
}
