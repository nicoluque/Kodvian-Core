namespace Kodvian.Core.Application.Common.Security;

public interface ITokenEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
