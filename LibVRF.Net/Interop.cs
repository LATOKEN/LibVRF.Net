// ReSharper disable InconsistentNaming

namespace LibVRF.Net
{
    [SymbolName(nameof(evaluate))]
    public unsafe delegate int evaluate(
        byte* privateKey, int privateKeyLen,
        byte* message, int messageLen,
        byte* result, int maxResultSize
    );

    [SymbolName(nameof(verify))]
    public unsafe delegate bool verify(
        byte* publicKey, int publicKeyLen,
        byte* proof, int proofLen,
        byte* message, int messageLen
    );

    [SymbolName(nameof(proof_to_hash))]
    public unsafe delegate int proof_to_hash(
        byte* proof, int proofLen,
        byte* result, int maxResultSize
    );
}