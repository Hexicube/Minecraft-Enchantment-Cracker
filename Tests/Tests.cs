using System.Collections.Generic;
using Minecraft_Enchantment_Cracker.Tasks;
using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace Tests
{
    public static class Tests
    {
        [TestFixture]
        public static class CrackingTests
        {
            [TestCase(15, 7, 17, 30, null, 81788565)]
            public static void CrackRng_Test(
                int shelves,
                int slot1,
                int slot2,
                int slot3,
                List<int> priorSeeds,
                int expectedSeeds
            )
            {
                List<int> results = new CrackerTask().GetSeeds(shelves, slot1, slot2, slot3, priorSeeds);
                Assert.AreEqual(expectedSeeds, results.Count);
            }

            [TestCase(15, 7, 17, 30, null, 81788565, 14, 7, 15, 28, 2073151)]
            public static void CrackRngMultiple_Test(
                int shelves1,
                int s11,
                int s21,
                int s31,
                List<int> priorSeeds,
                int expected1,
                int shelves2,
                int s12,
                int s22,
                int s32,
                int expected2
            )
            {
                var cracker = new CrackerTask();
                List<int> results = cracker.GetSeeds(shelves1, s11, s21, s31, priorSeeds);
                Assert.AreEqual(expected1, results.Count);
                List<int> results2 = cracker.GetSeeds(shelves2, s12, s22, s32, results);
                Assert.AreEqual(expected2, results2.Count);
            }

            [TestCase(
                new[] {15, 14, 13, 12, 11, 10, 9},
                new[] {6, 5, 4, 6, 5, 5, 5},
                new[] {10, 16, 10, 13, 11, 7, 7},
                new[] {30, 28, 26, 24, 22, 20, 18},
                new[] {44040339, 498996, 6692, 219, 27, 2, 1},
                614847255
            )]
            public static void CrackRngAll_Test(
                int[] shelves,
                int[] slots1,
                int[] slots2,
                int[] slots3,
                int[] seedCounts,
                int finalSeed
            )
            {
                var cracker = new CrackerTask();
                List<int> results = null;
                var len = shelves.Length;
                for(var i = 0; i < len; ++i)
                {
                    results = cracker.GetSeeds(shelves[i], slots1[i], slots2[i], slots3[i], results);
                    Assert.AreEqual(seedCounts[i], results.Count);
                }

                Assert.NotNull(results);
                Assert.AreEqual(finalSeed, results[0]);
            }
        }
    }
}
