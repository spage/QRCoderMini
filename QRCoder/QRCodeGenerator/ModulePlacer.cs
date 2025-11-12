using System.Collections;

namespace QRCoder;

public partial class QRCodeGenerator
{
    private static partial class ModulePlacer
    {
        /// <summary>
        /// Places the format information on the QR code, encoding the error correction level and mask pattern used.
        /// </summary>
        /// <param name="qrCode">The QR code data structure to modify.</param>
        /// <param name="formatStr">The bit array containing the format information.</param>
        public static void PlaceFormat(QRCodeData qrCode, BitArray formatStr)
        {
            // The bit pattern is considered an entire 'word' and LSB goes in position 0
            // So, we need to reverse the order of the generated bit pattern, hence the (14 - i) below
            for (var i = 0; i < 15; i++)
            {
                int x1, y1, x2, y2;

                // Standard QR format positions
                x1 = i < 8 ? 8 : i == 8 ? 7 : 14 - i;
                y1 = i < 6 ? i : i < 7 ? i + 1 : 8;
                x2 = i < 8 ? 25 - 1 - i : 8;
                y2 = i < 8 ? 8 : 25 - (15 - i);

                qrCode.ModuleMatrix[y1][x1] = formatStr[14 - i];
                qrCode.ModuleMatrix[y2][x2] = formatStr[14 - i];
            }
        }

        /// <summary>
        /// Applies the most effective mask pattern to the QR code based on minimizing the penalty score,
        /// which evaluates how well the pattern will work for QR scanners.
        /// </summary>
        /// <param name="qrCode">The QR code data structure where the mask will be applied.</param>
        /// <param name="version">The version of the QR code, which determines the size and complexity.</param>
        /// <param name="blockedModules">List of rectangles representing areas that must not be overwritten.</param>
        /// <param name="eccLevel">The error correction level of the QR code, which affects format string values.</param>
        /// <returns>The index of the selected mask pattern.</returns>
        public static int MaskCode(QRCodeData qrCode, BlockedModules blockedModules)
        {
            var selectedPattern = -1;        // no pattern selected yet
            var patternScore = int.MaxValue; // lower score is better

            // Temporary QRCodeData object to test different mask patterns without altering the original.
            var qrTemp = new QRCodeData();


            var formatStr = new BitArray(15);
            for (var maskPattern = 0; maskPattern < 8; maskPattern++)
            {
                Func<int, int, bool> patternFunc = MaskPattern.Patterns[maskPattern];

                // Reset the temporary QR code to the current state of the actual QR code
                // Reuse existing BitArray instances to avoid allocations
                for (var y = 0; y < 25; y++)
                {
                    BitArray sourceRow = qrCode.ModuleMatrix[y];
                    BitArray destRow = qrTemp.ModuleMatrix[y];
                    for (var x = 0; x < 25; x++)
                    {
                        destRow[x] = sourceRow[x];
                    }
                }

                // Place format information using the current mask pattern.
                GetFormatString(formatStr, maskPattern);
                PlaceFormat(qrTemp, formatStr);

                // Apply the mask pattern and calculate the score.
                for (var x = 0; x < 25; x++)
                {
                    for (var y = 0; y < x; y++)
                    {
                        if (!blockedModules.IsBlocked(x, y))
                        {
                            qrTemp.ModuleMatrix[y][x] ^= patternFunc(x, y);
                            qrTemp.ModuleMatrix[x][y] ^= patternFunc(y, x);
                        }
                    }

                    if (!blockedModules.IsBlocked(x, x))
                    {
                        qrTemp.ModuleMatrix[x][x] ^= patternFunc(x, x);
                    }
                }

                //var score = version < 0 ? MaskPattern.ScoreMicro(qrTemp) : MaskPattern.Score(qrTemp);
                var score = MaskPattern.Score(qrTemp);

                // Select the pattern with the lowest score, indicating better QR code readability.
                if (patternScore > score)
                {
                    selectedPattern = maskPattern;
                    patternScore = score;
                }
            }

            // Apply the best mask pattern to the actual QR code.
            Func<int, int, bool> selectedPatternFunc = MaskPattern.Patterns[selectedPattern];
            for (var x = 0; x < 25; x++)
            {
                for (var y = 0; y < x; y++)
                {
                    if (!blockedModules.IsBlocked(x, y))
                    {
                        qrCode.ModuleMatrix[y][x] ^= selectedPatternFunc(x, y);
                        qrCode.ModuleMatrix[x][y] ^= selectedPatternFunc(y, x);
                    }
                }

                if (!blockedModules.IsBlocked(x, x))
                {
                    qrCode.ModuleMatrix[x][x] ^= selectedPatternFunc(x, x);
                }
            }

            return selectedPattern;
        }

        /// <summary>
        /// Places data bits into the QR code's module matrix following a specific pattern that navigates around blocked modules.
        /// </summary>
        /// <param name="qrCode">The QR code data structure where the data bits are to be placed.</param>
        /// <param name="data">The data bits to be placed within the QR code matrix.</param>
        /// <param name="blockedModules">A list of rectangles representing areas within the QR code matrix that should not be modified because they contain other necessary information like format and version info.</param>
        public static void PlaceDataWords(QRCodeData qrCode, BitArray data, BlockedModules blockedModules)
        {
            var up = true; // A boolean flag used to alternate the direction of filling data: up or down.
            var index = 0; // Index to track the current bit position in the data BitArray.
            var count = data.Length; // Total number of data bits to place.

            // Loop from the rightmost column to the leftmost column, skipping one column each time.
            for (var x = 25 - 1; x >= 0; x -= 2)
            {
                // Skip the timing pattern column at position 6 (for normal QR codes only, not Micro QR codes).
                if (x == 6)
                {
                    x = 5;
                }

                // Loop through each row in the current column set.
                for (var yMod = 1; yMod <= 25; yMod++)
                {
                    // Determine the actual y position based on the current fill direction.
                    var y = up ? 25 - yMod : yMod - 1;

                    // Place data if within data length and current position is not blocked.
                    if (index < count && !blockedModules.IsBlocked(x, y))
                    {
                        qrCode.ModuleMatrix[y][x] = data[index++];
                    }

                    if (index < count && x > 0 && !blockedModules.IsBlocked(x - 1, y))
                    {
                        qrCode.ModuleMatrix[y][x - 1] = data[index++];

                    }
                }

                // Switch the fill direction after completing each column set.
                up = !up;
            }
        }

        /// <summary>
        /// Reserves separator areas around the positioning patterns of a QR code to ensure that these crucial areas remain unmodified during data placement.
        /// </summary>
        /// <param name="blockedModules">A list of rectangles representing areas that must not be overwritten.</param>
        public static void ReserveSeperatorAreas(BlockedModules blockedModules)
        {
            // Block areas around the top-left finder pattern
            blockedModules.Add(new Rectangle(7, 0, 1, 8));        // Vertical block near the top left finder pattern
            blockedModules.Add(new Rectangle(0, 7, 7, 1));        // Horizontal block near the top left finder pattern

            // Block areas around the bottom-left finder pattern
            blockedModules.Add(new Rectangle(0, 25 - 8, 8, 1)); // Horizontal block near the bottom left finder pattern
            blockedModules.Add(new Rectangle(7, 25 - 7, 1, 7)); // Vertical block near the bottom left finder pattern

            // Block areas around the top-right finder pattern
            blockedModules.Add(new Rectangle(25 - 8, 0, 1, 8)); // Vertical block near the top right finder pattern
            blockedModules.Add(new Rectangle(25 - 7, 7, 7, 1)); // Horizontal block near the top right finder pattern

        }

        /// <summary>
        /// Reserves areas for version information on QR codes that are version 7 or higher. Also reserves space for format information.
        /// </summary>
        /// <param name="blockedModules">A list of rectangles representing areas that must not be overwritten.</param>
        public static void ReserveVersionAreas(BlockedModules blockedModules)
        {
            // Reserve areas near the timing patterns for version and format information.
            blockedModules.Add(new Rectangle(8, 0, 1, 6));        // Near the top timing pattern
            blockedModules.Add(new Rectangle(8, 7, 1, 1));        // Small square near the top left finder pattern
            blockedModules.Add(new Rectangle(0, 8, 6, 1));        // Near the left timing pattern
            blockedModules.Add(new Rectangle(7, 8, 2, 1));        // Extension of the above block
            blockedModules.Add(new Rectangle(25 - 8, 8, 8, 1)); // Near the right timing pattern
            blockedModules.Add(new Rectangle(8, 25 - 7, 1, 7)); // Near the bottom timing pattern
        }

        /// <summary>
        /// Places a dark module on the QR code matrix as per QR code specification, which requires a dark module at a specific position for all QR codes.
        /// </summary>
        /// <param name="qrCode">The QR code data structure where the dark module is to be placed.</param>
        /// <param name="blockedModules">A list of rectangles representing areas that must not be overwritten, updated to include the dark module.</param>
        public static void PlaceDarkModule(QRCodeData qrCode, BlockedModules blockedModules)
        {
            // Place the dark module, which is always required to be black.
            qrCode.ModuleMatrix[17][8] = true;

            // Block the dark module area to prevent overwriting during further QR code generation steps.
            blockedModules.Add(new Rectangle(8, 17, 1, 1));
        }

        /// <summary>
        /// Places finder patterns on the QR code. Finder patterns are critical for QR code scanners to correctly orient and recognize the QR code.
        /// </summary>
        /// <param name="qrCode">The QR code data structure where the finder patterns will be placed.</param>
        /// <param name="blockedModules">A list of rectangles representing areas that must not be overwritten. This is updated with the areas occupied by the finder patterns.</param>
        public static void PlaceFinderPatterns(QRCodeData qrCode, BlockedModules blockedModules)
        {
            // Loop to place three finder patterns in the top-left, top-right, and bottom-left corners of the QR code.
            for (var i = 0; i < 3; i++)
            {
                // Calculate the x and y starting positions for each finder pattern based on the index.
                var locationX = i == 1 ? 25 - 7 : 0; // Place at top-right if i is 1, otherwise at left side (top or bottom).
                var locationY = i == 2 ? 25 - 7 : 0; // Place at bottom-left if i is 2, otherwise at top (left or right).

                // Nested loops to draw the 7x7 finder pattern at the calculated location.
                for (var x = 0; x < 7; x++)
                {
                    for (var y = 0; y < 7; y++)
                    {
                        // Condition to form the characteristic 5x5 black/white border of the finder pattern.
                        // The center 3x3 area is filled, bordered by a line of white modules, enclosed by a 7x7 black border.
                        if (!(((x == 1 || x == 5) && y > 0 && y < 6) || (x > 0 && x < 6 && (y == 1 || y == 5))))
                        {
                            qrCode.ModuleMatrix[y + locationY][x + locationX] = true;
                        }
                    }
                }

                // Add the area covered by the current finder pattern to the list of blocked modules, preventing any data from being placed there.
                blockedModules.Add(new Rectangle(locationX, locationY, 7, 7));
            }
        }

        /// <summary>
        /// Places alignment patterns on the QR code matrix. Alignment patterns help ensure the scanner can correctly interpret the QR code at various scales and orientations.
        /// </summary>
        /// <param name="qrCode">The QR code data structure where the alignment patterns will be placed.</param>
        /// <param name="blockedModules">A list of rectangles representing areas that must not be overwritten. Updated with the areas occupied by alignment patterns.</param>
        public static void PlaceAlignmentPatterns(QRCodeData qrCode, BlockedModules blockedModules)
        {
            // Place the alignment pattern by setting modules within the 5x5 area.
            // The pattern consists of a 3x3 center block with a single module border.
            for (var x = 0; x < 5; x++)
            {
                for (var y = 0; y < 5; y++)
                {
                    // Create the pattern: a 3x3 block surrounded by a border, with the very center module set.
                    if (y == 0 || y == 4 || x == 0 || x == 4 || (x == 2 && y == 2))
                    {
                        qrCode.ModuleMatrix[16 + y][16 + x] = true;
                    }
                }
            }

            // Add the alignment pattern's area to the list of blocked modules to prevent future overwrites.
            blockedModules.Add(new Rectangle(16, 16, 5, 5));
        }

        /// <summary>
        /// Places timing patterns in the QR code. Timing patterns are alternating dark and light modules that help scanners determine the coordinates of modules within the QR code.
        /// </summary>
        /// <param name="qrCode">The QR code data structure where the timing patterns will be placed.</param>
        /// <param name="blockedModules">A list of rectangles representing areas that must not be overwritten. Updated with the areas occupied by timing patterns.</param>
        public static void PlaceTimingPatterns(QRCodeData qrCode, BlockedModules blockedModules)
        {
            // Place timing patterns starting from the 8th module to the size - 8 to avoid overlapping with finder patterns.
            for (var i = 8; i < 25 - 8; i++)
            {
                if (i % 2 == 0) // Place a dark module every other module to create the alternating pattern.
                {
                    qrCode.ModuleMatrix[6][i] = true; // Horizontal timing pattern
                    qrCode.ModuleMatrix[i][6] = true; // Vertical timing pattern
                }
            }

            // Add the areas occupied by the timing patterns to the list of blocked modules.
            blockedModules.Add(new Rectangle(6, 8, 1, 9)); // Horizontal timing pattern area
            blockedModules.Add(new Rectangle(8, 6, 9, 1)); // Vertical timing pattern area
        }
    }
}
