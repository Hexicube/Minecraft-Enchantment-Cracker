using Minecraft_Enchantment_Cracker;
using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace Tests
{
    public static class Tests
    {
        [TestFixture]
        public static class RngTests
        {
            /// <summary>
            /// This method tests that the output of GetSeed returns a seed consistent
            /// with the constructor Random(long seed) or SetSeed
            /// </summary>
            [TestCase(1337, 25214903124L)]
            public static void GetSeed_Tests(int seed, long expected)
            {
                var output = JavaRandom.GetSeed(seed);
                Assert.AreEqual(output, expected);
            }

            /// <summary>
            /// This method tests the output of the NextInt method to ensure that it
            /// successfully changes the seed and outputs the correct value
            /// </summary>
            /// <param name="seed">The input seed to the PRNG</param>
            /// <param name="max">The maximum value passed to NextInt (exclusive)</param>
            /// <param name="expectedSeed">The expected seed after one iteration of the PRNG</param>
            /// <param name="expectedVal">The expected output of the NextInt call</param>
            [TestCase(1337L, 420, 33712326537040L, 419)]
            [TestCase(123456789L, 8, 119305093197820L, 3)]
            [TestCase(999999999L, 1, 94003067820958L, 0)]
            public static void NextInt_Tests(ref long seed, ref int max, long expectedSeed, int expectedVal)
            {
                var result = seed.NextInt(max);
                Assert.AreEqual(expectedSeed, seed);
                Assert.AreEqual(expectedVal, result);
            }
        }

        [TestFixture]
        public static class CrackingTests
        {
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
                int[] results = null;
                var len = shelves.Length;
                for(var i = 0; i < len; ++i)
                {
                    results = cracker.GetSeeds(shelves[i], slots1[i], slots2[i], slots3[i], results);
                    Assert.AreEqual(seedCounts[i], results.Length);
                }

                Assert.NotNull(results);
                Assert.AreEqual(finalSeed, results[0]);
            }
        }
    }
}
