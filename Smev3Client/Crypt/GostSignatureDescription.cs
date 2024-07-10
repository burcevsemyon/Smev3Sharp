using System.Security.Cryptography;

namespace Smev3Client.Crypt
{
    public class GostSignatureDescription : SignatureDescription
    {
        public override AsymmetricSignatureFormatter CreateFormatter(AsymmetricAlgorithm key)
        {
            var formatter = new GostAsymmetricSignatureFormatter();

            formatter.SetKey(key);

            return formatter;
        }

        public override HashAlgorithm CreateDigest()
        {
            return new GostR3411_2012_256HashAlgorithm();
        }
    }
}
