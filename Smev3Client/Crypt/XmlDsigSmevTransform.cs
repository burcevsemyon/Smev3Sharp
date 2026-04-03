using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Smev3Client.Crypt
{
    internal class XmlDsigSmevTransform : Transform
    {
        private static readonly Type[] InputOutputTypes = { typeof(XmlDocument) };
        private XmlDocument? _inputDocument;

        public const string ALGORITHM = "urn://smev-gov-ru/xmldsig/transform";

        public XmlDsigSmevTransform()
        {
            Algorithm = ALGORITHM;
        }

        public override Type[] InputTypes => InputOutputTypes;
        public override Type[] OutputTypes => InputOutputTypes;

        private static XmlDocument GetNodeDoc(XmlNode node)
        {
            return node.OwnerDocument ?? (XmlDocument)node;
        }

        private static void CloneAttributes(
            XmlNode dstNode,
            XmlNode srcNode,
            Stack<(string prefix, string namespaceUri)> namespaces,
            ref int nsIdx
        )
        {
            if (
                srcNode.Attributes == null
                || srcNode.Attributes.Count == 0
                || dstNode.Attributes == null
            )
            {
                return;
            }

            var dstDocument = GetNodeDoc(dstNode);
            for (var i = 0; i < srcNode.Attributes.Count; i++)
            {
                var srcAttr = srcNode.Attributes[i];
                var prefix = srcAttr.Prefix;
                var localName = srcAttr.LocalName;
                var namespaceUri = srcAttr.NamespaceURI;

                if (
                    srcAttr.Prefix == "xmlns"
                    || (srcAttr.Prefix.Length == 0 && localName == "xmlns")
                )
                {
                    prefix = "xmlns";
                    localName = GetOrAddPrefixForUri(namespaces, srcAttr.Value, ref nsIdx);
                }

                var newAttr = dstDocument.CreateAttribute(prefix, localName, namespaceUri);
                newAttr.Value = srcAttr.Value;
                dstNode.Attributes.Append(newAttr);
            }
        }

        private static string GetOrAddPrefixForUri(
            Stack<(string prefix, string namespaceUri)> stack,
            string uri,
            ref int nsIdx
        )
        {
            foreach (var ns in stack)
            {
                if (ns.namespaceUri == uri)
                {
                    return ns.prefix;
                }
            }

            var res = (prefix: $"ns{++nsIdx}", namespaceUri: uri);
            stack.Push(res);
            return res.prefix;
        }

        private static void CloneNode(
            XmlNode dstParentNode,
            XmlNode srcNode,
            Stack<(string prefix, string namespaceUri)> namespaces,
            ref int nsIdx
        )
        {
            if (
                srcNode.NodeType == XmlNodeType.XmlDeclaration
                || srcNode.NodeType == XmlNodeType.ProcessingInstruction
                || srcNode.NodeType == XmlNodeType.Whitespace
                || srcNode.NodeType == XmlNodeType.Attribute
            )
            {
                return;
            }

            var popNs = false;
            var prefix = string.Empty;
            var nsUri = string.Empty;

            if (srcNode.NodeType == XmlNodeType.Element)
            {
                var nsIdxOld = nsIdx;
                prefix = GetOrAddPrefixForUri(namespaces, srcNode.NamespaceURI, ref nsIdx);
                nsUri = srcNode.NamespaceURI;
                popNs = nsIdx > nsIdxOld;
            }

            var dstDocument = GetNodeDoc(dstParentNode);
            var newNode = dstDocument.CreateNode(
                srcNode.NodeType,
                prefix: prefix,
                srcNode.LocalName,
                namespaceURI: nsUri
            );

            CloneAttributes(newNode, srcNode, namespaces, ref nsIdx);

            if (srcNode.NodeType != XmlNodeType.Element)
            {
                newNode.Value = srcNode.Value;
            }

            dstParentNode.AppendChild(newNode);

            for (var i = 0; i < srcNode.ChildNodes.Count; i++)
            {
                CloneNode(newNode, srcNode.ChildNodes[i], namespaces, ref nsIdx);
            }

            if (popNs)
            {
                namespaces.Pop();
            }
        }

        public override object GetOutput()
        {
            if (_inputDocument is null)
            {
                throw new InvalidOperationException("Документ не загружен");
            }

            var nsIdx = 0;
            var namespaces = new Stack<(string prefix, string namespaceUri)>();
            var outDocument = new XmlDocument { PreserveWhitespace = true };

            var nodesCount = _inputDocument.ChildNodes.Count;
            for (var i = 0; i < nodesCount; i++)
            {
                CloneNode(outDocument, _inputDocument.ChildNodes[i], namespaces, ref nsIdx);
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

            if (_inputDocument is null)
            {
                throw new ArgumentException($"Тип параметра должен быть {nameof(XmlDocument)}.");
            }
        }
    }
}
