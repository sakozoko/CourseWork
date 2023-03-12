using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CourseWork.Services;

public class RsaEncryptHandler : IEncryptHandler
{
    private readonly RSACryptoServiceProvider _rsa;

    public RsaEncryptHandler()
    {
        _rsa = new RSACryptoServiceProvider(2048);
        //export parameters to file public.private.xml
        var parameters = _rsa.ExportParameters(true);
        var xml = _rsa.ToXmlString(true);
        File.WriteAllText("public.private.xml", xml);
        
    }

    public string Encrypt(string value)
    {
        var encryptedBytes = _rsa.Encrypt(Encoding.UTF8.GetBytes(value), false);
        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string value)
    {
        var decryptedBytes = _rsa.Decrypt(Convert.FromBase64String(value), false);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}