// Copyright (c) Microsoft. All rights reserved.

using System.Security.Cryptography;
using System.Text;

namespace Microsoft.SemanticKernel.Connectors.HunYuan.Core;

/// <summary>
/// Signature helper class.
/// </summary>
internal class SignHelper
{
    public static string SHA256Hex(string s)
    {
        return SHA256Hex(Encoding.UTF8.GetBytes(s));
    }

    public static string SHA256Hex(byte[] s)
    {
        using (SHA256 algo = SHA256.Create())
        {
            byte[] hashbytes = algo.ComputeHash(s);
            StringBuilder builder = new();
            for (int i = 0; i < hashbytes.Length; ++i)
            {
                builder.Append(hashbytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }

    public static byte[] HmacSHA256(byte[] key, byte[] msg)
    {
        using (HMACSHA256 mac = new HMACSHA256(key))
        {
            return mac.ComputeHash(msg);
        }
    }
}
