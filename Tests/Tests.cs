using Minecraft_Enchantment_Cracker.Tasks;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class RngTests
    {
        /// <summary>
        /// This method tests that the output of GetSeed returns a seed consistent
        /// with the constructor Random(long seed) or SetSeed
        /// </summary>
        [Test]
        public static void GetSeed_Tests()
        {
            const int inputSeed = 1337;
            var output = JavaRandom.GetSeed(inputSeed);
            const long expectedOutput = 25214903124;
            Assert.AreEqual(output, expectedOutput);
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
}
