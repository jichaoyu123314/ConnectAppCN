#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC.Rfc8032;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters
{
    public sealed class Ed25519PrivateKeyParameters
        : AsymmetricKeyParameter
    {
        public static readonly int KeySize = Ed25519.SecretKeySize;
        public static readonly int SignatureSize = Ed25519.SignatureSize;

        private readonly byte[] data = new byte[KeySize];

        public Ed25519PrivateKeyParameters(SecureRandom random)
            : base(true)
        {
            Ed25519.GeneratePrivateKey(random, data);
        }

        public Ed25519PrivateKeyParameters(byte[] buf, int off)
            : base(true)
        {
            Array.Copy(buf, off, data, 0, KeySize);
        }

        public Ed25519PrivateKeyParameters(Stream input)
            : base(true)
        {
            if (KeySize != Streams.ReadFully(input, data))
                throw new EndOfStreamException("EOF encountered in middle of Ed25519 private key");
        }

        public void Encode(byte[] buf, int off)
        {
            Array.Copy(data, 0, buf, off, KeySize);
        }

        public byte[] GetEncoded()
        {
            return Arrays.Clone(data);
        }

        public Ed25519PublicKeyParameters GeneratePublicKey()
        {
            byte[] publicKey = new byte[Ed25519.PublicKeySize];
            Ed25519.GeneratePublicKey(data, 0, publicKey, 0);
            return new Ed25519PublicKeyParameters(publicKey, 0);
        }

        public void Sign(Ed25519.Algorithm algorithm, Ed25519PublicKeyParameters publicKey, byte[] ctx, byte[] msg, int msgOff, int msgLen,
            byte[] sig, int sigOff)
        {
            byte[] pk = new byte[Ed25519.PublicKeySize];
            if (null == publicKey)
            {
                Ed25519.GeneratePublicKey(data, 0, pk, 0);
            }
            else
            {
                publicKey.Encode(pk, 0);
            }

            switch (algorithm)
            {
            case Ed25519.Algorithm.Ed25519:
            {
                if (null != ctx)
                    throw new ArgumentException("ctx");

                Ed25519.Sign(data, 0, pk, 0, msg, msgOff, msgLen, sig, sigOff);
                break;
            }
            case Ed25519.Algorithm.Ed25519ctx:
            {
                Ed25519.Sign(data, 0, pk, 0, ctx, msg, msgOff, msgLen, sig, sigOff);
                break;
            }
            case Ed25519.Algorithm.Ed25519ph:
            {
                if (Ed25519.PrehashSize != msgLen)
                    throw new ArgumentException("msgLen");

                Ed25519.SignPrehash(data, 0, pk, 0, ctx, msg, msgOff, sig, sigOff);
                break;
            }
            default:
            {
                throw new ArgumentException("algorithm");
            }
            }
        }
    }
}
#pragma warning restore
#endif
