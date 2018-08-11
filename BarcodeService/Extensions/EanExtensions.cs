using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeService.Extensions
{
    public static class EanExtensions
    {
        private static int GetEanChecksum(string data, int length)
        {
            int sum = 0;

            for (int i = length; i >= 0; i--)
            {
                int digit = data[i] - 0x30;
                if ((i & 0x01) == 1)
                    sum += digit;
                else
                    sum += digit * 3;
            }
            int mod = sum % 10;
            return mod == 0 ? 0 : 10 - mod;
        }

        private static int GetEan8Checksum(string data)
        {
            return GetEanChecksum(data, 6);
        }

        private static int GetEan13Checksum(string data)
        {
            return GetEanChecksum(data, 11);
        }

        public static bool IsEan(this string input)
        {
            int dummy = 0;
            if (int.TryParse(input, out dummy))
            {
                var checkDigit = input[input.Length - 1];

                //ean8
                if (input.Length == 8 && checkDigit == GetEan8Checksum(input))
                {
                    return true;
                }

                if (input.Length == 13 && checkDigit == GetEan8Checksum(input))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsEan8(this string input)
        {
            return input.IsEan();
        }

        public static bool IsEan13(this string input)
        {
            return input.IsEan();
        }

    }
}
