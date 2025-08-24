using System.Security.Cryptography;
using System.Text;

namespace AvaluxUI.Utils;

internal class EncryptedSettingsSection : SettingsSection
{
    private readonly Aes _aes = Aes.Create();

    public EncryptedSettingsSection(SettingsSection? parent, string? secretKey, string? name = null,
        Dictionary<string, string?>? dictionary = null,
        Dictionary<string, SettingsSection>? sections = null) : base(parent, name, dictionary, sections,
        BCrypt.Net.BCrypt.HashPassword(secretKey))
    {
        ArgumentNullException.ThrowIfNull(secretKey, nameof(secretKey));
        // Генерация ключа и IV (Initialization Vector) на основе пароля
        _aes.Key = GenerateKey(secretKey, _aes.KeySize / 8);
        _aes.IV = GenerateIv(secretKey, _aes.BlockSize / 8);
    }

    protected override string? Encrypt(string? plainText)
    {
        if (plainText == null)
            return null;
        // Создание шифратора
        var encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV);

        // Шифрование
        byte[] encryptedBytes;
        using (var msEncrypt = new MemoryStream())
        {
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                encryptedBytes = msEncrypt.ToArray();
            }
        }

        // Возврат зашифрованных данных в виде Base64 строки
        return Convert.ToBase64String(encryptedBytes);
    }

    protected override string? Decrypt(string? cipherText)
    {
        if (cipherText == null)
            return null;
        var decryptor = _aes.CreateDecryptor(_aes.Key, _aes.IV);

        // Расшифровка
        var cipherBytes = Convert.FromBase64String(cipherText);

        using var msDecrypt = new MemoryStream(cipherBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        var plainText = srDecrypt.ReadToEnd();


        return plainText;
    }

    private static byte[] GenerateKey(string password, int keySize)
    {
        // Используем SHA256 для генерации ключа из пароля
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return key.Take(keySize).ToArray(); // Обрезаем до нужного размера
    }

    private static byte[] GenerateIv(string password, int ivSize)
    {
        // Используем MD5 для генерации IV из пароля
        var iv = MD5.HashData(Encoding.UTF8.GetBytes(password));
        return iv.Take(ivSize).ToArray(); // Обрезаем до нужного размера
    }
}