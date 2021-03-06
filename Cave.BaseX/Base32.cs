﻿#region CopyRight 2018
/*
    Copyright (c) 2003-2018 Andreas Rohleder (andreas@rohleder.cc)
    All rights reserved
*/
#endregion
#region License LGPL-3
/*
    This program/library/sourcecode is free software; you can redistribute it
    and/or modify it under the terms of the GNU Lesser General Public License
    version 3 as published by the Free Software Foundation subsequent called
    the License.

    You may not use this program/library/sourcecode except in compliance
    with the License. The License is included in the LICENSE file
    found at the installation directory or the distribution package.

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the
    "Software"), to deal in the Software without restriction, including
    without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to
    permit persons to whom the Software is furnished to do so, subject to
    the following conditions:

    The above copyright notice and this permission notice shall be included
    in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
#region Authors & Contributors
/*
   Author:
     Andreas Rohleder <andreas@rohleder.cc>

   Contributors:
     Gernot Lenkner <g.lenkner@cavemail.org>
 */
#endregion

using System;
using System.Collections.Generic;

namespace Cave
{
    /// <summary>
    /// Provides Base32 en-/decoding
    /// </summary>
    public class Base32 : BaseX
    {
        #region public static default instances
        /// <summary>
        /// Provides the otp charset for Base32 en-/decoding (no padding)
        /// </summary>
        public static Base32 OTP
        {
            get
            {
                return new Base32(new CharacterDictionary("abcdefghijklmnopqrstuvwxyz234567"), null);
            }
        }

        /// <summary>
        /// Provides the default (uppercase) charset for base32 en-/decoding with padding
        /// </summary>
        public static Base32 Default
        {
            get
            {
                return new Base32(new CharacterDictionary("0123456789ABCDEFGHIJKLMNOPQRSTUV"), '=');
            }
        }

        /// <summary>
        /// Provides the default (uppercase) charset for Base32 en-/decoding without padding
        /// </summary>
        public static Base32 NoPadding
        {
            get
            {
                return new Base32(new CharacterDictionary("0123456789ABCDEFGHIJKLMNOPQRSTUV"), null);
            }
        }

        /// <summary>
        /// Provides the url safe dictatable (no i,l,v,0) charset for Base32 en-/decoding (no padding)
        /// </summary>
        public static Base32 Safe
        {
            get
            {
                return new Base32(new CharacterDictionary("abcdefghjkmnopqrstuwxyz123456789"), null);
            }
        }

        #endregion

        const int BitCount = 5;
        char? m_Padding;

        /// <summary>Initializes a new instance of the <see cref="Base32"/> class.</summary>
        /// <param name="dictionary">The dictionary containing 64 ascii characters used for encoding.</param>
        /// <param name="padding">The padding (use null to skip padding).</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public Base32(CharacterDictionary dictionary, char? padding) : base(dictionary, BitCount)
        {
            m_Padding = padding;
            if (m_Padding != null)
            {
                int paddingChar = (char)m_Padding;
                if ((paddingChar < 0) || (paddingChar > 127))
                {
                    throw new ArgumentException(string.Format("Invalid padding character!"), nameof(m_Padding));
                }
            }
        }

        #region public decoder interface

        /// <summary>
        /// Decodes a Base32 data array
        /// </summary>
        /// <param name="data">The Base32 data to decode</param>
        public override byte[] Decode(byte[] data)
        {
            if (CharacterDictionary == null)
            {
                throw new ArgumentNullException(nameof(CharacterDictionary));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (m_Padding != null)
            {
                int paddingChar = (char)m_Padding;
                if ((paddingChar < 0) || (paddingChar > 127))
                {
                    throw new ArgumentException(string.Format("Invalid padding character!"), nameof(m_Padding));
                }
            }
            //decode data
            List<byte> result = new List<byte>(data.Length);
            int value = 0;
            int bits = 0;
            foreach (byte b in data)
            {
                if (b == m_Padding)
                {
                    break;
                }

                value <<= BitCount;
                bits += BitCount;
                value |= CharacterDictionary.GetValue((char)b);
                if (bits >= 8)
                {
                    bits -= 8;
                    int outValue = value >> bits;
                    value = value & ~(0xFFFF << bits);
                    result.Add((byte)outValue);
                }
            }
            return result.ToArray();
        }
        #endregion

        #region public encoder interface        

        /// <summary>
        /// Encodes the specified data
        /// </summary>
        /// <param name="data">The data to encode</param>
        public override string Encode(byte[] data)
        {
            List<char> result = new List<char>(data.Length * 2);
            int value = 0;
            int bits = 0;
            foreach (byte b in data)
            {
                value = (value << 8) | b;
                bits += 8;
                while (bits >= BitCount)
                {
                    bits -= BitCount;
                    int outValue = value >> bits;
                    value = value & ~(0xFFFF << bits);
                    result.Add(CharacterDictionary.GetCharacter(outValue));
                }
            }
            if (bits >= BitCount)
            {
                bits -= BitCount;
                int outValue = value >> bits;
                value = value & ~(0xFFFF << bits);
                result.Add(CharacterDictionary.GetCharacter(outValue));
            }
            if (bits > 0)
            {
                int shift = BitCount - bits;
                int outValue = value << shift;
                result.Add(CharacterDictionary.GetCharacter(outValue));
                bits -= BitCount;
            }
            if (m_Padding != null)
            {
                char padding = (char)m_Padding;
                while (bits % 8 != 0)
                {
                    result.Add(padding);
                    bits -= BitCount;
                }
            }
            return new string(result.ToArray());
        }
        #endregion
    }
}
