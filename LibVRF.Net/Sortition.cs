using System.Linq;
using System.Numerics;

namespace LibVRF.Net
{
    public static class Sortition
    {
        public static BigInteger GetVotes(byte[] hash, BigInteger weight, BigInteger tau, BigInteger fullWeight)
        {
            var hashValue = new BigInteger(hash.Reverse().Append((byte) 0).ToArray());
            var maxValue = BigInteger.Pow(2, 256);
            var value = hashValue * BigInteger.Pow(10, 18) / maxValue;
            for (var j = 0; j <= weight; ++j)
            {
                var cur = SumMember(j, weight, tau, fullWeight);
                if (cur > value) return j;
                value -= cur;
            }

            return 0;
        }

        private static BigInteger SumMember(BigInteger j, BigInteger weight, BigInteger tau, BigInteger fullWeight)
        {
            var (a1, a2) = Binomial(j, weight, tau, fullWeight);
            return a1 * BigInteger.Pow(10, 18) / a2;
        }

        private static (BigInteger, BigInteger) Binomial(
            BigInteger k, BigInteger weight, BigInteger tau, BigInteger fullWeight
        )
        {
            var (a1, a2) = Combination(weight, k);
            var b1 = BigInteger.Pow(tau, (int) k);
            var b2 = BigInteger.Pow(fullWeight, (int) k);
            // require W > tau
            var c1 = BigInteger.Pow(fullWeight - tau, (int) (weight - k));
            var c2 = BigInteger.Pow(fullWeight, (int) (weight - k));

            return (a1 * b1 * c1, a2 * c2 * b2);
        }

        private static (BigInteger, BigInteger) Combination(BigInteger m, BigInteger n)
        {
            return (Factorial(m, n), Factorial(n, n));
        }

        private static BigInteger Factorial(BigInteger m, BigInteger n)
        {
            var num = new BigInteger(1);
            var count = 0;
            for (var i = m; i > 0; i--)
            {
                if (count == n)
                {
                    break;
                }

                num *= i;
                count++;
            }

            return num;
        }
    }
}