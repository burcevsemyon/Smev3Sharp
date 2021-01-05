using System;
using System.Xml;

using System.Security.Cryptography.Xml;

using Smev3Client.Crypt;

namespace Smev3Client
{
    public class Smev3XmlSigner : ISmev3XmlSigner
    {
        private readonly GostAsymmetricAlgorithm _algorithm;

        public Smev3XmlSigner(GostAsymmetricAlgorithm algorithm)
        {
            _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
        } 

        public XmlElement SignXmlElement(XmlElement xml, string uri)
        {
            var signedXml = new SignedXml(xml)
            {
                SigningKey = _algorithm                
            };

            signedXml.SafeCanonicalizationMethods.Add("urn://smev-gov-ru/xmldsig/transform");

            Reference reference = new Reference
            {
                Uri = $"#{uri}",
                DigestMethod = "urn:ietf:params:xml:ns:cpxmlsec:algorithms:gostr34112012-256"
            };

            signedXml.AddReference(reference);

            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
            signedXml.SignedInfo.SignatureMethod = "urn:ietf:params:xml:ns:cpxmlsec:algorithms:gostr34102012-gostr34112012-256";

            signedXml.KeyInfo.AddClause(new KeyInfoX509Data(_algorithm.CertRawData));

            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigExcC14NTransform());
            reference.AddTransform(new XmlDsigSmevTransform());

            signedXml.ComputeSignature();

            return signedXml.GetXml();
        }
    }
}
