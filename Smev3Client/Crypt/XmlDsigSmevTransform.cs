using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Smev3Client
{
    class XmlDsigSmevTransform : Transform
    {
        private static readonly Type[] _inputOutputTypes = new[] { typeof(XmlDocument) };        

        private XmlDocument _inputDocument;

        public const string ALGORITHM = "urn://smev-gov-ru/xmldsig/transform";

        public XmlDsigSmevTransform()
        {
            Algorithm = ALGORITHM;
        }

        public override Type[] InputTypes => _inputOutputTypes;

        public override Type[] OutputTypes => _inputOutputTypes;

        XmlDocument GetNodeDoc(XmlNode node)
        {
            return node.OwnerDocument ?? node as XmlDocument;
        }

        void CloneAttributes(
            XmlNode dstNode, 
            XmlNode srcNode,
            Stack<(string prefix, string namespaceURI)?> namespacesStack,
            ref int nsIndex)
        {
            if (srcNode.Attributes == null)
            {
                return;
            }

            var dstDocument = GetNodeDoc(dstNode);
            for (int i = 0; i < srcNode.Attributes?.Count; i++)
            {
                var srcAttrubute = srcNode.Attributes[i];

                var prefix = "";
                var localName = srcAttrubute.LocalName;
                var namespaceURI = srcAttrubute.NamespaceURI;
                if (srcAttrubute.LocalName == "xmlns")
                {   
                    prefix = "xmlns";
                    var @namespace = namespacesStack.FirstOrDefault(i => i.Value.namespaceURI == srcNode.NamespaceURI);
                    if (@namespace == null)
                    {
                        @namespace = (prefix: $"ns{++nsIndex}", namespaceURI: srcNode.NamespaceURI);
                        namespacesStack.Push(@namespace);
                    }

                    localName = @namespace.Value.prefix;                    
                }

                var newAttribute = dstDocument.CreateAttribute(
                                                    prefix,
                                                    localName,
                                                    namespaceURI);

                newAttribute.Value = srcAttrubute.Value;

                dstNode.Attributes.Append(newAttribute);
            }
        }

        void CloneNode(
            XmlNode dstParentNode,
            XmlNode srcNode,
            Stack<(string prefix, string namespaceURI)?> namespacesStack,
            ref int nsIndex)
        {
            if (srcNode.NodeType == XmlNodeType.XmlDeclaration
                || srcNode.NodeType == XmlNodeType.ProcessingInstruction
                || srcNode.NodeType == XmlNodeType.Whitespace)
            {
                return;
            }


            if (srcNode.NodeType == XmlNodeType.Attribute)
            {
                return;
            }

            bool popNamespace = false;

            var prefix = string.Empty;
            var namespaceURI = string.Empty;
            if (srcNode.NodeType == XmlNodeType.Element)
            {
                var @namespace = namespacesStack.FirstOrDefault(i => i.Value.namespaceURI == srcNode.NamespaceURI);
                if (@namespace == null)
                {
                    @namespace = (prefix: $"ns{++nsIndex}", namespaceURI: srcNode.NamespaceURI);
                    namespacesStack.Push(@namespace);

                    popNamespace = true;
                }

                prefix = @namespace.Value.prefix;
                namespaceURI = @namespace.Value.namespaceURI;
            }

            var dstDocument = GetNodeDoc(dstParentNode);

            var newNode = dstDocument.CreateNode(
                                    srcNode.NodeType,
                                    prefix: prefix,
                                    srcNode.LocalName,
                                    namespaceURI: namespaceURI);

            CloneAttributes(newNode, srcNode, namespacesStack, ref nsIndex);

            if (srcNode.NodeType != XmlNodeType.Element)
            {
                newNode.Value = srcNode.Value;
            }

            dstParentNode.AppendChild(newNode);

            for (int i = 0; i < srcNode.ChildNodes.Count; i++)
            {
                CloneNode(newNode, srcNode.ChildNodes[i], namespacesStack, ref nsIndex);
            }

            if (popNamespace)
            {
                namespacesStack.Pop();
            }
        }

        public override object GetOutput()
        {
            var nsIndex = 0;

            var namespaceStack = new Stack<(string prefix, string namespaceURI)?>();

            var outDocument = new XmlDocument() { PreserveWhitespace = true };

            for (int i = 0; i < _inputDocument.ChildNodes.Count; i++)
            {
                CloneNode(
                        outDocument,
                        _inputDocument.ChildNodes[i],
                        namespaceStack,
                        ref nsIndex);               
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

        public override void LoadInput(object obj)
        {
            _inputDocument = obj as XmlDocument;

            if (_inputDocument == null)
            {
                throw new ArgumentException("Тип параметра должен быть XmlDocument.");
            }
        }

        protected override XmlNodeList GetInnerXml()
        {
            return null;
        }
    }
}

