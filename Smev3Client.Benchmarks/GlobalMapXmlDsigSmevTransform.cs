using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Smev3Client.Benchmarks
{
    internal class GlobalMapXmlDsigSmevTransform : Transform
    {
        private static readonly Type[] InputOutputTypes = { typeof(XmlDocument) };
        private XmlDocument? _inputDocument;

        public const string ALGORITHM = "urn://smev-gov-ru/xmldsig/transform";

        public GlobalMapXmlDsigSmevTransform()
        {
            Algorithm = ALGORITHM;
        }

        public override Type[] InputTypes => InputOutputTypes;
        public override Type[] OutputTypes => InputOutputTypes;

        private static void CloneAttributes(
            XmlDocument dstDocument,
            XmlNode dstNode,
            XmlNode srcNode,
            Dictionary<string, string> namespaceToPrefix,
            ref int nsIdx
        )
        {
            var srcAttributes = srcNode.Attributes;
            var dstAttributes = dstNode.Attributes;
            if (srcAttributes == null || srcAttributes.Count == 0 || dstAttributes == null)
            {
                return;
            }

            for (var i = 0; i < srcAttributes.Count; i++)
            {
                var srcAttr = srcAttributes[i]!;
                var prefix = srcAttr.Prefix;
                var localName = srcAttr.LocalName;
                var namespaceUri = srcAttr.NamespaceURI;

                if (srcAttr.Prefix == "xmlns" || (srcAttr.Prefix.Length == 0 && localName == "xmlns"))
                {
                    prefix = "xmlns";
                    localName = GetOrAddPrefixForUri(namespaceToPrefix, srcAttr.Value, ref nsIdx);
                }

                var newAttr = dstDocument.CreateAttribute(prefix, localName, namespaceUri);
                newAttr.Value = srcAttr.Value;
                dstAttributes.Append(newAttr);
            }
        }

        private static string GetOrAddPrefixForUri(
            Dictionary<string, string> namespaceToPrefix,
            string uri,
            ref int nsIdx
        )
        {
            if (namespaceToPrefix.TryGetValue(uri, out var prefix))
            {
                return prefix;
            }

            var newPrefix = $"ns{++nsIdx}";
            namespaceToPrefix[uri] = newPrefix;
            return newPrefix;
        }

        private static void CloneNode(
            XmlDocument dstDocument,
            XmlNode dstParentNode,
            XmlNode srcNode,
            Dictionary<string, string> namespaceToPrefix,
            ref int nsIdx
        )
        {
            switch (srcNode.NodeType)
            {
                case XmlNodeType.XmlDeclaration:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Whitespace:
                case XmlNodeType.Attribute:
                    return;
            }

            var prefix = string.Empty;
            var nsUri = string.Empty;
            if (srcNode.NodeType == XmlNodeType.Element)
            {
                prefix = GetOrAddPrefixForUri(namespaceToPrefix, srcNode.NamespaceURI, ref nsIdx);
                nsUri = srcNode.NamespaceURI;
            }

            var newNode = dstDocument.CreateNode(
                srcNode.NodeType,
                prefix: prefix,
                srcNode.LocalName,
                namespaceURI: nsUri
            );

            CloneAttributes(dstDocument, newNode, srcNode, namespaceToPrefix, ref nsIdx);

            if (srcNode.NodeType != XmlNodeType.Element)
            {
                newNode.Value = srcNode.Value;
            }

            dstParentNode.AppendChild(newNode);

            var childNodes = srcNode.ChildNodes;
            for (var i = 0; i < childNodes.Count; i++)
            {
                CloneNode(dstDocument, newNode, childNodes[i]!, namespaceToPrefix, ref nsIdx);
            }
        }

        public override object GetOutput()
        {
            if (_inputDocument is null)
            {
                throw new InvalidOperationException("Документ не загружен");
            }

            var nsIdx = 0;
            var namespaceToPrefix = new Dictionary<string, string>(StringComparer.Ordinal);
            var outDocument = new XmlDocument { PreserveWhitespace = true };

            var childNodes = _inputDocument.ChildNodes;
            for (var i = 0; i < childNodes.Count; i++)
            {
                CloneNode(outDocument, outDocument, childNodes[i]!, namespaceToPrefix, ref nsIdx);
            }

            return outDocument;
        }

        public override object GetOutput(Type type)
        {
            throw new NotImplementedException();
        }

        public override void LoadInnerXml(XmlNodeList nodeList)
        {
            throw new NotImplementedException();
        }

        protected override XmlNodeList GetInnerXml()
        {
            throw new NotImplementedException();
        }

        public override void LoadInput(object obj)
        {
            _inputDocument = obj as XmlDocument;

            if (_inputDocument == null)
            {
                throw new ArgumentException($"Тип параметра должен быть {nameof(XmlDocument)}.");
            }
        }
    }
}
