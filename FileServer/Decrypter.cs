using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WebGallery.FileServer
{
    public static class Decrypter
    {
        public static async Task<MemoryStream> Decrypt(string filePath, string certName, string password = "")
        {
            //Read the file in memory so we can overwrite the source with it's original file.
            //We also need it in memory so we can extract the key.
            var file = (await File.ReadAllBytesAsync(filePath)).ToList();
            var encryptedKey = new Collection<byte>();
            //Checking the length so we know how much bytes we need to take from the file.
            //Different certificates can create different size of keys.
            var encryptLength = Encrypter.EncryptKey(Encoding.UTF8.GetBytes("string"), certName, password).Length;
            //Extract the key.
            for (var i = 0; i < encryptLength; i++)
                encryptedKey.Add(file[i]);
            file.RemoveRange(0, encryptLength);

            var decryptedKey = DecryptKey(encryptedKey.ToArray(), certName, password);

            using (var managed = new AesManaged())
            {
                //We're using AES encryption, but this time we do not generate the key but pass our decrypted key.
                Aes aesKey = Aes.Create();
                aesKey.Key = decryptedKey;
                byte[] ivKey = new byte[aesKey.IV.Length];
                Array.Copy(aesKey.Key, ivKey, aesKey.IV.Length);
                aesKey.IV = ivKey;
                var decryptor = aesKey.CreateDecryptor();

                var outputStream = new MemoryStream();
                using (var decryptStream = new CryptoStream(outputStream, decryptor, CryptoStreamMode.Write))
                using (var encryptedFileStream = new MemoryStream(file.ToArray()))
                    await encryptedFileStream.CopyToAsync(decryptStream);

                return outputStream;
            }
        }

        public static async Task<FileStream> DecryptToFileStream(string filePath, string certName, string password = "")
        {
            var encryptLength = Encrypter.EncryptKey(Encoding.UTF8.GetBytes("string"), certName, password).Length;

            await using var inputStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            var encryptedKey = new byte[encryptLength];
            var bytesRead = 0;
            while (bytesRead < encryptLength)
            {
                var read = await inputStream.ReadAsync(encryptedKey.AsMemory(bytesRead, encryptLength - bytesRead));
                if (read == 0)
                    throw new EndOfStreamException("Unexpected end of encrypted file while reading the key header.");

                bytesRead += read;
            }

            var decryptedKey = DecryptKey(encryptedKey, certName, password);

            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var outputStream = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.Read,
                81920,
                FileOptions.Asynchronous | FileOptions.DeleteOnClose);

            using var aesKey = Aes.Create();
            aesKey.Key = decryptedKey;
            byte[] ivKey = new byte[aesKey.IV.Length];
            Array.Copy(aesKey.Key, ivKey, aesKey.IV.Length);
            aesKey.IV = ivKey;
            var decryptor = aesKey.CreateDecryptor();

            using (var decryptStream = new CryptoStream(outputStream, decryptor, CryptoStreamMode.Write, leaveOpen: true))
                await inputStream.CopyToAsync(decryptStream);

            await outputStream.FlushAsync();
            outputStream.Position = 0;

            return outputStream;
        }

        private static byte[] DecryptKey(byte[] keyBytes, string certName, string password)
        {
            var cert = new X509Certificate2(certName);
            var privateKey = cert.GetRSAPrivateKey();
            //Decrypt the key with the same padding used to encrypt it.
            return privateKey.Decrypt(keyBytes, RSAEncryptionPadding.OaepSHA512);
        }
    }
}