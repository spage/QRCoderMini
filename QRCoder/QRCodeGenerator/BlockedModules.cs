using System.Collections;

namespace QRCoder;

public partial class QRCodeGenerator
{
    private static partial class ModulePlacer
    {
        /// <summary>
        /// Struct that represents blocked modules using rectangles.
        /// </summary>
        public readonly struct BlockedModules
        {
            private readonly BitArray[] blockedModules;

            /// <summary>
            /// Initializes a new instance of the <see cref="BlockedModules"/> struct with a specified size.
            /// </summary>
            /// <param name="size">The size of the blocked modules matrix.</param>
            public BlockedModules(int size)
            {
                blockedModules = new BitArray[size];
                for (var i = 0; i < size; i++)
                {
                    blockedModules[i] = new BitArray(size);
                }
            }

            /// <summary>
            /// Adds a blocked module defined by the specified rectangle.
            /// </summary>
            /// <param name="rect">The rectangle that defines the blocked module.</param>
            public readonly void Add(Rectangle rect)
            {
                for (var y = rect.Y; y < rect.Y + rect.Height; y++)
                {
                    for (var x = rect.X; x < rect.X + rect.Width; x++)
                    {
                        blockedModules[y][x] = true;
                    }
                }
            }

            /// <summary>
            /// Checks if the specified coordinates are blocked.
            /// </summary>
            /// <param name="x">The x-coordinate to check.</param>
            /// <param name="y">The y-coordinate to check.</param>
            /// <returns><c>true</c> if the coordinates are blocked; otherwise, <c>false</c>.</returns>
            public readonly bool IsBlocked(int x, int y)
                => blockedModules[y][x];
        }
    }
}
