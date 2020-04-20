namespace Minecraft_Enchantment_Cracker.Tasks {
    public static class JavaRandom {
        public const long M    = 0x5DEECE66DL;
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
}
