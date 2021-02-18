namespace CAPILite
{
    public static class CApiLiteConsts
    {
        public const string CP_GR3410_2012_PROV = "Crypto-Pro GOST R 34.10-2012 Cryptographic Service Provider";

        public const uint PROV_GOST_2012_256 = 80;

        public const uint CRYPT_MACHINE_KEYSET = 0x00000020;
        public const uint CRYPT_VERIFYCONTEXT = 0xF0000000;

        public const uint X509_ASN_ENCODING = 0x00000001;
        public const uint PKCS_7_ASN_ENCODING = 0x00010000;
        public const uint PKCS_7_OR_X509_ASN_ENCODING = PKCS_7_ASN_ENCODING | X509_ASN_ENCODING;

        public const uint CERT_COMPARE_SHIFT = 16;
        public const uint CERT_COMPARE_SHA1_HASH = 1;
        public const uint CERT_FIND_SHA1_HASH = ((int)CERT_COMPARE_SHA1_HASH << (int)CERT_COMPARE_SHIFT);

        public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        public const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        public const uint FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;

        public const uint PKCS12_IMPORT_SILENT = 0x00000040;

        public const uint CRYPT_ACQUIRE_SILENT_FLAG = 0x00000040;
        public const uint CRYPT_ACQUIRE_USE_PROV_INFO_FLAG = 0x00000002;

        public const uint CALG_GR3411_2012_256 = 0x8021;

        public const uint HP_HASHVAL = 0x0002;
        public const uint HP_HASHSIZE = 0x0004;
    }
}
