using System.Collections;

namespace QRCoder;

/// <summary>
/// Provides functionality to generate QR code data that can be used to create QR code images.
/// </summary>
public static partial class QRCodeGenerator
{
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
        // Create data segment from plain text
        var segment = new AlphanumericDataSegment(plainText);

        // Build the complete bit array for the determined version
        var completeBitArray = segment.ToBitArray();
        return GenerateQrCode(completeBitArray);
    }

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

        return qrData;

        // fills the bit array with a repeating pattern to reach the required length
        void PadData()
        {
            // Version 2, ECC Level M: 28 codewords = 224 bits of data capacity
            var dataLength = 28 * 8;
            var lengthDiff = dataLength - bitArray.Length;
            if (lengthDiff > 0)
            {
                // set 'write index' to end of existing bit array
                var index = bitArray.Length;

                // extend bit array to required length
                bitArray.Length = dataLength;

                // compute padding length
                var padLength = 4;

                // pad with zeros (or less if not enough room)
                index += padLength;

                // pad to nearest 8 bit boundary
                if ((uint)index % 8 != 0)
                {
                    index += 8 - (int)((uint)index % 8);
                }

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
            // Version 2, ECC Level M: 1 block, 28 data codewords, 16 ECC codewords
            // Generate the generator polynomial using 16 ECC words (hardcoded for Version 2, ECC Level M)
            Polynom generatorPolynom = CalculateGeneratorPolynom(16);

            // Calculate error correction words
            List<CodewordBlock> codewordBlocks = new(2);
            AddCodeWordBlocks(1, 1, 28, 0, bitArray.Length, generatorPolynom);
            var offset = 1 * 28 * 8;
            AddCodeWordBlocks(2, 1, 28, offset, bitArray.Length - offset, generatorPolynom);
            return codewordBlocks;

            void AddCodeWordBlocks(int blockNum, int blocksInGroup, int codewordsInGroup, int offset2, int count, Polynom generatorPolynom)
            {
                _ = blockNum;
                var groupLength = codewordsInGroup * 8;
                groupLength = groupLength > count ? count : groupLength;
                for (var i = 0; i < blocksInGroup; i++)
                {
                    var eccWordList = CalculateECCWords(bitArray, offset2, groupLength, generatorPolynom);
                    codewordBlocks.Add(new CodewordBlock(offset2, groupLength, eccWordList));
                    offset2 += groupLength;
                }
            }
        }

        // Calculate the length of the interleaved data
        int CalculateInterleavedLength()
        {
            var length = 0;
            var codewords = 28;

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

            for (var i = 0; i < 16; i++)
            {
                foreach (CodewordBlock codeBlock in codeWordWithECC)
                {
                    if (codeBlock.ECCWords.Length > i)
                    {
                        length += 8;
                    }
                }
            }

            length += 7;
            return length;
        }

        // Interleave the data
        BitArray InterleaveData()
        {
            var data = new BitArray(interleavedLength);
            var pos = 0;

            for (var i = 0; i < 28; i++)
            {
                foreach (CodewordBlock codeBlock in codeWordWithECC)
                {
                    if ((uint)codeBlock.CodeWordsLength / 8 > i)
                    {
                        // Inline copy of 8 bits from bitArray to data
                        for (var j = 0; j < 8; j++)
                        {
                            data[pos + j] = bitArray[(int)((uint)i * 8) + codeBlock.CodeWordsOffset + j];
                        }
                        pos += 8;
                    }
                }
            }

            for (var i = 0; i < 16; i++)
            {
                foreach (CodewordBlock codeBlock in codeWordWithECC)
                {
                    if (codeBlock.ECCWords.Length > i)
                    {
                        pos = DecToBin(codeBlock.ECCWords[i], 8, data, pos);
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
            var tempBitArray = new BitArray(18); // version string requires 18 bits
            var blockedModules = new ModulePlacer.BlockedModules(25);
            ModulePlacer.PlaceFinderPatterns(qr, blockedModules);
            ModulePlacer.ReserveSeperatorAreas(blockedModules);
            ModulePlacer.PlaceAlignmentPatterns(qr, blockedModules);
            ModulePlacer.PlaceTimingPatterns(qr, blockedModules);
            ModulePlacer.PlaceDarkModule(qr, blockedModules);
            ModulePlacer.ReserveVersionAreas(blockedModules);
            ModulePlacer.PlaceDataWords(qr, interleavedData, blockedModules);
            var maskVersion = ModulePlacer.MaskCode(qr, blockedModules);
            GetFormatString(tempBitArray, maskVersion);
            ModulePlacer.PlaceFormat(qr, tempBitArray);

            return qr;
        }
    }

    private static readonly BitArray getFormatGenerator = new(new bool[] { true, false, true, false, false, true, true, false, true, true, true });
    private static readonly BitArray getFormatMask = new(new bool[] { true, false, true, false, true, false, false, false, false, false, true, false, false, true, false });

    /// <summary>
    /// Generates a BitArray containing the format string for a QR code based on the error correction level and mask pattern version.
    /// The format string includes the error correction level, mask pattern version, and error correction coding.
    /// </summary>
    /// <param name="fStrEcc">The <see cref="BitArray"/> to write to, or null to create a new one.</param>
    /// <param name="version">The version number of the QR Code (1-40, or -1 to -4 for Micro QR codes).</param>
    /// <param name="level">The error correction level to be encoded in the format string.</param>
    /// <param name="maskVersion">The mask pattern version to be encoded in the format string.</param>
    private static void GetFormatString(BitArray fStrEcc, int maskVersion)
    {
        fStrEcc.Length = 15;
        fStrEcc.SetAll(false);
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
        WriteEccLevelAndVersion();

        // XOR the format string with a predefined mask to add robustness against errors.
        _ = fStrEcc.Xor(getFormatMask);

        void WriteEccLevelAndVersion()
        {
            // Insert the 3-bit mask version directly after the error correction level bits.
            _ = DecToBin(maskVersion, 3, fStrEcc, 2);
        }
    }

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

    /// <summary>
    /// Calculates the Error Correction Codewords (ECC) for a segment of data using the provided ECC information.
    /// This method applies polynomial division, using the message polynomial and a generator polynomial,
    /// to compute the remainder which forms the ECC codewords.
    /// </summary>
    private static byte[] CalculateECCWords(BitArray bitArray, int offset, int count, Polynom generatorPolynomBase)
    {
        var eccWords = 16;

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

                // Update the message polynomial with the new remainder.
                leadTermSource = newPoly;
            }
        }

        // Convert the resulting polynomial into a byte array representing the ECC codewords.
        var array = new byte[leadTermSource.Count];

        for (var i = 0; i < leadTermSource.Count; i++)
        {
            array[i] = (byte)leadTermSource[i].Coefficient;
        }

        return array;
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

        var multiplierPolynom = new Polynom(numEccWords * 2); // Used for polynomial multiplication

        for (var i = 1; i <= numEccWords - 1; i++)
        {
            // Clear and set up the multiplier polynomial for the current multiplication
            multiplierPolynom.Clear();
            multiplierPolynom.Add(new PolynomItem(0, 1));
            multiplierPolynom.Add(new PolynomItem(i, 0));

            // Multiply the generator polynomial by the current multiplier polynomial
            Polynom newGeneratorPolynom = MultiplyAlphaPolynoms(generatorPolynom, multiplierPolynom);
            generatorPolynom = newGeneratorPolynom;
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
                        buffer[idx++] = last;
                        expCount = 0;
                    }
                }

                last = buffer[i];
            }

            return buffer[..idx];
        }
    }
}
