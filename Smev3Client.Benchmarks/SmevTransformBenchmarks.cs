using System.Text;
using System.Xml;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Smev3Client.Crypt;

namespace Smev3Client.Benchmarks
{
    [Config(typeof(Config))]
    [MemoryDiagnoser]
    public class SmevTransformBenchmarks
    {
        public enum XmlScenario
        {
            Simple,
            NamespaceHeavy
        }

        private XmlDocument _document = new XmlDocument { PreserveWhitespace = true };

        [Params(10, 1_000)]
        public int RepeatCount { get; set; }

        [Params(XmlScenario.Simple, XmlScenario.NamespaceHeavy)]
        public XmlScenario Scenario { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _document = new XmlDocument { PreserveWhitespace = true };
            _document.LoadXml(BuildXml(repeatCount: RepeatCount, scenario: Scenario));
        }

        [Benchmark(Baseline = true)]
        public XmlDocument Transform_Legacy()
        {
            var transform = new LegacyXmlDsigSmevTransform();
            transform.LoadInput(_document);
            return (XmlDocument)transform.GetOutput();
        }

        [Benchmark]
        public XmlDocument Transform_Optimized()
        {
            var transform = new XmlDsigSmevTransform();
            transform.LoadInput(_document);
            return (XmlDocument)transform.GetOutput();
        }

        [Benchmark]
        public XmlDocument Transform_GlobalMap()
        {
            var transform = new GlobalMapXmlDsigSmevTransform();
            transform.LoadInput(_document);
            return (XmlDocument)transform.GetOutput();
        }

        private static string BuildXml(int repeatCount, XmlScenario scenario)
        {
            if (scenario == XmlScenario.NamespaceHeavy)
            {
                return BuildNamespaceHeavyXml(repeatCount);
            }

            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.Append("<root xmlns=\"urn:test:default\">");

            for (var i = 0; i < repeatCount; i++)
            {
                sb.Append("<item id=\"");
                sb.Append(i);
                sb.Append("\"><value code=\"X\">text</value><value>plain</value></item>");
            }

            sb.Append("</root>");
            return sb.ToString();
        }

        private static string BuildNamespaceHeavyXml(int repeatCount)
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.Append("<root xmlns=\"urn:test:root\">");

            for (var i = 0; i < repeatCount; i++)
            {
                sb.Append("<item xmlns=\"urn:test:ns:");
                sb.Append(i);
                sb.Append("\" id=\"");
                sb.Append(i);
                sb.Append("\"><value xmlns=\"urn:test:value:");
                sb.Append(i);
                sb.Append("\" code=\"X\">text</value></item>");
            }

            sb.Append("</root>");
            return sb.ToString();
        }

        private sealed class Config : ManualConfig
        {
            public Config()
            {
                AddJob(Job.Default);
                AddColumn(TargetMethodColumn.Method, StatisticColumn.Mean, BaselineRatioColumn.RatioMean);
            }
        }
    }
}
