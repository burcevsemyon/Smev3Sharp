using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Шаг 1 СМЭВ: инструкции обработки не попадают в результат (декларация XML уже не клонируется в CloneNode).
        /// </summary>
        [TestMethod]
        public void GetOutput_RemovesProcessingInstructions()
        {
            var input = Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?><root><?pi-test data?></root>");
            var output = Transform(input);

            var anyPi = output
                .SelectNodes("//processing-instruction()")
                ?.Cast<XmlNode>()
                .Any();
            Assert.IsFalse(anyPi ?? false);
            Assert.AreEqual("root", output.DocumentElement?.LocalName);
        }

        // --- Ниже: ожидаемое поведение по спецификации СМЭВ 3.x (см. smev-transform.md).
        // Текущий XmlDsigSmevTransform эти шаги не выполняет — снять [Ignore] при реализации.

        [TestMethod]
        [Ignore("СМЭВ шаг 2: удаление текстовых узлов, состоящих только из пробельных символов (≤ U+0020).")]
        public void GetOutput_Step2_RemovesWhitespaceOnlyTextBetweenElements()
        {
            var input = Parse("<root>\n\t <inner xmlns=\"http://x\" />\n </root>");
            var output = Transform(input);

            var root = output.DocumentElement;
            Assert.IsNotNull(root);
            Assert.AreEqual(1, root.ChildNodes.Count);
            Assert.AreEqual(XmlNodeType.Element, root.FirstChild?.NodeType);
        }

        [TestMethod]
        [Ignore("СМЭВ шаг 3: пустой элемент после канонизации — пара тегов <tag></tag>, не самозакрывающаяся форма.")]
        public void GetOutput_Step3_EmptyElementBecomesExplicitOpenClosePair()
        {
            var input = Parse("<root xmlns=\"http://x\"><leaf xmlns=\"http://x\"/></root>");
            var output = Transform(input);

            var leaf = output.DocumentElement?.FirstChild as XmlElement;
            Assert.IsNotNull(leaf);
            Assert.IsFalse(leaf.IsEmpty, "По спецификации СМЭВ пустой тег должен стать парой открывающий/закрывающий.");
        }

        [TestMethod]
        [Ignore("СМЭВ шаг 4: неиспользуемые объявления xmlns на элементе удаляются.")]
        public void GetOutput_Step4_RemovesUnusedNamespaceDeclarations()
        {
            const string u = "http://test/1";
            const string unused = "http://unused";
            var input = Parse(
                $"<elementOne xmlns=\"{u}\" xmlns:qwe=\"{unused}\"><child xmlns=\"{u}\"/></elementOne>"
            );
            var output = Transform(input);

            var root = output.DocumentElement;
            Assert.IsNotNull(root);
            CollectionAssert.DoesNotContain(
                AttributeNames(root).ToList(),
                "xmlns:qwe",
                "Префикс qwe нигде не используется — объявление должно быть удалено."
            );
        }

        [TestMethod]
        [Ignore("СМЭВ шаг 7: атрибуты без префикса сортируются по локальному имени (attA перед attB).")]
        public void GetOutput_Step7_UnprefixedAttributesSortedLexicographically()
        {
            const string u = "http://test/1";
            var input = Parse(
                $"<elementOne xmlns=\"{u}\"><elementTwo xmlns=\"{u}\" attB=\"bbb\" attA=\"aaa\"/></elementOne>"
            );
            var output = Transform(input);

            var two = output.DocumentElement?.FirstChild as XmlElement;
            Assert.IsNotNull(two);
            var names = NonNamespaceAttributeLocalNames(two);
            CollectionAssert.AreEqual(new[] { "attA", "attB" }, names);
        }

        [TestMethod]
        [Ignore("СМЭВ шаг 8: объявления xmlns располагаются перед обычными атрибутами (после сортировки).")]
        public void GetOutput_Step8_XmlnsDeclarationsBeforeRegularAttributes()
        {
            const string u = "http://test/1";
            var input = Parse(
                $"<elementOne xmlns=\"{u}\"><elementTwo xmlns=\"{u}\" z=\"last\" attA=\"a\"/></elementOne>"
            );
            var output = Transform(input);

            var two = output.DocumentElement?.FirstChild as XmlElement;
            Assert.IsNotNull(two);
            var ordered = AttributeOrderForSpecCheck(two);
            var seenNonXmlns = false;
            foreach (var name in ordered)
            {
                var isXmlns = name.StartsWith("xmlns", StringComparison.Ordinal);
                Assert.IsFalse(
                    isXmlns && seenNonXmlns,
                    "Объявления xmlns должны идти в начале элемента, перед обычными атрибутами."
                );
                if (!isXmlns)
                {
                    seenNonXmlns = true;
                }
            }
        }

        [TestMethod]
        [Ignore("СМЭВ шаг 9.1: CDATA заменяется извлечённым текстом (секция не сохраняется).")]
        public void GetOutput_Step9_CDataSectionIsUnwrappedToText()
        {
            var input = Parse("<root><![CDATA[<not-a-tag>]]></root>");
            var output = Transform(input);

            var root = output.DocumentElement;
            Assert.IsNotNull(root);
            Assert.AreEqual(1, root.ChildNodes.Count);
            Assert.AreEqual(XmlNodeType.Text, root.FirstChild?.NodeType);
            Assert.AreEqual("<not-a-tag>", root.FirstChild?.Value);
        }

        [TestMethod]
        [Ignore("СМЭВ: полный пример из smev-transform.md (префиксы, сортировка атрибутов, пустой элемент как пара тегов).")]
        public void GetOutput_MatchesSmevTransformDocSample()
        {
            var input = Parse(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                    + "<elementOne xmlns=\"http://test/1\" xmlns:qwe=\"http://test/2\">\n"
                    + "    <qwe:elementTwo attB=\"bbb\" attA=\"aaa\"/>\n"
                    + "</elementOne>"
            );
            var output = Transform(input);

            const string expected =
                "<ns1:elementOne xmlns:ns1=\"http://test/1\">"
                    + "<ns2:elementTwo xmlns:ns2=\"http://test/2\" attA=\"aaa\" attB=\"bbb\">"
                    + "</ns2:elementTwo>"
                    + "</ns1:elementOne>";

            Assert.AreEqual(expected, output.OuterXml);
        }

        private static IEnumerable<string> AttributeNames(XmlElement e)
        {
            if (e.Attributes == null)
            {
                yield break;
            }

            foreach (XmlAttribute a in e.Attributes)
            {
                yield return a.Name;
            }
        }

        /// <summary>
        /// Локальные имена атрибутов не из пространства имён xmlns (для проверки шага 7).
        /// </summary>
        private static List<string> NonNamespaceAttributeLocalNames(XmlElement e)
        {
            var list = new List<string>();
            if (e.Attributes == null)
            {
                return list;
            }

            foreach (XmlAttribute a in e.Attributes)
            {
                if (a.NamespaceURI == "http://www.w3.org/2000/xmlns/")
                {
                    continue;
                }

                list.Add(a.LocalName);
            }

            return list;
        }

        private static List<string> AttributeOrderForSpecCheck(XmlElement e)
        {
            var list = new List<string>();
            if (e.Attributes == null)
            {
                return list;
            }

            foreach (XmlAttribute a in e.Attributes)
            {
                list.Add(a.Name);
            }

            return list;
        }
    }
}
