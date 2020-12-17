using System.Security.Cryptography;

namespace Smev3Client.Crypt
{
    public class GostAsymmetricSignatureFormatter : AsymmetricSignatureFormatter
    {
        private GostAsymmetricAlgorithm _key;

        public override byte[] CreateSignature(byte[] rgbHash)
        {
            return _key.CreateHashSignature(rgbHash);
        }

        public override void SetHashAlgorithm(string strName)
        {
            
        }

        public override void SetKey(AsymmetricAlgorithm key)
        {
            _key = (GostAsymmetricAlgorithm)key;
        }
    }
}
