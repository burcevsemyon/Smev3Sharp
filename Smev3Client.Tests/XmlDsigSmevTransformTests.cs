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

        [TestMethod]
        public void GetOutput_WithoutLoadInput_ThrowsInvalidOperationException()
        {
            var t = new XmlDsigSmevTransform();

            var ex = Assert.ThrowsException<InvalidOperationException>(() => t.GetOutput());

        }

        // --- Ниже: ожидаемое поведение по спецификации СМЭВ 3.x (см. smev-transform.md).
        // Текущий XmlDsigSmevTransform эти шаги не выполняет — снять [Ignore] при реализации.
        //
        // Сводка по полному алгоритму (прил. А МР 3.5.0.28 / smev-transform.md):
        // Шаг 1 — ПИ и декларация: частично (ПИ и XmlDeclaration не клонируются; пробельные XmlNodeType.Whitespace отбрасываются).
        // Шаг 2 — «пустые» текстовые узлы между тегами: не делается (остаётся XmlNodeType.Text из пробелов при PreserveWhitespace).
        // Шаг 3 — пустой элемент → пара тегов: не делается (IsEmpty сохраняется).
        // Шаг 4 — неиспользуемые xmlns: не удаляются.
        // Шаг 5 — недостающие объявления префиксов: не гарантируется отдельно от клонирования.
        // Шаг 6 — префиксы ns1, ns2…: частично (сквозная нумерация и стек URI; иное поведение при смежных одноимённых URI).
        // Шаг 7 — сортировка атрибутов (qualified по URI+local, затем unqualified): не делается.
        // Шаг 8 — объявления xmlns перед прочими атрибутами: не делается.
        // Шаг 9 — декодирование/нормализация текста и атрибутов, CDATA: не делается.
        // Сценарии из прил. Д МР 3.5.0.28 — отдельные тесты ниже.

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
        [Ignore(
            "СМЭВ шаг 7: qualified-атрибуты сортируются по namespace URI, затем по локальному имени; unqualified — после всех qualified."
        )]
        public void GetOutput_Step7_QualifiedAttributesSortedByUriThenLocal_UnqualifiedLast()
        {
            const string uA = "http://example.com/a";
            const string uB = "http://example.com/b";
            var input = Parse(
                $"<root xmlns=\"http://root\" xmlns:pb=\"{uB}\" xmlns:pa=\"{uA}\">"
                    + $"<el xmlns=\"http://root\" pb:z=\"2\" pa:m=\"1\" pa:a=\"0\" u=\"u\"/></root>"
            );
            var output = Transform(input);

            var el = output.DocumentElement?.FirstChild as XmlElement;
            Assert.IsNotNull(el);
            var dataAttrs = el
                .Attributes.Cast<XmlAttribute>()
                .Where(a => !IsXmlNsDeclaration(a))
                .ToList();

            var firstUnqualified = dataAttrs.FindIndex(a => a.NamespaceURI.Length == 0);
            var lastQualified = dataAttrs.FindLastIndex(a => a.NamespaceURI.Length > 0);
            if (firstUnqualified >= 0 && lastQualified >= 0)
            {
                Assert.IsTrue(
                    firstUnqualified > lastQualified,
                    "Непрефиксные атрибуты (unqualified) должны идти после всех qualified."
                );
            }

            var qualified = dataAttrs.Where(a => a.NamespaceURI.Length > 0).ToList();
            for (var i = 1; i < qualified.Count; i++)
            {
                var prev = qualified[i - 1];
                var cur = qualified[i];
                var cmpNs = string.CompareOrdinal(prev.NamespaceURI, cur.NamespaceURI);
                Assert.IsTrue(
                    cmpNs < 0
                        || (
                            cmpNs == 0
                            && string.CompareOrdinal(prev.LocalName, cur.LocalName) <= 0
                        ),
                    "Внутри qualified: сначала по URI, при равенстве — по локальному имени."
                );
            }
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

        /// <summary>
        /// Приложение Д.4 МР 3.5.0.28: удаление неиспользуемого xmlns:fnst с корневого элемента (упрощённый фрагмент).
        /// </summary>
        [TestMethod]
        [Ignore("Прил. Д.4 / шаг 4: неиспользуемое объявление xmlns:fnst не должно попадать в результат.")]
        public void GetOutput_AppendixD4_RemovesUnusedXmlnsFnstFromRoot()
        {
            const string uTns = "urn://x-artefacts-zags-pernamezp/4.0.0";
            const string uFnst = "urn://x-artefacts-zags-pernamezp/types/4.0.0";
            var input = Parse(
                $"<tns:PERNAMEZPRequest xmlns:tns=\"{uTns}\" xmlns:fnst=\"{uFnst}\"></tns:PERNAMEZPRequest>"
            );
            var output = Transform(input);

            var root = output.DocumentElement;
            Assert.IsNotNull(root);
            CollectionAssert.DoesNotContain(
                AttributeNames(root).ToList(),
                "xmlns:fnst",
                "Префикс fnst ни на элементе, ни на атрибутах не используется."
            );
        }

        /// <summary>
        /// Приложение Д.6 МР 3.5.0.28: порядок xmlns и qualified/unqualified атрибутов на PERNAMEZPRequest.
        /// </summary>
        [TestMethod]
        [Ignore("Прил. Д.6 / шаги 7–8: ожидаемый порядок атрибутов и объявлений пространств имён.")]
        public void GetOutput_AppendixD6_PERNAMEZPRequest_AttributeAndXmlnsOrder()
        {
            var input = Parse(
                "<tns:PERNAMEZPRequest xmlns:markcont=\"urn://x-artefacts-zags-pernamezp/markertypes/4.0.0\" "
                    + "xmlns:tns=\"urn://x-artefacts-zags-pernamezp/4.0.0\" "
                    + "xmlns:fnst=\"urn://x-artefacts-zags-pernamezp/types/4.0.0\" "
                    + "xmlns:frgu=\"urn://x-artefacts-zags-pernamezp/frgutypes/4.0.0\" "
                    + "markcont:ТипЗаявл=\"ФЛ\" ЗаявлДата=\"2019-08-13\" tns:ИдСвед=\"a\" "
                    + "markcont:Заявление=\"15843\" ДатаСвед=\"2018-08-13\" frgu:КодУслуги=\"3482943\">"
                    + "</tns:PERNAMEZPRequest>"
            );
            var output = Transform(input);

            var root = output.DocumentElement;
            Assert.IsNotNull(root);
            const string expectedStart =
                "<ns1:PERNAMEZPRequest xmlns:ns1=\"urn://x-artefacts-zags-pernamezp/4.0.0\" "
                    + "xmlns:ns2=\"urn://x-artefacts-zags-pernamezp/frgutypes/4.0.0\" "
                    + "xmlns:ns3=\"urn://x-artefacts-zags-pernamezp/markertypes/4.0.0\" "
                    + "ns1:ИдСвед=\"a\" ns2:КодУслуги=\"3482943\" ns3:Заявление=\"15843\" ns3:ТипЗаявл=\"ФЛ\" "
                    + "ДатаСвед=\"2018-08-13\" ЗаявлДата=\"2019-08-13\"";

            StringAssert.StartsWith(root.OuterXml, expectedStart);
            CollectionAssert.DoesNotContain(
                AttributeNames(root).ToList(),
                "xmlns:fnst",
                "Неиспользуемый fnst из исходника (см. прил. Д.4) не должен остаться."
            );
        }

        /// <summary>
        /// Приложение Д.7 МР 3.5.0.28 / шаг 9.1: декодирование текстового содержимого (&amp;gt; и т.д.).
        /// </summary>
        [TestMethod]
        [Ignore("Прил. Д.7 / шаг 9.1: эталонное содержимое после нормализации текста (см. МР, блок Фамилия).")]
        public void GetOutput_AppendixD7_Step9_FamilyNameText_Normalized()
        {
            const string u = "urn://x-artefacts-zags-pernamezp/types/4.0.0";
            var input = Parse($"<fnst:Фамилия xmlns:fnst=\"{u}\">&gt;&gt;Иванов</fnst:Фамилия>");
            var output = Transform(input);

            var el = output.DocumentElement;
            Assert.IsNotNull(el);
            Assert.AreEqual("Фамилия", el.LocalName);
            Assert.AreEqual(">>Иванов", el.InnerText);
        }

        /// <summary>
        /// Приложение Д.7 МР 3.5.0.28 / шаг 9.2: нормализация и декодирование значения qualified-атрибута.
        /// </summary>
        [TestMethod]
        [Ignore("Прил. Д.7 / шаг 9.2: значение ns1:НомерЗапис после трансформации (см. МР, СведРегПерИмя).")]
        public void GetOutput_AppendixD7_Step9_QualifiedAttributeValue_Normalized()
        {
            const string uTns = "urn://x-artefacts-zags-pernamezp/4.0.0";
            var input = Parse(
                "<tns:СведРегПерИмя xmlns:tns=\""
                    + uTns
                    + "\" ДатаЗапис=\"2018-08-13\" tns:НомерЗапис=\"&gt;&gt;&#xA;2&gt;&#xA;2&lt;&gt; 1&amp;8&gt;&gt;5&apos;0&#xA;&quot; a\">"
                    + "</tns:СведРегПерИмя>"
            );
            var output = Transform(input);

            var el = output.DocumentElement;
            Assert.IsNotNull(el);
            XmlAttribute num = null;
            foreach (XmlAttribute a in el.Attributes)
            {
                if (a.LocalName == "НомерЗапис" && a.NamespaceURI == uTns)
                {
                    num = a;
                    break;
                }
            }

            Assert.IsNotNull(num);
            Assert.AreEqual(">> 2> 2<> 1&8>>5'0 \" a", num.Value);
        }

        /// <summary>
        /// Приложение Д.3 МР 3.5.0.28 / шаг 3: самозакрывающийся тег со сложным локальным именем → пара тегов.
        /// </summary>
        [TestMethod]
        [Ignore("Прил. Д.3 / шаг 3: образец СтатусЗаписи — пустой элемент в виде start+end.")]
        public void GetOutput_AppendixD3_StatusZapisi_EmptyElementBecomesPair()
        {
            const string u = "urn://example/zags";
            var input = Parse(
                $"<tns:СтатусЗаписи xmlns:tns=\"{u}\" ДатаНачСтатус=\"1957-08-13\" КодСтатус=\"03\" НаимСтатус=\"a\"/>"
            );
            var output = Transform(input);

            var el = output.DocumentElement;
            Assert.IsNotNull(el);
            Assert.AreEqual("СтатусЗаписи", el.LocalName);
            Assert.IsFalse(el.IsEmpty, "По прил. Д.3 ожидается не самозакрывающаяся форма.");
            Assert.AreEqual(0, el.ChildNodes.Count);
        }

        private static bool IsXmlNsDeclaration(XmlAttribute a)
        {
            return a.NamespaceURI == "http://www.w3.org/2000/xmlns/"
                || a.Prefix == "xmlns"
                || (a.Prefix.Length == 0 && a.LocalName == "xmlns");
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
