using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace Smev3Client.Test
{
    class XmlDsigSmevTransform : Transform
    {
        private static Type[] _inputTypes = new[] { typeof(XmlDocument) };
        private static Type[] _outputTypes = new[] { typeof(XmlDocument) };

        public XmlDsigSmevTransform()
        {
            Algorithm = "urn://smev-gov-ru/xmldsig/transform";
        }

        public override Type[] InputTypes => _inputTypes;

        public override Type[] OutputTypes => _outputTypes;

        public override object GetOutput()
        {
            return _obj;
        }

        public override object GetOutput(Type type)
        {
            throw new NotImplementedException();
        }

        public override void LoadInnerXml(XmlNodeList nodeList)
        {
            throw new NotImplementedException();
        }

        private object _obj;

        public override void LoadInput(object obj)
        {
            _obj = obj;
        }

        protected override XmlNodeList GetInnerXml()
        {
            return null;
        }
    }
}
