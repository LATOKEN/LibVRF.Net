using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using NUnit.Framework;

namespace LibVRF.Net.Test
{
    public class VrfTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void EvaluationTest()
        {
            var privateKey = "D95D6DB65F3E2223703C5D8E205D98E3E6B470F067B0F94F6C6BF73D4301CE48".HexToBytes();
            var seed = "74657374".HexToBytes();
            var role = "7374616b6572".HexToBytes();
            var (proof, value, j) = Vrf.Evaluate(privateKey, seed, role, 22, 999, 999);

            Assert.AreEqual(
                "037f4f7190ca66feddf3c52bc1fd103334c457dc3cd3625924e4f98666d5c80d40c9d771f0a6b6b576938a07027ce393ec5d84eafdd1945952b72923c505e348af92ee986859d0e263ce86c20b000c5b8d",
                proof.ToHex()
            );
            Assert.AreEqual("abfecf1cbcc6e18b8663159b715b4298e7b32e590adf87a7c56c6dd9cac47893", value.ToHex());
            Assert.AreEqual((BigInteger) 24, j);
        }

        [Test]
        public void VerificationTest()
        {
            var publicKey = "02e5974f3e1e9599ff5af036b5d6057d80855e7182afb4c2fa1fe38bc6efb9072b".HexToBytes();
            var seed = "74657374".HexToBytes();
            var role = "7374616b6572".HexToBytes();
            var proof =
                "037f4f7190ca66feddf3c52bc1fd103334c457dc3cd3625924e4f98666d5c80d40c9d771f0a6b6b576938a07027ce393ec5d84eafdd1945952b72923c505e348af92ee986859d0e263ce86c20b000c5b8d"
                    .HexToBytes();
            var success = Vrf.IsWinner(publicKey, proof, seed, role, 22, 999, 999);
            Assert.True(success);
        }
    }

    public static class HexExtensions
    {
        public static byte[] HexToBytes(this string buffer)
        {
            if (string.IsNullOrEmpty(buffer))
                return new byte[0];
            if (buffer.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                buffer = buffer.Substring(2);
            if (buffer.Length % 2 == 1)
                throw new FormatException();
            var result = new byte[buffer.Length / 2];
            for (var i = 0; i < result.Length; i++)
                result[i] = byte.Parse(buffer.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
            return result;
        }

        public static string ToHex(this IEnumerable<byte> buffer)
        {
            var sb = new StringBuilder();
            foreach (var b in buffer)
                sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }
    }
}