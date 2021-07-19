using System;
using System.Xml;
using System.Security.Cryptography.Xml;

namespace Smev3Client.Crypt
{
    internal class Smev3XmlSigner : ISmev3XmlSigner
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

            signedXml.SafeCanonicalizationMethods.Add(XmlDsigSmevTransform.ALGORITHM);

            Reference reference = new Reference
            {
                Uri = $"#{uri}",
                DigestMethod = XmlDsigConsts.XmlDsigGost3411_2012_256Url
            };

            signedXml.AddReference(reference);

            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
            signedXml.SignedInfo.SignatureMethod = XmlDsigConsts.XmlDsigGost3410_2012_256Url;

            signedXml.KeyInfo.AddClause(new KeyInfoX509Data(_algorithm.CertRawData));

            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigExcC14NTransform());
            reference.AddTransform(new XmlDsigSmevTransform());

            signedXml.ComputeSignature();

            return signedXml.GetXml();
        }
    }
}
