using System;
using System.Xml;

using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography.X509Certificates;

namespace Smev3Client.Test
{
    public class Smev3XmlSigner : ISmev3XmlSigner
    {
        public XmlElement SignXmlElement(XmlElement xml, string skid)
        {
            var signedXml = new SignedXml(xml) 
            { 
                SigningKey = GetSigningKeyBySkid(skid)
            };

            // Create a reference to be signed.
            Reference reference = new Reference();
            reference.Uri = "";

            // Add an enveloped transformation to the reference.            
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigExcC14NTransform());
            reference.AddTransform(new XmlDsigSmevTransform());

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            signedXml.ComputeSignature();

            return signedXml.GetXml();
        }

        private AsymmetricAlgorithm GetSigningKeyBySkid(string skid)
        {
            if (string.IsNullOrWhiteSpace(skid))
                throw new ArgumentException("Идентификатор ключа субъекта не может быть пустой строкой.", nameof(skid));

            using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine, OpenFlags.ReadOnly);

            var cert = store.Certificates.Find(X509FindType.FindByThumbprint, skid, false);
            if(cert.Count == 0)
            {
                throw new Exception($"Сертификат с идентификатором ключа субъекта {skid} не найден.");
            }

            return cert[0].PrivateKey;
        }
    }
}
