using System.Buffers;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace QRCoder;

/// <summary>
/// Provides functionality to generate QR code data that can be used to create QR code images.
/// </summary>
public partial class QRCodeGenerator : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QRCodeGenerator"/> class.
    /// Initializes the QR code generator.
    /// </summary>
    public QRCodeGenerator()
    {
    }

    /// <summary>
    /// Calculates the QR code data which than can be used in one of the rendering classes to generate a graphical representation.
    /// </summary>
    /// <param name="plainText">The payload which shall be encoded in the QR code.</param>
    /// <param name="eccLevel">The level of error correction data.</param>
    /// <param name="forceUtf8">Shall the generator be forced to work in UTF-8 mode?.</param>
    /// <param name="utf8BOM">Should the byte-order-mark be used?.</param>
    /// <param name="eciMode">Which ECI mode shall be used?.</param>
    /// <param name="requestedVersion">Set fixed QR code target version.</param>
    /// <returns>Returns the raw QR code data which can be used for rendering.</returns>
    // #pragma warning disable CA1822 // Mark members as static
    //     public QRCodeData CreateQrCode(string plainText)
    // #pragma warning restore CA1822 // Mark members as static
    //         => GenerateQrCode(plainText);

    /// <summary>
    /// Calculates the QR code data which than can be used in one of the rendering classes to generate a graphical representation.
    /// </summary>
    /// <param name="binaryData">A byte array which shall be encoded/stored in the QR code.</param>
    /// <param name="eccLevel">The level of error correction data.</param>
    /// <returns>Returns the raw QR code data which can be used for rendering.</returns>
    // #pragma warning disable CA1822 // Mark members as static
    //     public QRCodeData CreateQrCode(byte[] binaryData, ECCLevel eccLevel)
    // #pragma warning restore CA1822 // Mark members as static
    //         => GenerateQrCode(binaryData);

    /// <summary>
    /// Calculates the QR code data which than can be used in one of the rendering classes to generate a graphical representation.
    /// </summary>
    /// <param name="plainText">The payload which shall be encoded in the QR code.</param>
    /// <param name="eccLevel">The level of error correction data.</param>
    /// <param name="forceUtf8">Shall the generator be forced to work in UTF-8 mode?.</param>
    /// <param name="utf8BOM">Should the byte-order-mark be used?.</param>
    /// <param name="eciMode">Which ECI mode shall be used?.</param>
    /// <param name="requestedVersion">Set fixed QR code target version.</param>
    /// <returns>Returns the raw QR code data which can be used for rendering.</returns>
    public static QRCodeData GenerateQrCode(string plainText)
    {
        //eccLevel = ECCLevel.M;  // only valid level ValidateECCLevel(eccLevel);

        // Determine the appropriate version based on segment bit length
        //var version = 2;  //DetermineVersion(segment, eccLevel, requestedVersion);

        // Create data segment from plain text
        DataSegment segment = new AlphanumericDataSegment(plainText);

        // Build the complete bit array for the determined version
        var completeBitArray = segment.ToBitArray();
        return GenerateQrCode(completeBitArray);
    }

    /// <summary>
    /// Creates a data segment from plain text, encoding it appropriately.
    /// </summary>
    // private static DataSegment CreateDataSegment(string plainText, bool forceUtf8, bool utf8BOM, EciMode eciMode)
    // {
    //     return new AlphanumericDataSegment(plainText);

    //     // EncodingMode encoding = EncodingMode.Alphanumeric;

    //     // // Use specialized segment classes based on encoding mode
    //     // return encoding switch
    //     // {
    //     //     EncodingMode.Alphanumeric => new AlphanumericDataSegment(plainText),
    //     //     EncodingMode.Byte => throw new NotImplementedException(),
    //     //     EncodingMode.Numeric => throw new NotImplementedException(),
    //     //     EncodingMode.Kanji => throw new NotImplementedException(),
    //     //     EncodingMode.ECI => throw new NotImplementedException(),
    //     //     _ => throw new InvalidOperationException($"Unsupported encoding mode: {encoding}"),
    //     // };
    // }

    /// <summary>
    /// Determines the appropriate QR code version based on the data segment and error correction level.
    /// Validates that the data fits within the requested version, or finds the minimum version if not specified.
    /// </summary>
    // private static int DetermineVersion(DataSegment segment, ECCLevel eccLevel, int version)
    // {
    //     if (!CapacityTables.TryCalculateMinimumVersion(segment, eccLevel, out var minVersion))
    //     {
    //         return Throw(eccLevel, segment.EncodingMode, version == -1 ? 40 : version);
    //     }
    //     else if (version == -1)
    //     {
    //         return minVersion;
    //     }
    //     else
    //     {
    //         // Version was passed as fixed version via parameter. Thus let's check if chosen version is valid.
    //         if (minVersion > version)
    //         {
    //             // Use a throw-helper to avoid allocating a closure
    //             return Throw(eccLevel, segment.EncodingMode, version);
    //         }

    //         return version;
    //     }

    //     static int Throw(ECCLevel eccLevel, EncodingMode encoding, int version)
    //     {
    //         var maxSizeByte = CapacityTables.GetVersionInfo(version).Details.First(x => x.ErrorCorrectionLevel == eccLevel).CapacityDict[encoding];
    //         throw new Exceptions.DataTooLongException(eccLevel.ToString(), encoding.ToString(), version, maxSizeByte);
    //     }
    // }

    /// <summary>
    /// Calculates the Micro QR code data which then can be used in one of the rendering classes to generate a graphical representation.
    /// </summary>
    /// <param name="plainText">The payload which shall be encoded in the QR code.</param>
    /// <param name="eccLevel">The level of error correction data.</param>
    /// <param name="requestedVersion">Set fixed Micro QR code target version; must be -1 to -4 representing M1 to M4, or 0 for default.</param>
    /// <exception cref="Exceptions.DataTooLongException">Thrown when the payload is too big to be encoded in a QR code.</exception>
    /// <returns>Returns the raw QR code data which can be used for rendering.</returns>
    // public static QRCodeData GenerateMicroQrCode(string plainText, ECCLevel eccLevel = ECCLevel.Default, int requestedVersion = 0)
    // {
    //     if (requestedVersion is < -4 or > 0)
    //     {
    //         throw new ArgumentOutOfRangeException(nameof(requestedVersion), requestedVersion, "Requested version must be -1 to -4 representing M1 to M4, or 0 for default.");
    //     }

    //     _ = ValidateECCLevel(eccLevel);
    //     if (eccLevel == ECCLevel.H)
    //     {
    //         throw new ArgumentOutOfRangeException(nameof(eccLevel), eccLevel, "Micro QR codes does not support error correction level H.");
    //     }

    //     if (eccLevel == ECCLevel.Q && requestedVersion == 0)
    //     {
    //         requestedVersion = -4; // If Q level is specified without a version, automatically select M4 since Q is only supported on M4
    //     }

    //     if (eccLevel == ECCLevel.Q && requestedVersion != -4)
    //     {
    //         throw new ArgumentOutOfRangeException(nameof(eccLevel), eccLevel, "Micro QR codes only supports error correction level Q for version M4.");
    //     }

    //     if (eccLevel != ECCLevel.Default && requestedVersion == -1)
    //     {
    //         throw new ArgumentOutOfRangeException(nameof(eccLevel), eccLevel, "Please specify ECCLevel.Default for version M1.");
    //     }

    //     ArgumentNullException.ThrowIfNull(plainText);

    //     EncodingMode encoding = EncodingMode.Alphanumeric;
    //     BitArray codedText = PlainTextToBinary(plainText, encoding, EciMode.Default, false, false);
    //     var dataInputLength = GetDataLength(encoding, plainText, codedText, false);
    //     var version = requestedVersion;
    //     var minVersion = 2; //CapacityTables.CalculateMinimumMicroVersion(dataInputLength, encoding, eccLevel);

    //     if (version == 0)
    //     {
    //         version = minVersion;
    //     }
    //     else
    //     {
    //         // Version was passed as fixed version via parameter. Thus let's check if chosen version is valid.
    //         if (minVersion < version)
    //         {
    //             var matchedEncoding = CapacityTables.GetVersionInfo(version).Details
    //                 .First(x => x.ErrorCorrectionLevel == eccLevel || (eccLevel == ECCLevel.Default && x.ErrorCorrectionLevel == ECCLevel.L))
    //                 .CapacityDict.TryGetValue(encoding, out var maxSizeByte);
    //             if (!matchedEncoding)
    //             {
    //                 throw new InvalidOperationException("Required encoding is not supported for this version.");
    //             }

    //             throw new Exceptions.DataTooLongException(eccLevel.ToString(), encoding.ToString(), version, maxSizeByte);
    //         }
    //     }

    //     if (version < -1 && eccLevel == ECCLevel.Default)
    //     {
    //         eccLevel = ECCLevel.L;
    //     }

    //     var modeIndicatorLength = -version - 1; // 0 for M1, 1 for M2, 2 for M3, 3 for M4
    //     var countIndicatorLength = GetCountIndicatorLength(version, encoding);
    //     var completeBitArrayLength = modeIndicatorLength + countIndicatorLength + codedText.Length;

    //     var completeBitArray = new BitArray(completeBitArrayLength);

    //     // write mode indicator
    //     var completeBitArrayIndex = 0;
    //     if (version < 0)
    //     {
    //         var encodingValue =
    //             encoding == EncodingMode.Numeric ? 0 :
    //             encoding == EncodingMode.Alphanumeric ? 1 :
    //             encoding == EncodingMode.Byte ? 2 : 3;
    //         completeBitArrayIndex = DecToBin(encodingValue, modeIndicatorLength, completeBitArray, completeBitArrayIndex);
    //     }
    //     else
    //     {
    //         completeBitArrayIndex = DecToBin((int)encoding, 4, completeBitArray, completeBitArrayIndex);
    //     }

    //     // write count indicator
    //     completeBitArrayIndex = DecToBin(dataInputLength, countIndicatorLength, completeBitArray, completeBitArrayIndex);

    //     // write data
    //     for (var i = 0; i < codedText.Length; i++)
    //     {
    //         completeBitArray[completeBitArrayIndex++] = codedText[i];
    //     }

    //     return GenerateQrCode(completeBitArray, eccLevel, version);
    // }

    /// <summary>
    /// Calculates the QR code data which than can be used in one of the rendering classes to generate a graphical representation.
    /// </summary>
    /// <param name="binaryData">A byte array which shall be encoded/stored in the QR code.</param>
    /// <param name="eccLevel">The level of error correction data.</param>
    /// <exception cref="Exceptions.DataTooLongException">Thrown when the payload is too big to be encoded in a QR code.</exception>
    /// <returns>Returns the raw QR code data which can be used for rendering.</returns>
    public static QRCodeData GenerateQrCode(byte[] binaryData)
    {
        //eccLevel = ECCLevel.M;  // only valid level ValidateECCLevel(eccLevel);
        //var version = 2;  //CapacityTables.CalculateMinimumVersion(binaryData.Length, EncodingMode.Byte, eccLevel);

        var countIndicatorLen = 9; //GetCountIndicatorLength(version, EncodingMode.Byte);

        // Convert byte array to bit array, with prefix padding for mode indicator and count indicator
        BitArray bitArray = ToBitArray(binaryData, prefixZeros: 4 + countIndicatorLen);

        // Add mode indicator and count indicator
        var index = DecToBin((int)EncodingMode.Byte, 4, bitArray, 0);
        _ = DecToBin(binaryData.Length, countIndicatorLen, bitArray, index);

        return GenerateQrCode(bitArray);
    }

    /// <summary>
    /// Validates the specified error correction level.
    /// Returns the provided level if it is valid, or the level M if the provided level is Default.
    /// Throws an exception if an invalid level is provided.
    /// </summary>
    // private static ECCLevel ValidateECCLevel(ECCLevel eccLevel) => eccLevel switch
    // {
    //     ECCLevel.L or ECCLevel.M or ECCLevel.Q or ECCLevel.H => eccLevel,
    //     ECCLevel.Default => ECCLevel.M,
    //     _ => throw new ArgumentOutOfRangeException(nameof(eccLevel), eccLevel, "Invalid error correction level."),
    // };

    private static readonly BitArray repeatingPattern = new(
        new bool[] { true, true, true, false, true, true, false, false, false, false, false, true, false, false, false, true });

    /// <summary>
    /// Generates a QR code data structure using the provided BitArray, error correction level, and version.
    /// The BitArray provided is assumed to already include the count, encoding mode, and/or ECI mode information.
    /// </summary>
    /// <param name="bitArray">The BitArray containing the binary-encoded data to be included in the QR code. It should already contain the count, encoding mode, and/or ECI mode information.</param>
    /// <param name="eccLevel">The desired error correction level for the QR code. This impacts how much data can be recovered if damaged.</param>
    /// <param name="version">The version of the QR code, determining the size and complexity of the QR code data matrix.</param>
    /// <returns>A QRCodeData structure containing the full QR code matrix, which can be used for rendering or analysis.</returns>
    private static QRCodeData GenerateQrCode(BitArray bitArray)
    {
        //ECCInfo eccInfo = CapacityTables.GetEccInfo(version, eccLevel);
        var eccInfo = new ECCInfo(
            version: 2,
            errorCorrectionLevel: ECCLevel.M,
            totalDataCodewords: 28,
            totalDataBits: 28 * 8,
            eccPerBlock: 16);

        // Fill up data code word
        PadData();

        // Calculate error correction blocks
        List<CodewordBlock> codeWordWithECC = CalculateECCBlocks();

        // Calculate interleaved code word lengths
        var interleavedLength = CalculateInterleavedLength();

        // Interleave code words
        BitArray interleavedData = InterleaveData();

        // Place interleaved data on module matrix
        QRCodeData qrData = PlaceModules();

        CodewordBlock.ReturnList(codeWordWithECC);

        return qrData;

        // fills the bit array with a repeating pattern to reach the required length
        void PadData()
        {
            var dataLength = eccInfo.TotalDataBits;
            var lengthDiff = dataLength - bitArray.Length;
            if (lengthDiff > 0)
            {
                // set 'write index' to end of existing bit array
                var index = bitArray.Length;

                // extend bit array to required length
                bitArray.Length = dataLength;

                // compute padding length
                var padLength = 4;
                // {
                //     > 0 => 4,
                //     -1 => 3,
                //     -2 => 5,
                //     -3 => 7,
                //     _ => 9,
                // };

                // pad with zeros (or less if not enough room)
                index += padLength;

                // pad to nearest 8 bit boundary
                if ((uint)index % 8 != 0)
                {
                    index += 8 - (int)((uint)index % 8);
                }

                // for m1 and m3 sizes don't fill last 4 bits with repeating pattern
                // if (version == -1)
                // {
                //     dataLength -= 4;
                // }
                // if (version == -3)
                // {
                //     dataLength -= 4;
                // }

                // pad with repeating pattern
                var repeatingPatternIndex = 0;
                while (index < dataLength)
                {
                    bitArray[index++] = repeatingPattern[repeatingPatternIndex++];
                    if (repeatingPatternIndex >= repeatingPattern.Length)
                    {
                        repeatingPatternIndex = 0;
                    }
                }
            }
        }

        List<CodewordBlock> CalculateECCBlocks()
        {
            List<CodewordBlock> codewordBlocks;

            // Generate the generator polynomial using the number of ECC words.
            using (Polynom generatorPolynom = CalculateGeneratorPolynom(eccInfo.ECCPerBlock))
            {
                // Calculate error correction words
                codewordBlocks = CodewordBlock.GetList(eccInfo.BlocksInGroup1 + eccInfo.BlocksInGroup2);
                AddCodeWordBlocks(1, eccInfo.BlocksInGroup1, eccInfo.CodewordsInGroup1, 0, bitArray.Length, generatorPolynom);
                var offset = eccInfo.BlocksInGroup1 * eccInfo.CodewordsInGroup1 * 8;
                AddCodeWordBlocks(2, eccInfo.BlocksInGroup2, eccInfo.CodewordsInGroup2, offset, bitArray.Length - offset, generatorPolynom);
                return codewordBlocks;
            }

            void AddCodeWordBlocks(int blockNum, int blocksInGroup, int codewordsInGroup, int offset2, int count, Polynom generatorPolynom)
            {
                _ = blockNum;
                var groupLength = codewordsInGroup * 8;
                groupLength = groupLength > count ? count : groupLength;
                for (var i = 0; i < blocksInGroup; i++)
                {
                    ArraySegment<byte> eccWordList = CalculateECCWords(bitArray, offset2, groupLength, eccInfo, generatorPolynom);
                    codewordBlocks.Add(new CodewordBlock(offset2, groupLength, eccWordList));
                    offset2 += groupLength;
                }
            }
        }

        // Calculate the length of the interleaved data
        int CalculateInterleavedLength()
        {
            var length = 0;
            var codewords = Math.Max(eccInfo.CodewordsInGroup1, eccInfo.CodewordsInGroup2);
            // if (version is (-1) or (-3))
            // {
            //     codewords--;
            //     length += 4;
            // }

            for (var i = 0; i < codewords; i++)
            {
                foreach (CodewordBlock codeBlock in codeWordWithECC)
                {
                    if ((uint)codeBlock.CodeWordsLength / 8 > i)
                    {
                        length += 8;
                    }
                }
            }

            for (var i = 0; i < eccInfo.ECCPerBlock; i++)
            {
                foreach (CodewordBlock codeBlock in codeWordWithECC)
                {
                    if (codeBlock.ECCWords.Count > i)
                    {
                        length += 8;
                    }
                }
            }

            length += 7;  //CapacityTables.GetRemainderBits(version);
            return length;
        }

        // Interleave the data
        BitArray InterleaveData()
        {
            var data = new BitArray(interleavedLength);
            var pos = 0;
            //var codewords = Math.Max(eccInfo.CodewordsInGroup1, eccInfo.CodewordsInGroup2);
            // if (version is (-1) or (-3))
            // {
            //     codewords--;
            // }

            for (var i = 0; i < Math.Max(eccInfo.CodewordsInGroup1, eccInfo.CodewordsInGroup2); i++)
            {
                foreach (CodewordBlock codeBlock in codeWordWithECC)
                {
                    if ((uint)codeBlock.CodeWordsLength / 8 > i)
                    {
                        //pos = bitArray.CopyTo(data, (int)((uint)i * 8) + codeBlock.CodeWordsOffset, pos, 8);
                        for (var j = 0; j < 8; j++)
                        {
                            data[pos + j] = bitArray[(int)((uint)j * 8) + codeBlock.CodeWordsOffset + j];
                        }
                        pos += 8;
                    }
                }
            }

            /*
            public static int CopyTo(this BitArray source, BitArray destination, int sourceOffset, int destinationOffset, int count)
            {
                for (var i = 0; i < 8; i++)
                {
                    data[pos + i] = source[(int)((uint)i * 8) + codeBlock.CodeWordsOffset + i];
                }

                return pos + 8;
            }
            */

            // if (version is (-1) or (-3))
            // {
            //     pos = bitArray.CopyTo(data, (int)((uint)codewords * 8) + codeWordWithECC[0].CodeWordsOffset, pos, 4);
            // }

            for (var i = 0; i < eccInfo.ECCPerBlock; i++)
            {
                foreach (CodewordBlock codeBlock in codeWordWithECC)
                {
                    if (codeBlock.ECCWords.Count > i)
                    {
                        pos = DecToBin(codeBlock.ECCWords.Array![i], 8, data, pos);
                    }
                }
            }

            return data;
        }

        // Place the modules on the QR code matrix
        QRCodeData PlaceModules()
        {
            // NOTE: qr HAS NO BORDER NOW
            var qr = new QRCodeData();
            var size = qr.ModuleMatrix.Count; // NO BOREDER ADJUST - 8;
            var tempBitArray = new BitArray(18); // version string requires 18 bits
            using (var blockedModules = new ModulePlacer.BlockedModules(size))
            {
                ModulePlacer.PlaceFinderPatterns(qr, blockedModules);
                ModulePlacer.ReserveSeperatorAreas(2, size, blockedModules); //likely wrong
                ModulePlacer.PlaceAlignmentPatterns(qr, AlignmentPatterns.alignmentPattern, blockedModules);
                ModulePlacer.PlaceTimingPatterns(qr, blockedModules);
                ModulePlacer.PlaceDarkModule(qr, 2, blockedModules);
                ModulePlacer.ReserveVersionAreas(size, blockedModules);
                ModulePlacer.PlaceDataWords(qr, interleavedData, blockedModules);
                var maskVersion = ModulePlacer.MaskCode(qr, blockedModules);
                GetFormatString(tempBitArray, 2, ECCLevel.M, maskVersion);
                ModulePlacer.PlaceFormat(qr, tempBitArray, false);
            }

            return qr;
        }
    }

    private static readonly BitArray getFormatGenerator = new(new bool[] { true, false, true, false, false, true, true, false, true, true, true });
    private static readonly BitArray getFormatMask = new(new bool[] { true, false, true, false, true, false, false, false, false, false, true, false, false, true, false });
    //private static readonly BitArray getFormatMicroMask = new(new bool[] { true, false, false, false, true, false, false, false, true, false, false, false, true, false, true });

    /// <summary>
    /// Generates a BitArray containing the format string for a QR code based on the error correction level and mask pattern version.
    /// The format string includes the error correction level, mask pattern version, and error correction coding.
    /// </summary>
    /// <param name="fStrEcc">The <see cref="BitArray"/> to write to, or null to create a new one.</param>
    /// <param name="version">The version number of the QR Code (1-40, or -1 to -4 for Micro QR codes).</param>
    /// <param name="level">The error correction level to be encoded in the format string.</param>
    /// <param name="maskVersion">The mask pattern version to be encoded in the format string.</param>
    private static void GetFormatString(BitArray fStrEcc, int version, ECCLevel level, int maskVersion)
    {
        fStrEcc.Length = 15;
        fStrEcc.SetAll(false);
        // if (version < 0)
        // {
        //     WriteMicroEccLevelAndVersion();
        // }
        // else
        // {
        //     WriteEccLevelAndVersion();
        // }
        WriteEccLevelAndVersion();

        // Apply the format generator polynomial to add error correction to the format string.
        var index = 0;
        var count = 15;
        TrimLeadingZeros(fStrEcc, ref index, ref count);
        while (count > 10)
        {
            for (var i = 0; i < getFormatGenerator.Length; i++)
            {
                fStrEcc[index + i] ^= getFormatGenerator[i];
            }

            TrimLeadingZeros(fStrEcc, ref index, ref count);
        }

        // Align bits with the start of the array.
        ShiftTowardsBit0(fStrEcc, index);

        // Prefix the error correction bits with the ECC level and version number.
        fStrEcc.Length = 10 + 5;
        ShiftAwayFromBit0(fStrEcc, 10 - count + 5);
        // if (version < 0)
        // {
        //     WriteMicroEccLevelAndVersion();
        // }
        // else
        // {
        //     WriteEccLevelAndVersion();
        // }
        WriteEccLevelAndVersion();

        // XOR the format string with a predefined mask to add robustness against errors.
        _ = fStrEcc.Xor(getFormatMask);

        void WriteEccLevelAndVersion()
        {
            // switch (level)
            // {
            //     case ECCLevel.L: // 01
            //         fStrEcc[1] = true;
            //         break;
            //     case ECCLevel.H: // 10
            //         fStrEcc[0] = true;
            //         break;
            //     case ECCLevel.Q: // 11
            //         fStrEcc[0] = true;
            //         fStrEcc[1] = true;
            //         break;
            //     case ECCLevel.Default:
            //         break;
            //     case ECCLevel.M:
            //         break;
            //     default: // M: 00
            //         break;
            // }

            // Insert the 3-bit mask version directly after the error correction level bits.
            _ = DecToBin(maskVersion, 3, fStrEcc, 2);
        }

        // void WriteMicroEccLevelAndVersion()
        // {
        //     switch (version)
        //     {
        //         case -1: // M1
        //             break;
        //         case -2: // M2
        //             fStrEcc[level == ECCLevel.L ? 2 : 1] = true; // 001 for L and 010 for M
        //             break;
        //         case -3: // M3
        //             if (level == ECCLevel.L)
        //             {
        //                 fStrEcc[1] = true; // 011 for L
        //                 fStrEcc[2] = true;
        //             }
        //             else
        //             {
        //                 fStrEcc[0] = true; // 100 for M
        //             }

        //             break;
        //         default: // M4
        //             fStrEcc[0] = true;
        //             if (level == ECCLevel.L) // 101 for L
        //             {
        //                 fStrEcc[2] = true;
        //             }
        //             else if (level == ECCLevel.M) // 110 for M
        //             {
        //                 fStrEcc[1] = true;
        //             }
        //             else // 111 for Q
        //             {
        //                 fStrEcc[1] = true;
        //                 fStrEcc[2] = true;
        //             }

        //             break;
        //     }

        //     // Insert the 2-bit mask version directly after the version / error correction level bits.
        //     var microMaskVersion = maskVersion switch
        //     {
        //         1 => 0,
        //         4 => 1,
        //         6 => 2,
        //         _ => 3,
        //     };
        //     _ = DecToBin(microMaskVersion, 2, fStrEcc, 3);
        // }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TrimLeadingZeros(BitArray fStrEcc, ref int index, ref int count)
    {
        while (count > 0 && !fStrEcc[index])
        {
            index++;
            count--;
        }
    }

    private static void ShiftTowardsBit0(BitArray fStrEcc, int num) => _ = fStrEcc.RightShift(num); // Shift towards bit 0

    private static void ShiftAwayFromBit0(BitArray fStrEcc, int num) => _ = fStrEcc.LeftShift(num); // Shift away from bit 0

    private static readonly BitArray getVersionGenerator = new(new bool[] { true, true, true, true, true, false, false, true, false, false, true, false, true });

    /// <summary>
    /// Encodes the version information of a QR code into a BitArray using error correction coding similar to format information encoding.
    /// This method is used for QR codes version 7 and above.
    /// </summary>
    /// <param name="vStr">A <see cref="BitArray"/> to write the version string to.</param>
    /// <param name="version">The version number of the QR code (7-40).</param>
    private static void GetVersionString(BitArray vStr, int version)
    {
        vStr.Length = 18;
        vStr.SetAll(false);
        _ = DecToBin(version, 6, vStr, 0); // Convert the version number to a 6-bit binary representation.

        var count = vStr.Length;
        var index = 0;
        TrimLeadingZeros(vStr, ref index, ref count); // Trim leading zeros to normalize the version bit sequence.

        // Perform error correction encoding using a polynomial generator (specified by _getVersionGenerator).
        while (count > 12) // The target length of the version information error correction information is 12 bits.
        {
            for (var i = 0; i < getVersionGenerator.Length; i++)
            {
                vStr[index + i] ^= getVersionGenerator[i]; // XOR the current bits with the generator sequence.
            }

            TrimLeadingZeros(vStr, ref index, ref count); // Trim leading zeros after each XOR operation to maintain the proper sequence.
        }

        ShiftTowardsBit0(vStr, index); // Align the bit array so the data starts at index 0.

        // Prefix the error correction encoding with 6 bits containing the version number
        vStr.Length = 12 + 6;
        ShiftAwayFromBit0(vStr, 12 - count + 6);
        _ = DecToBin(version, 6, vStr, 0);
    }

    /// <summary>
    /// Calculates the Error Correction Codewords (ECC) for a segment of data using the provided ECC information.
    /// This method applies polynomial division, using the message polynomial and a generator polynomial,
    /// to compute the remainder which forms the ECC codewords.
    /// </summary>
    private static ArraySegment<byte> CalculateECCWords(BitArray bitArray, int offset, int count, ECCInfo eccInfo, Polynom generatorPolynomBase)
    {
        var eccWords = eccInfo.ECCPerBlock;

        // Calculate the message polynomial from the bit array data.
        Polynom messagePolynom = CalculateMessagePolynom(bitArray, offset, count);
        Polynom generatorPolynom = generatorPolynomBase.Clone();

        // Adjust the exponents in the message polynomial to account for ECC length.
        for (var i = 0; i < messagePolynom.Count; i++)
        {
            messagePolynom[i] = new PolynomItem(
                messagePolynom[i].Coefficient,
                messagePolynom[i].Exponent + eccWords);
        }

        // Adjust the generator polynomial exponents based on the message polynomial.
        for (var i = 0; i < generatorPolynom.Count; i++)
        {
            generatorPolynom[i] = new PolynomItem(
                generatorPolynom[i].Coefficient,
                generatorPolynom[i].Exponent + (messagePolynom.Count - 1));
        }

        // Divide the message polynomial by the generator polynomial to find the remainder.
        Polynom leadTermSource = messagePolynom;
        for (var i = 0; leadTermSource.Count > 0 && leadTermSource[^1].Exponent > 0; i++)
        {
            if (leadTermSource[0].Coefficient == 0) // Simplify the polynomial if the leading coefficient is zero.
            {
                leadTermSource.RemoveAt(0);
                leadTermSource.Add(new PolynomItem(0, leadTermSource[^1].Exponent - 1));
            }
            else // Otherwise, perform polynomial reduction using XOR and multiplication with the generator polynomial.
            {
                // Convert the first coefficient to its corresponding alpha exponent unless it's zero.
                // Coefficients that are zero remain zero because log(0) is undefined.
                var index0Coefficient = leadTermSource[0].Coefficient;
                index0Coefficient = index0Coefficient == 0 ? 0 : GaloisField.GetAlphaExpFromIntVal(index0Coefficient);
                var alphaNotation = new PolynomItem(index0Coefficient, leadTermSource[0].Exponent);
                Polynom resPoly = MultiplyGeneratorPolynomByLeadterm(generatorPolynom, alphaNotation, i);
                ConvertToDecNotationInPlace(resPoly);
                Polynom newPoly = XORPolynoms(leadTermSource, resPoly);

                // Free memory used by the previous polynomials.
                resPoly.Dispose();
                leadTermSource.Dispose();

                // Update the message polynomial with the new remainder.
                leadTermSource = newPoly;
            }
        }

        // Free memory used by the generator polynomial.
        generatorPolynom.Dispose();

        // Convert the resulting polynomial into a byte array representing the ECC codewords.
        var array = ArrayPool<byte>.Shared.Rent(leadTermSource.Count);
        var ret = new ArraySegment<byte>(array, 0, leadTermSource.Count);

        for (var i = 0; i < leadTermSource.Count; i++)
        {
            array[i] = (byte)leadTermSource[i].Coefficient;
        }

        // Free memory used by the message polynomial.
        leadTermSource.Dispose();

        return ret;
    }

    /// <summary>
    /// Converts all polynomial item coefficients from their alpha exponent notation to decimal representation in place.
    /// This conversion facilitates operations that require polynomial coefficients in their integer forms.
    /// </summary>
    private static void ConvertToDecNotationInPlace(Polynom poly)
    {
        for (var i = 0; i < poly.Count; i++)
        {
            // Convert the alpha exponent of the coefficient to its decimal value and create a new polynomial item with the updated coefficient.
            poly[i] = new PolynomItem(GaloisField.GetIntValFromAlphaExp(poly[i].Coefficient), poly[i].Exponent);
        }
    }

    /// <summary>
    /// Checks if a character falls within a specified range.
    /// </summary>
    private static bool IsInRange(char c, char min, char max)
        => (uint)(c - min) <= (uint)(max - min);

    /// <summary>
    /// Converts a segment of a BitArray representing QR code data into a polynomial,
    /// padding the final byte if necessary (for Micro QR variants like M1 or M3).
    /// </summary>
    /// <param name="bitArray">The full bit array representing encoded QR code data.</param>
    /// <param name="offset">Starting position in the bit array.</param>
    /// <param name="bitCount">Total number of bits to convert into codewords.</param>
    /// <returns>A polynomial representing the message codewords.</returns>
    private static Polynom CalculateMessagePolynom(BitArray bitArray, int offset, int bitCount)
    {
        // Calculate how many full 8-bit codewords are present
        var fullBytes = bitCount / 8;

        // Determine if there is a remaining partial byte (e.g., 4 bits for Micro QR M1 and M3 versions)
        var remainingBits = bitCount % 8;

        if (remainingBits > 0)
        {
            // Pad the last byte with zero bits to make it a full 8-bit codeword
            var addlBits = 8 - remainingBits;
            var minBitArrayLength = offset + bitCount + addlBits;

            // Extend BitArray length if needed to fit the padded bits
            if (bitArray.Length < minBitArrayLength)
            {
                bitArray.Length = minBitArrayLength;
            }

            // Pad the remaining bits with false (0) values
            for (var i = 0; i < addlBits; i++)
            {
                bitArray[offset + bitCount + i] = false;
            }
        }

        // Total number of codewords (includes extra for partial byte if present)
        var polynomLength = fullBytes + (remainingBits > 0 ? 1 : 0);

        // Initialize the polynomial
        var messagePol = new Polynom(polynomLength);

        // Exponent for polynomial terms starts from highest degree
        var exponent = polynomLength - 1;

        // Convert each 8-bit segment into a decimal value and add it to the polynomial
        for (var i = 0; i < polynomLength; i++)
        {
            messagePol.Add(new PolynomItem(BinToDec(bitArray, offset, 8), exponent--));
            offset += 8;
        }

        return messagePol;
    }

    /// <summary>
    /// Calculates the generator polynomial used for creating error correction codewords.
    /// </summary>
    /// <param name="numEccWords">The number of error correction codewords to generate.</param>
    /// <returns>A polynomial that can be used to generate ECC codewords.</returns>
    private static Polynom CalculateGeneratorPolynom(int numEccWords)
    {
        var generatorPolynom = new Polynom(2); // Start with the simplest form of the polynomial
        generatorPolynom.Add(new PolynomItem(0, 1));
        generatorPolynom.Add(new PolynomItem(0, 0));

        using (var multiplierPolynom = new Polynom(numEccWords * 2)) // Used for polynomial multiplication
        {
            for (var i = 1; i <= numEccWords - 1; i++)
            {
                // Clear and set up the multiplier polynomial for the current multiplication
                multiplierPolynom.Clear();
                multiplierPolynom.Add(new PolynomItem(0, 1));
                multiplierPolynom.Add(new PolynomItem(i, 0));

                // Multiply the generator polynomial by the current multiplier polynomial
                Polynom newGeneratorPolynom = MultiplyAlphaPolynoms(generatorPolynom, multiplierPolynom);
                generatorPolynom.Dispose();
                generatorPolynom = newGeneratorPolynom;
            }
        }

        return generatorPolynom; // Return the completed generator polynomial
    }

    /// <summary>
    /// Converts a segment of a BitArray into its decimal (integer) equivalent.
    /// </summary>
    /// <returns>The integer value that represents the specified binary data.</returns>
    private static int BinToDec(BitArray bitArray, int offset, int count)
    {
        var ret = 0;
        for (var i = 0; i < count; i++)
        {
            ret ^= bitArray[offset + i] ? 1 << (count - i - 1) : 0;
        }

        return ret;
    }

    /// <summary>
    /// Converts a decimal number to binary and stores the result in a BitArray starting from a specific index.
    /// </summary>
    /// <param name="decNum">The decimal number to convert to binary.</param>
    /// <param name="bits">The number of bits to use for the binary representation (ensuring fixed-width like 8, 16, 32 bits).</param>
    /// <param name="bitList">The BitArray where the binary bits will be stored.</param>
    /// <param name="index">The starting index in the BitArray where the bits will be stored.</param>
    /// <returns>The next index in the BitArray after the last bit placed.</returns>
    private static int DecToBin(int decNum, int bits, BitArray bitList, int index)
    {
        // Convert decNum to binary using a bitwise operation
        for (var i = bits - 1; i >= 0; i--)
        {
            // Check each bit from most significant to least significant
            var bit = (decNum & (1 << i)) != 0;
            bitList[index++] = bit;
        }

        return index;
    }

    /// <summary>
    /// Determines the number of bits used to indicate the count of characters in a segment, depending on the QR code version and the encoding mode.
    /// </summary>
    /// <param name="version">The version of the QR code, which influences the number of bits due to increasing data capacity.</param>
    /// <param name="encMode">The encoding mode (e.g., Numeric, Alphanumeric, Byte) used for the data segment.</param>
    /// <returns>The number of bits needed to represent the character count in the specified encoding mode and version.</returns>
    // private static int GetCountIndicatorLength(int version, EncodingMode encMode)
    // {
    //     return 9;
    //     // Different versions and encoding modes require different lengths of bits to represent the character count efficiently
    //     // if (version == -1)
    //     // {
    //     //     return 3;
    //     // }
    //     // else if (version == -2)
    //     // {
    //     //     return encMode == EncodingMode.Numeric ? 4 : 3;
    //     // }
    //     // else if (version == -3)
    //     // {
    //     //     return encMode == EncodingMode.Numeric ? 5 : encMode == EncodingMode.Kanji ? 3 : 4;
    //     // }
    //     // else if (version == -4)
    //     // {
    //     //     return encMode == EncodingMode.Numeric ? 6 : encMode == EncodingMode.Kanji ? 4 : 5;
    //     // }
    //     // else if (version < 10)
    //     // {
    //     //     return encMode == EncodingMode.Numeric ? 10 : encMode == EncodingMode.Alphanumeric ? 9 : 8;
    //     // }
    //     // else if (version < 27)
    //     // {
    //     //     return encMode == EncodingMode.Numeric ? 12 : encMode == EncodingMode.Alphanumeric ? 11 : encMode == EncodingMode.Byte ? 16 : 10;
    //     // }
    //     // else
    //     // {
    //     //     return encMode == EncodingMode.Numeric ? 14 : encMode == EncodingMode.Alphanumeric ? 13 : encMode == EncodingMode.Byte ? 16 : 12;
    //     // }
    // }

    /// <summary>
    /// Calculates the data length based on the encoding mode, text content, and whether UTF-8 is forced.
    /// </summary>
    /// <param name="encoding">The encoding mode used for the QR code data.</param>
    /// <param name="plainText">The plain text input to be encoded.</param>
    /// <param name="codedText">A BitArray representing the binary data of the encoded text.</param>
    /// <param name="forceUtf8">Flag to determine if UTF-8 encoding should be enforced.</param>
    /// <returns>The length of data in units appropriate to the encoding (bytes or characters).</returns>
    // private static int GetDataLength(EncodingMode encoding, string plainText, BitArray codedText, bool forceUtf8)
    // {
    //     // If UTF-8 is forced or the text is detected as UTF-8, return the number of bytes, otherwise return the character count.
    //     return forceUtf8 || IsUtf8() ? (int)((uint)codedText.Length / 8) : plainText.Length;

    //     bool IsUtf8()
    //     {
    //         return encoding == EncodingMode.Byte && (forceUtf8 || !IsValidISO(plainText));
    //     }
    // }

    /// <summary>
    /// Checks if the given string can be accurately represented and retrieved in ISO-8859-1 encoding.
    /// </summary>
    // private static bool IsValidISO(string input)
    // {
    //     // ISO-8859-1 contains the same characters as UTF-16 for the range 0x00-0xFF.
    //     //   0x00-0x7F: ASCII (0-127)
    //     //   0x80-0x9F: C1 control characters (128-159)
    //     //   0xA0-0xFF: Extended Latin (160-255)
    //     foreach (var c in input)
    //     {
    //         if (c > 0xFF)
    //         {
    //             return false;
    //         }
    //     }

    //     return true;
    // }

    /// <summary>
    /// Converts plain text to a binary format suitable for QR code generation, based on the specified encoding mode.
    /// </summary>
    /// <param name="plainText">The text to be encoded.</param>
    /// <param name="encMode">The encoding mode.</param>
    /// <param name="eciMode">The ECI mode specifying the character encoding to use.</param>
    /// <param name="utf8BOM">Flag indicating whether to prepend a UTF-8 Byte Order Mark.</param>
    /// <param name="forceUtf8">Flag indicating whether UTF-8 encoding is forced.</param>
    /// <returns>A BitArray containing the binary representation of the encoded data.</returns>
    // private static BitArray PlainTextToBinary(string plainText, EncodingMode encMode, EciMode eciMode, bool utf8BOM, bool forceUtf8) => encMode switch
    // {
    //     EncodingMode.Alphanumeric => AlphanumericEncoder.GetBitArray(plainText),
    //     EncodingMode.Byte => PlainTextToBinaryByte(plainText, eciMode, utf8BOM, forceUtf8),
    //     EncodingMode.Numeric => throw new NotImplementedException(),
    //     EncodingMode.Kanji => throw new NotImplementedException(),
    //     EncodingMode.ECI => throw new NotImplementedException(),
    //     _ => emptyBitArray,
    // };

    //private static readonly BitArray emptyBitArray = new(0);

    /// <summary>
    /// Converts an array of bytes into a BitArray, considering the proper bit order within each byte.
    /// Unlike the constructor of BitArray, this function preserves the MSB-to-LSB order within each byte.
    /// </summary>
    /// <param name="byteArray">The byte array to convert into a BitArray.</param>
    /// <param name="prefixZeros">The number of leading zeros to prepend to the resulting BitArray.</param>
    /// <returns>A BitArray representing the bits of the input byteArray, with optional leading zeros.</returns>
    private static BitArray ToBitArray(
        ReadOnlySpan<byte> byteArray, // byte[] has an implicit cast to ReadOnlySpan<byte>
        int prefixZeros = 0)
    {
        // Calculate the total number of bits in the resulting BitArray including the prefix zeros.
        var bitArray = new BitArray((int)((uint)byteArray.Length * 8) + prefixZeros);
        CopyToBitArray(byteArray, bitArray, prefixZeros);
        return bitArray;
    }

    /// <summary>
    /// Converts an array of bytes into a BitArray at a specified offset, considering the proper bit order within each byte.
    /// Unlike the constructor of BitArray, this function preserves the MSB-to-LSB order within each byte.
    /// </summary>
    /// <param name="byteArray">The byte array to convert into a BitArray.</param>
    /// <param name="bitArray">The target BitArray to write to.</param>
    /// <param name="offset">The starting offset in the BitArray where bits will be written.</param>
    private static void CopyToBitArray(
        ReadOnlySpan<byte> byteArray, // byte[] has an implicit cast to ReadOnlySpan<byte>
        BitArray bitArray,
        int offset)
    {
        for (var i = 0; i < byteArray.Length; i++)
        {
            var byteVal = byteArray[i];
            for (var j = 0; j < 8; j++)
            {
                // Set each bit in the BitArray based on the corresponding bit in the byte array.
                // It shifts bits within the byte to align with the MSB-to-LSB order.
                bitArray[(int)((uint)i * 8) + j + offset] = (byteVal & (1 << (7 - j))) != 0;
            }
        }
    }

    /// <summary>
    /// Performs a bitwise XOR operation between two polynomials, commonly used in QR code error correction coding.
    /// </summary>
    /// <returns>The resultant polynomial after performing the XOR operation.</returns>
    private static Polynom XORPolynoms(Polynom messagePolynom, Polynom resPolynom)
    {
        // Determine the larger of the two polynomials to guide the XOR operation.
        var resultPolynom = new Polynom(Math.Max(messagePolynom.Count, resPolynom.Count) - 1);
        Polynom longPoly, shortPoly;
        if (messagePolynom.Count >= resPolynom.Count)
        {
            longPoly = messagePolynom;
            shortPoly = resPolynom;
        }
        else
        {
            longPoly = resPolynom;
            shortPoly = messagePolynom;
        }

        // XOR the coefficients of the two polynomials.
        for (var i = 1; i < longPoly.Count; i++)
        {
            var polItemRes = new PolynomItem(
                longPoly[i].Coefficient ^
                (shortPoly.Count > i ? shortPoly[i].Coefficient : 0),
                messagePolynom[0].Exponent - i);
            resultPolynom.Add(polItemRes);
        }

        return resultPolynom;
    }

    /// <summary>
    /// Multiplies a generator polynomial by a leading term polynomial, reducing the result by a specified lower exponent,
    /// used in constructing QR code error correction codewords.
    /// </summary>
    private static Polynom MultiplyGeneratorPolynomByLeadterm(Polynom genPolynom, PolynomItem leadTerm, int lowerExponentBy)
    {
        var resultPolynom = new Polynom(genPolynom.Count);
        foreach (PolynomItem polItemBase in genPolynom)
        {
            var polItemRes = new PolynomItem(

                (polItemBase.Coefficient + leadTerm.Coefficient) % 255,
                polItemBase.Exponent - lowerExponentBy);
            resultPolynom.Add(polItemRes);
        }

        return resultPolynom;
    }

    /// <summary>
    /// Multiplies two polynomials, treating coefficients as exponents of a primitive element (alpha), which is common in error correction algorithms such as Reed-Solomon.
    /// </summary>
    /// <param name="polynomBase">The first polynomial to multiply.</param>
    /// <param name="polynomMultiplier">The second polynomial to multiply.</param>
    /// <returns>A new polynomial which is the result of the multiplication of the two input polynomials.</returns>
    private static Polynom MultiplyAlphaPolynoms(Polynom polynomBase, Polynom polynomMultiplier)
    {
        // Initialize a new polynomial with a size based on the product of the sizes of the two input polynomials.
        var resultPolynom = new Polynom(polynomMultiplier.Count * polynomBase.Count);

        // Multiply each term of the first polynomial by each term of the second polynomial.
        foreach (PolynomItem polItemBase in polynomMultiplier)
        {
            foreach (PolynomItem polItemMulti in polynomBase)
            {
                // Create a new polynomial term with the coefficients added (as exponents) and exponents summed.
                var polItemRes = new PolynomItem(
                    GaloisField.ShrinkAlphaExp(polItemBase.Coefficient + polItemMulti.Coefficient),
                    polItemBase.Exponent + polItemMulti.Exponent);
                resultPolynom.Add(polItemRes);
            }
        }

        // Identify and merge terms with the same exponent.
        ReadOnlySpan<int> toGlue = GetNotUniqueExponents(resultPolynom, resultPolynom.Count <= 128 ? (stackalloc int[128])[..resultPolynom.Count] : new int[resultPolynom.Count]);
        Span<PolynomItem> gluedPolynoms = toGlue.Length <= 128
            ? (stackalloc PolynomItem[128])[..toGlue.Length]
            : new PolynomItem[toGlue.Length];
        var gluedPolynomsIndex = 0;
        foreach (var exponent in toGlue)
        {
            var coefficient = 0;
            foreach (PolynomItem polynomOld in resultPolynom)
            {
                if (polynomOld.Exponent == exponent)
                {
                    coefficient ^= GaloisField.GetIntValFromAlphaExp(polynomOld.Coefficient);
                }
            }

            // Fix the polynomial terms by recalculating the coefficients based on XORed results.
            var polynomFixed = new PolynomItem(GaloisField.GetAlphaExpFromIntVal(coefficient), exponent);
            gluedPolynoms[gluedPolynomsIndex++] = polynomFixed;
        }

        // Remove duplicated exponents and add the corrected ones back.
        for (var i = resultPolynom.Count - 1; i >= 0; i--)
        {
            if (toGlue.Contains(resultPolynom[i].Exponent))
            {
                resultPolynom.RemoveAt(i);
            }
        }

        foreach (PolynomItem polynom in gluedPolynoms)
        {
            resultPolynom.Add(polynom);
        }

        // Sort the polynomial terms by exponent in descending order.
        resultPolynom.Sort((x, y) => -x.Exponent.CompareTo(y.Exponent));
        return resultPolynom;

        // Auxiliary function to identify exponents that appear more than once in the polynomial.
        static ReadOnlySpan<int> GetNotUniqueExponents(Polynom list, Span<int> buffer)
        {
            // It works as follows:
            // 1. a scratch buffer of the same size as the list is passed in
            // 2. exponents are written / copied to that scratch buffer
            // 3. scratch buffer is sorted, thus the exponents are in order
            // 4. for each item in the scratch buffer (= ordered exponents) it's compared w/ the previous one
            //   * if equal, then increment a counter
            //   * else check if the counter is $>0$ and if so write the exponent to the result
            //
            // For writing the result the same scratch buffer is used, as by definition the index to write the result
            // is `<=` the iteration index, so no overlap, etc. can occur.
            Debug.Assert(list.Count == buffer.Length);

            var idx = 0;
            foreach (PolynomItem row in list)
            {
                buffer[idx++] = row.Exponent;
            }

            buffer.Sort();

            idx = 0;
            var expCount = 0;
            var last = buffer[0];

            for (var i = 1; i < buffer.Length; ++i)
            {
                if (buffer[i] == last)
                {
                    expCount++;
                }
                else
                {
                    if (expCount > 0)
                    {
                        Debug.Assert(idx <= i - 1);

                        buffer[idx++] = last;
                        expCount = 0;
                    }
                }

                last = buffer[i];
            }

            return buffer[..idx];
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public virtual void Dispose() =>
        // left for back-compat
        GC.SuppressFinalize(this);
}
