using KachelService.Database;
using KachelService.Model;
using KachelService.Model.Customer;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Web;

namespace KachelService.Services
{
    public class LinkService
    {
        public async Task<string> GetUri(Customer? customer, int version)
        {
            if (customer != null)
            {

                string requestUrl = string.Empty;
                string url = string.Empty;

                switch (customer.Origin)
                {
                    case "PV":
                        {
                            url = Configuration.UrlPv;
                            string kdnr = "1";
                            kdnr += customer.DeliveryAddress.ToString("D3");

                            var nullen = 8 - customer.CustomerNumber.Length;
                            if (nullen > 0)
                            {
                                for (int i = 0; i < nullen; i++)
                                {
                                    kdnr += "0";
                                }
                            }

                            kdnr += customer.CustomerNumber;

                            if (long.TryParse(kdnr, out var knr))
                            {
                                requestUrl =
                                    $"&gruppeID={knr}" +
                                    $"&userID={customer.CustomerNumber}" +
                                    $"&vorname={customer.User?.FirstName ?? ""}" +
                                    $"&nachname={customer.User?.LastName ?? ""}" +
                                    $"&rolle=ANMELDUNG" +
                                    $"&gueltig={DateTime.Now:yyyyMMdd}" +
                                    $"&verId={version}";
                            }
                        }
                        break;
                    case "STG":
                        {
                            url = Configuration.UrlStg;
                            string custNumber = string.Empty;
                            if (int.TryParse(customer.CustomerNumber, out var cnr))
                            {
                                custNumber = "1" + cnr.ToString("D6");
                            }

                            if (long.TryParse(custNumber, out var knr))
                            {
                                requestUrl =
                                    $"&gruppeID={knr}" +
                                    $"&userID={knr}" +
                                    $"&vorname={customer.User?.FirstName ?? ""}" +
                                    $"&nachname={customer.User?.LastName ?? ""}" +
                                    $"&rolle=ANMELDUNG" +
                                    $"&gueltig={DateTime.Now:yyyyMMdd}" +
                                    $"&verId={version}";
                            }
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(requestUrl))
                {

                    await using var db = new Pv100JahreContext();

                    string cryptString = String.Empty;

                    if (!Configuration.UseAES)
                    {
                        // Use sqlserver crypt.
                        var cryptResult = await db.Procedures.CrypeUrlAsync(requestUrl);
                        if (cryptResult != null && cryptResult.Any())
                        {
                            var cr = cryptResult.First();
                            cryptString = cr.text;
                        }
                    }
                    else
                    {
                        var aes = new Aes();
                        cryptString = aes.EncryptToBase64(requestUrl);
                    }

                    if (!string.IsNullOrEmpty(cryptString))
                    {
                        var cr = cryptString;
                        var entry = new LinkEntry()
                        {
                            Query = requestUrl,

                            Encrypted = cr,
                            BaseUrl = url
                        };

                        db.Links.Add(new Links()
                        {
                            Created = DateTime.Now,
                            Data = JsonSerializer.Serialize(entry,
                                new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })
                        });

                        await db.SaveChangesAsync();

                        return $"{url}{cr}";
                    }
                }
            }
            return string.Empty;
        }

        public async Task<string> GetUriEssen(Customer? customer, int version)
        {
            if (customer != null)
            {

                string requestUrl = string.Empty;
                string url = string.Empty;

                switch (customer.Origin)
                {
                    case "PV":
                        {
                            url = Configuration.UrlPvLive;
                            string kdnr = "1";
                            kdnr += customer.DeliveryAddress.ToString("D3");

                            var nullen = 8 - customer.CustomerNumber.Length;
                            if (nullen > 0)
                            {
                                for (int i = 0; i < nullen; i++)
                                {
                                    kdnr += "0";
                                }
                            }

                            kdnr += customer.CustomerNumber;

                            if (long.TryParse(kdnr, out var knr))
                            {
                                requestUrl =
                                    $"&gruppeID={knr}" +
                                    $"&userID={knr}" +
                                    $"&vorname={customer.User?.FirstName ?? ""}" +
                                    $"&nachname={customer.User?.LastName ?? ""}" +
                                    $"&rolle=ANMELDUNG" +
                                    $"&gueltig={DateTime.Now:yyyyMMdd}" +
                                    $"&verId={version}";
                            }
                        }
                        break;

                }

                if (!string.IsNullOrEmpty(requestUrl))
                {
                    try
                    {
                        await using var db = new Pv100JahreContext();

                        string cryptString = String.Empty;

                        if (!Configuration.UseAES)
                        {
                            // Use sqlserver crypt.
                            var cryptResult = await db.Procedures.CrypeUrlAsync(requestUrl);
                            var myTest = TestEnc(requestUrl);

                            if (cryptResult != null && cryptResult.Any())
                            {
                                var cr = cryptResult.First();
                                cryptString = cr.text;
                            }
                        }
                        else
                        {
                            var aes = new Aes();
                            cryptString = aes.EncryptToBase64(requestUrl);
                        }

                        if (!string.IsNullOrEmpty(cryptString))
                        {
                            var cr = cryptString;
                            var entry = new LinkEntry()
                            {
                                Query = requestUrl,

                                Encrypted = cr,
                                BaseUrl = url
                            };

                            db.Links.Add(new Links()
                            {
                                Created = DateTime.Now,
                                Data = JsonSerializer.Serialize(entry,
                                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })
                            });

                            await db.SaveChangesAsync();

                            return $"{url}{cr}";
                        }

                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            return string.Empty;
        }

        private string TestEnc(string plaintext)
        {

            string passphrase = "KM#43deV!DnGB$YXrSJt";

            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            var key = new Rfc2898DeriveBytes(passphrase, salt, 1000);
            byte[] keyBytes = key.GetBytes(32);
            byte[] ivBytes = key.GetBytes(16);

            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] encrypted;

                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                    cs.Write(plaintextBytes, 0, plaintextBytes.Length);
                    cs.FlushFinalBlock();
                    encrypted = ms.ToArray();
                }

                string base64Encoded = Convert.ToBase64String(encrypted);
                string urlEncoded = HttpUtility.UrlEncode(base64Encoded);

                return urlEncoded;
            }
        }
    }

}
