using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LibVRF.Net
{
    public class Vrf
    {
        internal readonly Lazy<evaluate> EvaluateDelegate;
        internal readonly Lazy<verify> VerifyDelegate;
        internal readonly Lazy<proof_to_hash> ProofToHashDelegate;

        const string Lib = "vrf";

        private static readonly Lazy<string> LibPathLazy = new Lazy<string>(() => LibPathResolver.Resolve(Lib));
        private static readonly Lazy<IntPtr> LibPtr = new Lazy<IntPtr>(() => LoadLibNative.LoadLib(LibPathLazy.Value));

        internal static Vrf Imports = new Vrf();

        private const int MaxProofSize = 1024, MaxHashSize = 32;

        private Vrf()
        {
            // load all delegates
            EvaluateDelegate = LazyDelegate<evaluate>();
            VerifyDelegate = LazyDelegate<verify>();
            ProofToHashDelegate = LazyDelegate<proof_to_hash>();
        }


        Lazy<TDelegate> LazyDelegate<TDelegate>()
        {
            var symbol = SymbolNameCache<TDelegate>.SymbolName;
            return new Lazy<TDelegate>(
                () => LoadLibNative.GetDelegate<TDelegate>(LibPtr.Value, symbol),
                true
            );
        }

        public static (byte[], byte[], BigInteger) Evaluate(
            Span<byte> privateKey, Span<byte> seed, Span<byte> role,
            BigInteger tau, BigInteger weight, BigInteger fullWeight
        )
        {
            var message = Combine(seed, role);
            unsafe
            {
                Span<byte> proof = stackalloc byte[MaxProofSize];
                fixed (byte* privateKeyPtr = privateKey)
                fixed (byte* messagePtr = message)
                fixed (byte* proofPtr = proof)
                {
                    var proofLen = Imports.EvaluateDelegate.Value(
                        privateKeyPtr, privateKey.Length,
                        messagePtr, message.Length,
                        proofPtr, MaxProofSize
                    );
                    if (proofLen == 0) throw new InvalidOperationException("libvrf: evaluate did not return proof");
                    var resultingHash = ProofToHash(proof.Slice(0, proofLen));
                    var resultingProof = proof.Slice(0, proofLen).ToArray();
                    var j = Sortition.GetVotes(resultingHash, weight, tau, fullWeight);
                    return (resultingProof, resultingHash, j);
                }
            }
        }

        public static bool IsWinner(
            byte[] publicKey, byte[] proof, byte[] seed, byte[] role,
            BigInteger tau, BigInteger weight, BigInteger fullWeight
        )
        {
            var message = Combine(seed, role);
            unsafe
            {
                fixed (byte* proofPtr = proof)
                fixed (byte* publicKeyPtr = publicKey)
                fixed (byte* messagePtr = message)
                {
                    var success = Imports.VerifyDelegate.Value(
                        publicKeyPtr, publicKey.Length, proofPtr, proof.Length, messagePtr, message.Length
                    );
                    if (!success) return false;
                }
            }

            var hash = ProofToHash(proof);
            var j = Sortition.GetVotes(hash, weight, tau, fullWeight);
            return j > 0;
        }

        public static byte[] ProofToHash(Span<byte> proof)
        {
            unsafe
            {
                Span<byte> hash = stackalloc byte[MaxHashSize];
                fixed (byte* proofPtr = proof)
                fixed (byte* hashPtr = hash)
                {
                    var hashLen = Imports.ProofToHashDelegate.Value(proofPtr, proof.Length, hashPtr, MaxHashSize);
                    if (hashLen == 0) throw new InvalidOperationException("libvrf: proof_to_hash did not return hash");
                    return hash.Slice(0, hashLen).ToArray();
                }
            }
        }

        private static byte[] Combine(Span<byte> first, Span<byte> second)
        {
            var result = new byte[first.Length + second.Length];
            first.CopyTo(result);
            second.CopyTo(result.AsSpan().Slice(first.Length));
            return result;
        }
    }
}