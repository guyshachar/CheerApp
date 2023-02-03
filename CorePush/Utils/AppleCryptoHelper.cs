using System;
using System.Security.Cryptography;

namespace CorePush.Utils
{
    public static class AppleCryptoHelper
    {
        public static ECDsa GetEllipticCurveAlgorithm(string privateKey)
        {
            var keyParams = (ECPrivateKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));
            var q = keyParams.Parameters.G.Multiply(keyParams.D).Normalize();

            var curve = ECCurve.CreateFromValue(keyParams.PublicKeyParamSet.Id);
            var ecd = new ECParameters
            {
                Curve = curve,
                D = keyParams.D.ToByteArrayUnsigned(),
                Q =
                {
                    X = q.XCoord.GetEncoded(),
                    Y = q.YCoord.GetEncoded()
                }
            };

            try
            {
                var ecdsa = ECDsa.Create(ecd);

                return ecdsa;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}