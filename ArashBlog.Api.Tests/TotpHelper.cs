using System.Collections.Generic;
using System.Security.Cryptography;

namespace ArashBlog.Api.Tests;

// Generates a valid RFC 6238 TOTP (SHA1, 30s step, 6 digits — the same
// parameters ASP.NET Core Identity's default authenticator provider uses)
// for the base32 "manual key" returned by /api/auth/otp/setup. No external
// device needed, same purpose as the totp() helper in the Django project's
// "Local test login" doc.
public static class TotpHelper
{
    public static string Generate(string formattedKey)
    {
        var key = Base32Decode(formattedKey.Replace(" ", ""));
        var timestep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        var timestepBytes = BitConverter.GetBytes(timestep);
        if (BitConverter.IsLittleEndian) Array.Reverse(timestepBytes);

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(timestepBytes);
        var offset = hash[^1] & 0x0F;
        var binaryCode = ((hash[offset] & 0x7F) << 24)
                          | ((hash[offset + 1] & 0xFF) << 16)
                          | ((hash[offset + 2] & 0xFF) << 8)
                          | (hash[offset + 3] & 0xFF);
        var code = binaryCode % 1_000_000;
        return code.ToString("D6");
    }

    private static byte[] Base32Decode(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        input = input.TrimEnd('=').ToUpperInvariant();

        var bits = new List<bool>();
        foreach (var c in input)
        {
            var value = alphabet.IndexOf(c);
            if (value < 0) continue;
            for (var i = 4; i >= 0; i--) bits.Add(((value >> i) & 1) == 1);
        }

        var bytes = new List<byte>();
        for (var i = 0; i + 8 <= bits.Count; i += 8)
        {
            byte b = 0;
            for (var j = 0; j < 8; j++) b = (byte)((b << 1) | (bits[i + j] ? 1 : 0));
            bytes.Add(b);
        }

        return bytes.ToArray();
    }
}
