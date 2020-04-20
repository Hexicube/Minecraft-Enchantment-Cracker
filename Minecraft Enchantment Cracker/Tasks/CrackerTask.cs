using System;

namespace Minecraft_Enchantment_Cracker.Tasks {
    public class CrackerTask : IProgressiveTask {
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
                        if(ench1 < threeSubHalf) { if (slot1 != 1) { progressAmt++; continue; } }
                        if(ench1 < slot1low || ench1 > slot1high) { progressAmt++; continue; }

                        ench2 = (seed.NextTwoIntP2(shelvesPlusOne) + halfShelves) * 2 / 3;
                        if(ench2 != secondSubOne) { progressAmt++; continue; }

                        if(!early3) {
                            ench3 = (seed.NextTwoIntP2(shelvesPlusOne) + halfShelves);
                            if(Math.Max(ench3, twoShelves) != slot3) { progressAmt++; continue; }
                        }

                        result.AddValue(progressAmt++);
                    }
                    while(progressAmt != int.MinValue);
                    progressAmt = int.MaxValue;
                }
                else {
                    do {
                        seed = JavaRandom.GetSeed(progressAmt);

                        ench1 = seed.NextTwoIntNotP2(shelvesPlusOne);
                        if(ench1 < threeSubHalf)
                            if(slot1 != 1) { progressAmt++; continue; }

                        if(ench1 < slot1low || ench1 > slot1high) { progressAmt++; continue; }

                        ench2 = (seed.NextTwoIntNotP2(shelvesPlusOne) + halfShelves) * 2 / 3;
                        if(ench2 != secondSubOne) { progressAmt++; continue; }

                        if(!early3) {
                            ench3 = seed.NextTwoIntNotP2(shelvesPlusOne) + halfShelves;
                            if(Math.Max(ench3, twoShelves) != slot3) { progressAmt++; continue; }
                        }

                        result.AddValue(progressAmt++);
                    }
                    while(progressAmt != int.MinValue);
                    progressAmt = int.MaxValue;
                }
            }
            else
            {
                // Allow for cleaner pre-increment
                progressAmt = -1;
                // reasonable guess (+5 is for small quantities)
                result = new IntArray(priorSeeds.Length / 20 + 5);

                float total = priorSeeds.Length;
                progressMax = priorSeeds.Length;
                for(var i = 0; i < total; i++)
                {
                    ++progressAmt;
                    var s = priorSeeds[i];
                    // less efficient, but runs on far fewer seeds
                    seed = JavaRandom.GetSeed(s);

                    ench1 = seed.NextTwoInt(shelvesPlusOne);
                    if(ench1 < threeSubHalf)
                        if(slot1 != 1)
                            continue;

                    if(ench1 < slot1low || ench1 > slot1high)
                        continue;

                    ench2 = (seed.NextTwoInt(shelvesPlusOne) + halfShelves) * 2 / 3;
                    if(ench2 != secondSubOne)
                        continue;

                    if(!early3)
                    {
                        ench3 = (seed.NextTwoInt(shelvesPlusOne) + halfShelves);
                        if(Math.Max(ench3, twoShelves) != slot3)
                            continue;
                    }

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
