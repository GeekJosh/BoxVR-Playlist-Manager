using System;
using System.Text;
using System.Security.Cryptography;

namespace BoxVRPlaylistManagerNETCore.FitXr.Utility
{
    public sealed class MD5
    {
        public static string MD5Sum(string strToEncrypt) => MD5.Md5Sum(strToEncrypt);

        public static string Md5Sum(string strToEncrypt)
        {
            byte[] hash = new MD5CryptoServiceProvider().ComputeHash(new UTF8Encoding().GetBytes(strToEncrypt));
            string str = "";
            for(int index = 0; index < hash.Length; ++index)
                str += Convert.ToString(hash[index], 16).PadLeft(2, '0');
            return str.PadLeft(32, '0');
        }
    }
}
