using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                switch(LastSeedsFound)
                {
                    case-1:
                        return progressMax == -1
                            ? $"{LongToQty((long)progressAmt - (long)int.MinValue)} / {LongToQty((long)int.MaxValue - (long)int.MinValue)}"
                            : $"{LongToQty(progressAmt)} / {LongToQty(progressMax)}";
                    case 0:
                        return"No seeds found";
                    case 1:
                        return$"Seed: {TheSeed:X8}";
                    default:
                        return$"Seeds found: {LongToQty(LastSeedsFound)}";
                }
            }
        }

        public string ProgressText2 =>
            LastSeedsFound == -1 ? $"{(int)(Progress * 100):00}%" : $"Took {LastSearchTime / 1000f:0.0}s";

        public SynchronizedCollection<int> GetSeeds(
            int shelves,
            int slot1,
            int slot2,
            int slot3,
            List<int> priorSeeds
        )
        {
            LastSeedsFound = -1;
            progressMax = 1;

            var results = new SynchronizedCollection<int>();
            if(priorSeeds == null)
            {
                progressAmt = int.MinValue;
                progressMax = -1;
                var threads = Environment.ProcessorCount;
                var blockSize = (int.MaxValue - (long)int.MinValue) / threads;
                var tasks = new Task[threads];
                for(var i = 0; i < threads; ++i)
                {
                    var i1 = i;
                    tasks[i] = Task.Factory.StartNew(
                        () => Get15SeedsWorker(
                            shelves,
                            slot1,
                            slot2,
                            (int)(int.MinValue + i1 * blockSize),
                            i1 == threads - 1 ? int.MaxValue : (int)(int.MinValue + (i1 + 1) * blockSize) - 1,
                            results
                        )
                    );
                }

                Task.WaitAll(tasks);
            }
            else
            {
                progressMax = priorSeeds.Count;
                progressAmt = -1;
                Task.Factory.StartNew(() => GetSeedsWorker(shelves, slot1, slot2, slot3, priorSeeds.ToList(), results)).Wait();

                LastSeedsFound = results.Count;
                if(LastSeedsFound == 1)
                    TheSeed = results[0];
            }

            return results;
        }

        public void Get15SeedsWorker(
            int shelves,
            int slot1,
            int slot2,
            int lowSeed,
            int highSeed,
            SynchronizedCollection<int> results
        )
        {
            var halfShelves = shelves / 2 + 1;
            var slot1Low = slot1 * 3 - halfShelves;
            var slot1High = slot1 * 3 + 2 - halfShelves;

            var len = highSeed - lowSeed + 1;
            for(var i = 0; i < len; ++i)
            {
                // TODO: This is sad
                if(i % (2 << 4) == 0)
                    Interlocked.Add(ref progressAmt, 2 << 4);

                //Interlocked.Increment(ref progressAmt);
                var s = lowSeed + i;
                var seed = s ^ JavaRandom.M & JavaRandom.Mask;

                seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                var temprng = (int)((ulong)seed >> 45) & 7;
                seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                var ench1 = temprng + ((int)((ulong)seed >> 44) & 15);
                if(ench1 < slot1Low || ench1 > slot1High)
                    continue;

                seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                temprng = (int)((ulong)seed >> 45) & 7;
                seed = (seed * JavaRandom.M + 0xBL) & JavaRandom.Mask;
                var ench2 = (temprng + ((int)((ulong)seed >> 44) & 15) + halfShelves) * 2 / 3;
                if(ench2 != slot2 - 1)
                    continue;

                results.Add(s);
            }
        }

        public void GetSeedsWorker(
            int shelves,
            int slot1,
            int slot2,
            int slot3,
            List<int> priorSeeds,
            SynchronizedCollection<int> results
        )
        {
            // useful pre-computes
            int twoShelves = shelves * 2;
            int halfShelves = shelves / 2 + 1;
            int shelvesPlusOne = shelves + 1;

            // pre-computes for tests
            int slot1Low = slot1 * 3 - halfShelves;
            int slot1High = slot1 * 3 + 2 - halfShelves;
            int threeSubHalf = 3 - halfShelves;
            int secondSubOne = slot2 - 1;
            bool early3 = (slot3 == twoShelves) && ((shelves + 7 + halfShelves) <= twoShelves);

            var priorCount = priorSeeds.Count;
            for(var i = 0; i < priorCount; i++)
            {
                // TODO: MAKE THIS BETTERER
                Interlocked.Increment(ref progressAmt);
                var s = priorSeeds[i];
                // less efficient, but runs on far fewer seeds
                var seed = s ^ JavaRandom.M & JavaRandom.Mask;

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

                var ench1 = temprng + temprng2;
                if(ench1 < threeSubHalf)
                    if(slot1 != 1)
                        continue;

                if(ench1 < slot1Low || ench1 > slot1High)
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

                var ench2 = (temprng + temprng2 + halfShelves) * 2 / 3;
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

                    var ench3 = temprng + temprng2 + halfShelves;
                    if(Math.Max(ench3, twoShelves) != slot3)
                        continue;
                }

                results.Add(s);
            }
        }

       /* public List<int> GetSeeds(int shelves, int slot1, int slot2, int slot3, List<int> priorSeeds)
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

            LastSeedsFound = result.Count;
            if(LastSeedsFound == 1)
                TheSeed = result[0];

            LastSearchTime = Environment.TickCount - start;
            return result;
        }*/
    }
}
