using System;
using System.Collections.Generic;

namespace Minecraft_Enchantment_Cracker.Tasks
{
    public class CrackerTask : IProgressiveTask
    {
        private int progressAmt, progressMax;

        public float Progress
        {
            get
            {
                if(progressMax == -1)
                    return(float)((long)progressAmt - (long)int.MinValue) /
                          (float)((long)int.MaxValue - (long)int.MinValue);

                return(float)progressAmt / (float)progressMax;
            }
        }

        public  bool Success => LastSeedsFound != 0;
        private int  LastSeedsFound = -1;
        private int  TheSeed;
        private long LastSearchTime;

        private static string LongToQty(long v)
        {
            if(v > 1000000000)
                return$"{v / 1000000000f:0.00}B";

            if(v > 100000000)
                return$"{v / 1000000f:000}M";

            if(v > 10000000)
                return$"{v / 1000000f:00.0}M";

            if(v > 1000000)
                return$"{v / 1000000f:0.00}M";

            if(v > 100000)
                return$"{v / 1000f:000}K";

            if(v > 10000)
                return$"{v / 1000f:00.0}K";

            if(v > 1000)
                return$"{v / 1000f:0.00}K";

            return$"{v}";
        }

        public string ProgressText
        {
            get
            {
                switch(LastSeedsFound) {
                    case -1:
                        return progressMax == -1 ? $"{LongToQty((long)progressAmt - (long)int.MinValue)} / {LongToQty((long)int.MaxValue - (long)int.MinValue)}" : $"{LongToQty(progressAmt)} / {LongToQty(progressMax)}";
                    case 0:
                        return"No seeds found";
                    case 1:
                        return$"Seed: {TheSeed:X8}";
                    default:
                        return$"Seeds found: {LongToQty(LastSeedsFound)}";
                }
            }
        }

        public string ProgressText2 => LastSeedsFound == -1 ? $"{(int)(Progress * 100):00}%" : $"Took {LastSearchTime / 1000f:0.0}s";

        public List<int> GetSeeds(int shelves, int slot1, int slot2, int slot3, List<int> priorSeeds)
        {
            LastSeedsFound = -1;
            long start = Environment.TickCount;

            progressMax = 1;

            List<int> result;

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

            if(priorSeeds == null)
            {
                // reasonable guess (based on 15:7/17/30 being ~80M)
                result = new List<int>(100000000);

                progressAmt = int.MinValue;
                progressMax = -1; // progress has explicit check for this
                if(shelvesPlusOne == 16)
                {
                    do
                    {
                        seed = progressAmt ^ JavaRandom.M & JavaRandom.Mask;

                        seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                        var temprng = (int)((ulong)seed >> 45) & 7;
                        seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                        ench1 = temprng + ((int)((ulong)seed >> 44) & 15);
                        if(ench1 < slot1low || ench1 > slot1high)
                        {
                            progressAmt++;
                            continue;
                        }

                        seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                        temprng = (int)((ulong)seed >> 45) & 7;
                        seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                        ench2 = (temprng + ((int)((ulong)seed >> 44) & 15) + halfShelves) * 2 / 3;
                        if(ench2 != secondSubOne)
                        {
                            progressAmt++;
                            continue;
                        }

                        result.Add(progressAmt++);
                    } while(progressAmt != int.MinValue);

                    progressAmt = int.MaxValue;
                }
                else if((shelvesPlusOne & -shelvesPlusOne) == shelvesPlusOne)
                {
                    do
                    {
                        seed = progressAmt ^ JavaRandom.M & JavaRandom.Mask;
                        seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                        var temprng = (int)((ulong)seed >> 45) & 7;

                        seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                        ench1 = temprng + (int)((shelvesPlusOne * (long)((ulong)seed >> 17)) >> 31);
                        if(ench1 < threeSubHalf)
                            if(slot1 != 1)
                            {
                                progressAmt++;
                                continue;
                            }

                        if(ench1 < slot1low || ench1 > slot1high)
                        {
                            progressAmt++;
                            continue;
                        }

                        seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                        temprng = (int)((ulong)seed >> 45) & 7;

                        seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                        ench2 = (temprng + (int)((shelvesPlusOne * (long)((ulong)seed >> 17)) >> 31) + halfShelves) *
                                2 /
                                3;

                        if(ench2 != secondSubOne)
                        {
                            progressAmt++;
                            continue;
                        }

                        if(!early3)
                        {
                            seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                            temprng = (int)((ulong)seed >> 45) & 7;

                            seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                            ench3 = temprng + (int)((shelvesPlusOne * (long)((ulong)seed >> 17)) >> 31) + halfShelves;
                            if(Math.Max(ench3, twoShelves) != slot3)
                            {
                                progressAmt++;
                                continue;
                            }
                        }

                        result.Add(progressAmt++);
                    } while(progressAmt != int.MinValue);

                    progressAmt = int.MaxValue;
                }
                else
                {
                    do
                    {
                        seed = progressAmt ^ JavaRandom.M & JavaRandom.Mask;

                        seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                        var temprng = (int)((ulong)seed >> 45) & 7;

                        seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                        int tempval;
                        int r;
                        do
                        {
                            seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                            r = (int)((ulong)seed >> 17);
                            tempval = r % shelvesPlusOne;
                        } while(r - tempval + shelvesPlusOne < -1);

                        ench1 = temprng + tempval;
                        if(ench1 < threeSubHalf)
                            if(slot1 != 1)
                            {
                                progressAmt++;
                                continue;
                            }

                        if(ench1 < slot1low || ench1 > slot1high)
                        {
                            progressAmt++;
                            continue;
                        }

                        seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                        temprng = (int)((ulong)seed >> 45) & 7;

                        seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                        do
                        {
                            seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                            r = (int)((ulong)seed >> 17);
                            tempval = r % shelvesPlusOne;
                        } while(r - tempval + shelvesPlusOne < -1);

                        ench2 = (temprng + tempval + halfShelves) * 2 / 3;
                        if(ench2 != secondSubOne)
                        {
                            progressAmt++;
                            continue;
                        }

                        if(!early3)
                        {
                            seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                            temprng = (int)((ulong)seed >> 45) & 7;

                            seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                            do
                            {
                                seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                                r = (int)((ulong)seed >> 17);
                                tempval = r % shelvesPlusOne;
                            } while(r - tempval + shelvesPlusOne < -1);

                            ench3 = (temprng + tempval + halfShelves);
                            if(Math.Max(ench3, twoShelves) != slot3)
                            {
                                progressAmt++;
                                continue;
                            }
                        }

                        result.Add(progressAmt++);
                    } while(progressAmt != int.MinValue);

                    progressAmt = int.MaxValue;
                }
            }
            else
            {
                // Allow for cleaner pre-increment
                progressAmt = -1;
                // reasonable guess (+5 is for small quantities)
                result = new List<int>(priorSeeds.Count / 20 + 5);

                progressMax = priorSeeds.Count;
                for(var i = 0; i < progressMax; i++)
                {
                    ++progressAmt;
                    var s = priorSeeds[i];
                    // less efficient, but runs on far fewer seeds
                    seed = s ^ JavaRandom.M & JavaRandom.Mask;

                    seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                    var temprng = (int)((ulong)seed >> 45) & 7;

                    seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                    var temprng2 = (int)((ulong)seed >> 17);
                    if((shelvesPlusOne & shelvesPlusOne - 1) == 0)
                        temprng2 = (int)((shelvesPlusOne * (long)temprng2) >> 31);
                    else
                        while(temprng2 - (temprng2 %= shelvesPlusOne) + shelvesPlusOne - 1 < 0)
                        {
                            seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                            temprng2 = (int)((ulong)seed >> 17);
                        }

                    ench1 = temprng + temprng2;
                    if(ench1 < threeSubHalf)
                        if(slot1 != 1)
                            continue;

                    if(ench1 < slot1low || ench1 > slot1high)
                        continue;

                    seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                    temprng = (int)((ulong)seed >> 45) & 7;

                    seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                    temprng2 = (int)((ulong)seed >> 17);
                    if((shelvesPlusOne & shelvesPlusOne - 1) == 0)
                        temprng2 = (int)((shelvesPlusOne * (long)temprng2) >> 31);
                    else
                        while(temprng2 - (temprng2 %= shelvesPlusOne) + shelvesPlusOne - 1 < 0)
                        {
                            seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                            temprng2 = (int)((ulong)seed >> 17);
                        }

                    ench2 = (temprng + temprng2 + halfShelves) * 2 / 3;
                    if(ench2 != secondSubOne)
                        continue;

                    if(!early3)
                    {
                        seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                        temprng = (int)((ulong)seed >> 45) & 7;

                        seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                        temprng2 = (int)((ulong)seed >> 17);
                        if((shelvesPlusOne & shelvesPlusOne - 1) == 0)
                            temprng2 = (int)((shelvesPlusOne * (long)temprng2) >> 31);
                        else
                            while(temprng2 - (temprng2 %= shelvesPlusOne) + shelvesPlusOne - 1 < 0)
                            {
                                seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                                temprng2 = (int)((ulong)seed >> 17);
                            }

                        ench3 = temprng + temprng2 + halfShelves;
                        if(Math.Max(ench3, twoShelves) != slot3)
                            continue;
                    }

                    result.Add(s);
                }
            }

            LastSeedsFound = result.Count;
            if(LastSeedsFound == 1)
                TheSeed = result[0];

            LastSearchTime = Environment.TickCount - start;
            return result;
        }
    }
}
