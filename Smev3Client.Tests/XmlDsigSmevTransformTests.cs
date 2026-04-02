using System;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Smev3Client.Crypt;

namespace Smev3Client.Tests
{
    [TestClass]
    public class XmlDsigSmevTransformTests
    {
        private static XmlDocument Parse(string xml, bool preserveWhitespace = true)
        {
            var doc = new XmlDocument { PreserveWhitespace = preserveWhitespace };
            doc.LoadXml(xml);
            return doc;
        }

        private static XmlDocument Transform(XmlDocument input)
        {
            var t = new XmlDsigSmevTransform();
            t.LoadInput(input);
            return (XmlDocument)t.GetOutput();
        }

        [TestMethod]
        public void Constructor_SetsSmevAlgorithmUri()
        {
            var t = new XmlDsigSmevTransform();

            Assert.AreEqual(XmlDsigSmevTransform.ALGORITHM, t.Algorithm);
        }

        [TestMethod]
        public void LoadInput_Null_ThrowsArgumentException()
        {
            var t = new XmlDsigSmevTransform();

            var ex = Assert.ThrowsException<ArgumentException>(() => t.LoadInput(null));

            Assert.AreEqual("Тип параметра должен быть XmlDocument.", ex.Message);
        }

        [TestMethod]
        public void LoadInput_WrongType_ThrowsArgumentException()
        {
            var t = new XmlDsigSmevTransform();

            var ex = Assert.ThrowsException<ArgumentException>(() => t.LoadInput("not a document"));

            Assert.AreEqual("Тип параметра должен быть XmlDocument.", ex.Message);
        }

        [TestMethod]
        public void GetOutput_SimpleElement_PreservesLocalNameAndText()
        {
            var input = Parse("<root>hello</root>");
            var output = Transform(input);

            Assert.AreEqual("root", output.DocumentElement?.LocalName);
            Assert.AreEqual("hello", output.DocumentElement?.InnerText);
        }

        [TestMethod]
        public void GetOutput_SetsPreserveWhitespaceTrueOnResult()
        {
            var input = Parse("<r/>");
            var output = Transform(input);

            Assert.IsTrue(output.PreserveWhitespace);
        }

        [TestMethod]
        public void GetOutput_NestedSameNamespace_ReusesPrefix()
        {
            const string u = "http://example.com/ns";
            var input = Parse($"<a xmlns=\"{u}\"><b xmlns=\"{u}\">x</b></a>");
            var output = Transform(input);

            var a = output.DocumentElement;
            var b = a?.FirstChild as XmlElement;

            Assert.IsNotNull(b);
            Assert.AreEqual(u, a.NamespaceURI);
            Assert.AreEqual(u, b.NamespaceURI);
            Assert.AreEqual(a.Prefix, b.Prefix);
        }

        [TestMethod]
        public void GetOutput_SiblingSameNamespace_GetsDistinctPrefixes()
        {
            const string u = "http://example.com/ns";
            var input = Parse($"<root><a xmlns=\"{u}\"/><b xmlns=\"{u}\"/></root>");
            var output = Transform(input);

            var root = output.DocumentElement;
            var a = root?.FirstChild as XmlElement;
            var b = a?.NextSibling as XmlElement;

            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.AreEqual(u, a.NamespaceURI);
            Assert.AreEqual(u, b.NamespaceURI);
            StringAssert.StartsWith(a.Prefix, "ns");
            StringAssert.StartsWith(b.Prefix, "ns");
            Assert.AreNotEqual(a.Prefix, b.Prefix);
        }

        [TestMethod]
        public void GetOutput_TwoDifferentNamespaces_PreservesBothUris()
        {
            const string u1 = "http://example.com/one";
            const string u2 = "http://example.com/two";
            var input = Parse($"<root><a xmlns=\"{u1}\"/><b xmlns=\"{u2}\"/></root>");
            var output = Transform(input);

            var root = output.DocumentElement;
            var a = root?.FirstChild as XmlElement;
            var b = a?.NextSibling as XmlElement;

            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.AreEqual(u1, a.NamespaceURI);
            Assert.AreEqual(u2, b.NamespaceURI);
        }

        [TestMethod]
        public void GetOutput_XmlDeclarationOnly_ProducesEmptyDocument()
        {
            var input = new XmlDocument { PreserveWhitespace = true };
            input.AppendChild(input.CreateXmlDeclaration("1.0", "utf-8", null));

            var output = Transform(input);

            Assert.IsNull(output.DocumentElement);
            Assert.AreEqual(0, output.ChildNodes.Count);
        }

        [TestMethod]
        public void GetOutput_WithPreserveWhitespace_PreservesTextWhitespace()
        {
            var input = Parse("<root>  t  </root>", preserveWhitespace: true);
            var output = Transform(input);

            Assert.AreEqual("  t  ", output.DocumentElement?.InnerText);
        }

        [TestMethod]
        public void GetOutput_WithXmlnsDeclarationAndPrefixedAttribute_DoesNotThrowAndPreservesAttributeNamespace()
        {
            var input = Parse("<root xmlns:a=\"urn:test:a\"><a:item a:id=\"42\"/></root>");
            var output = Transform(input);

            var item = output.DocumentElement?.FirstChild as XmlElement;
            Assert.IsNotNull(item);

            Assert.AreEqual("urn:test:a", item.NamespaceURI);

            var namespacedAttr = item.Attributes?["id", "urn:test:a"];
            Assert.IsNotNull(namespacedAttr);
            Assert.AreEqual("42", namespacedAttr.Value);
        }

        [TestMethod]
        public void GetOutput_TypeOverload_ThrowsNotImplementedException()
        {
            var t = new XmlDsigSmevTransform();
            t.LoadInput(Parse("<r/>"));

            Assert.ThrowsException<NotImplementedException>(() => t.GetOutput(typeof(XmlDocument)));
        }

        [TestMethod]
        public void LoadInnerXml_ThrowsNotImplementedException()
        {
            var t = new XmlDsigSmevTransform();

            Assert.ThrowsException<NotImplementedException>(() => t.LoadInnerXml(null));
        }
    }
}
