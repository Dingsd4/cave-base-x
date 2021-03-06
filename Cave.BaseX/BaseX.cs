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
using System.IO;
using System.Text;

namespace Cave
{
    /// <summary>
    /// Provides abstract functions for all fixed base (32, 64, ...) conversions
    /// </summary>
    public abstract class BaseX
    {
        /// <summary>Gets the bits per character.</summary>
        /// <value>The bits per character.</value>
        public int BitsPerCharacter { get; }

        /// <summary>Gets the character dictionary.</summary>
        /// <value>The character dictionary.</value>
        public CharacterDictionary CharacterDictionary { get; }

        /// <summary>Initializes a new instance of the <see cref="BaseX" /> class.</summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="bitCount">The bit count.</param>
        /// <exception cref="Exception">Invalid dictionary / bits</exception>
        public BaseX(CharacterDictionary dictionary, int bitCount)
        {
            if (bitCount < 1 || bitCount > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(BitsPerCharacter));
            }

            CharacterDictionary = dictionary;
            BitsPerCharacter = bitCount;
            if (dictionary.Length != 1 << bitCount)
            {
                throw new Exception("Invalid dictionary / bits");
            }
        }

        #region decoder interface        
        /// <summary>Decodes the u int8.</summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public byte DecodeUInt8(string value)
        {
            return unchecked((byte)DecodeUInt32(value));
        }

        /// <summary>Decodes the int8.</summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public sbyte DecodeInt8(string value)
        {
            return unchecked((sbyte)DecodeUInt32(value));
        }

        /// <summary>Decodes the u int16.</summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public ushort DecodeUInt16(string value)
        {
            return unchecked((ushort)DecodeUInt32(value));
        }

        /// <summary>Decodes the int16.</summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public short DecodeInt16(string value)
        {
            return unchecked((short)DecodeUInt32(value));
        }

        /// <summary>Decodes the u int32.</summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public uint DecodeUInt32(string value)
        {
            uint result = 0;
            int bitPosition = 0;
            foreach (char character in value)
            {
                uint bits = (uint)DecodeCharacter(character);
                bits <<= bitPosition;
                result |= bits;
                bitPosition += BitsPerCharacter;
            }
            return result;
        }

        /// <summary>Decodes the int32.</summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public int DecodeInt32(string value)
        {
            return unchecked((int)DecodeUInt32(value));
        }

        /// <summary>Decodes the u int64.</summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public ulong DecodeUInt64(string value)
        {
            ulong result = 0;
            int bitPosition = 0;
            foreach(char character in value)
            {
                ulong bits = (ulong)DecodeCharacter(character);
                bits <<= bitPosition;
                result |= bits;
                bitPosition += BitsPerCharacter;
            }
            return result;
        }

        /// <summary>Decodes the int64.</summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public long DecodeInt64(string value)
        {
            return unchecked((long)DecodeUInt64(value));
        }

        /// <summary>
        /// Decodes a data string and converts the result to utf8
        /// </summary>
        /// <param name="value">The data to decode</param>
        public string DecodeUtf8(string value)
        {
            return Encoding.UTF8.GetString(Decode(value));
        }

        /// <summary>
        /// Decodes data and converts the result to utf8
        /// </summary>
        /// <param name="value">The data to decode</param>
        public string DecodeUtf8(byte[] value)
        {
            return Encoding.UTF8.GetString(Decode(value));
        }

        /// <summary>
        /// Decodes a data string 
        /// </summary>
        /// <param name="value">The data to decode</param>
        public byte[] Decode(string value)
        {
            return Decode(Encoding.ASCII.GetBytes(value));
        }

        /// <summary>Decodes the character.</summary>
        /// <param name="character">The character.</param>
        /// <returns>Returns the value for the specified character</returns>
        public int DecodeCharacter(char character)
        {
            return CharacterDictionary.GetValue(character);
        }

        /// <summary>Decodes the specified data.</summary>
        /// <param name="data">The data.</param>
        /// <returns>Returns the decoded data</returns>
        public abstract byte[] Decode(byte[] data);
        #endregion

        #region encoder interface
        
        /// <summary>Encodes the specified value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns the BaseX encoded string</returns>
        public string Encode(byte value)
        {
            return unchecked(Encode((uint)value));
        }

        /// <summary>Encodes the specified value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns the BaseX encoded string</returns>
        public string Encode(sbyte value)
        {
            return unchecked(Encode((uint)value));
        }

        /// <summary>Encodes the specified value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns the BaseX encoded string</returns>
        public string Encode(ushort value)
        {
            return unchecked(Encode((uint)value));
        }

        /// <summary>Encodes the specified value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns the BaseX encoded string</returns>
        public string Encode(short value)
        {
            return unchecked(Encode((uint)value));
        }

        /// <summary>Encodes the specified value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns the BaseX encoded string</returns>
        public string Encode(int value)
        {
            return unchecked(Encode((uint)value));
        }

        /// <summary>Encodes the specified value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns the BaseX encoded string</returns>
        public string Encode(uint value)
        {
            StringBuilder result = new StringBuilder();
            int bits = BitsPerCharacter;
            uint mask = 0xFFFFFFFF >> (32 - bits);
            while (value > 0)
            {
                uint character = value & mask;
                result.Append(EncodeCharacter((char)character));
                value >>= bits;
            }
            return result.ToString();
        }

        /// <summary>Encodes the specified value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns the BaseX encoded string</returns>
        public string Encode(long value)
        {
            return unchecked(Encode((ulong)value));
        }

        /// <summary>Encodes the specified value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns the BaseX encoded string</returns>
        public string Encode(ulong value)
        {
            StringBuilder result = new StringBuilder();
            int bits = BitsPerCharacter;
            ulong mask = 0xFFFFFFFF >> (32 - bits);
            while (value > 0)
            {
                ulong character = value & mask;
                result.Append(EncodeCharacter((char)character));
                value >>= bits;
            }
            return result.ToString();
        }

        /// <summary>
        /// Encodes the specified string
        /// </summary>
        /// <param name="data">The data to encode</param>
        /// <returns>Returns the BaseX encoded string</returns>
        public string Encode(string data)
        {
            return Encode(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>Encodes the specified value in range 0..X.</summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns the BaseX encoded character</returns>
        public char EncodeCharacter(int value)
        {
            return CharacterDictionary.GetCharacter(value);
        }

        /// <summary>Encodes the specified data.</summary>
        /// <param name="data">The data.</param>
        /// <returns>Returns the BaseX encoded string</returns>
        public abstract string Encode(byte[] data);
        #endregion
    }
}