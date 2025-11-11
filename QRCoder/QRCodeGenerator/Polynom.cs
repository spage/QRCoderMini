using System.Diagnostics;

namespace QRCoder;

public partial class QRCodeGenerator
{
    /// <summary>
    /// Represents a polynomial, which is a sum of polynomial terms.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Polynom"/> struct with a specified number of initial capacity for polynomial terms.
    /// </remarks>
    /// <param name="count">The initial capacity of the polynomial items list.</param>
    private struct Polynom(int count) : IDisposable
    {
        private PolynomItem[] polyItems = RentArray(count);

        /// <summary>
        /// Adds a polynomial term to the polynomial.
        /// </summary>
        public void Add(PolynomItem item) =>
            polyItems[Count++] = item;

        /// <summary>
        /// Removes the polynomial term at the specified index.
        /// </summary>
        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)Count)
            {
                ThrowIndexArgumentOutOfRangeException();
            }

            if (index < Count - 1)
            {
                Array.Copy(polyItems, index + 1, polyItems, index, Count - index - 1);
            }

            Count--;
        }

        /// <summary>
        /// Gets or sets a polynomial term at the specified index.
        /// </summary>
        public readonly PolynomItem this[int index]
        {
            get
            {
                if ((uint)index >= Count)
                {
                    ThrowIndexArgumentOutOfRangeException();
                }

                return polyItems[index];
            }

            set
            {
                if ((uint)index >= Count)
                {
                    ThrowIndexArgumentOutOfRangeException();
                }

                polyItems[index] = value;
            }
        }

        [StackTraceHidden]
        private static void ThrowIndexArgumentOutOfRangeException() => throw new ArgumentOutOfRangeException("index");

        /// <summary>
        /// Gets the number of polynomial terms in the polynomial.
        /// </summary>
        public int Count { get; private set; } = 0;

        /// <summary>
        /// Removes all polynomial terms from the polynomial.
        /// </summary>
        public void Clear() => Count = 0;

        /// <summary>
        /// Clones the polynomial, creating a new instance with the same polynomial terms.
        /// </summary>
        public readonly Polynom Clone()
        {
            var newPolynom = new Polynom(Count);
            Array.Copy(polyItems, newPolynom.polyItems, Count);
            newPolynom.Count = Count;
            return newPolynom;
        }

        /// <summary>
        /// Sorts the collection of <see cref="PolynomItem"/> using a custom comparer function.
        /// </summary>
        /// <param name="comparer">
        /// A function that compares two <see cref="PolynomItem"/> objects and returns an integer indicating their relative order:
        /// less than zero if the first is less than the second, zero if they are equal, or greater than zero if the first is greater than the second.
        /// </param>
        public readonly void Sort(Func<PolynomItem, PolynomItem, int> comparer)
        {
            ArgumentNullException.ThrowIfNull(comparer);

            PolynomItem[] items = polyItems ?? throw new ObjectDisposedException(nameof(Polynom));

            if (Count <= 1)
            {
                return; // Nothing to sort if the list is empty or contains only one element
            }

            void QuickSort(int left, int right)
            {
                var i = left;
                var j = right;
                PolynomItem pivot = items[(left + right) / 2];

                while (i <= j)
                {
                    while (comparer(items[i], pivot) < 0)
                    {
                        i++;
                    }

                    while (comparer(items[j], pivot) > 0)
                    {
                        j--;
                    }

                    if (i <= j)
                    {
                        // Swap items[i] and items[j]
                        (items[j], items[i]) = (items[i], items[j]);
                        i++;
                        j--;
                    }
                }

                // Recursively sort the sub-arrays
                if (left < j)
                {
                    QuickSort(left, j);
                }

                if (i < right)
                {
                    QuickSort(i, right);
                }
            }

            QuickSort(0, Count - 1);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ReturnArray(polyItems);
            polyItems = null!;
        }

        /// <summary>
        /// Rents memory for the polynomial terms from the shared memory pool.
        /// </summary>
        private static PolynomItem[] RentArray(int count)
            => System.Buffers.ArrayPool<PolynomItem>.Shared.Rent(count);

        /// <summary>
        /// Returns memory allocated for the polynomial terms back to the shared memory pool.
        /// </summary>
        private static void ReturnArray(PolynomItem[] array)
            => System.Buffers.ArrayPool<PolynomItem>.Shared.Return(array);

        /// <summary>
        /// Returns an enumerator that iterates through the polynomial terms.
        /// </summary>
        public readonly PolynumEnumerator GetEnumerator() => new(this);

        /// <summary>
        /// Value type enumerator for the <see cref="Polynom"/> struct.
        /// </summary>
        public struct PolynumEnumerator(Polynom polynom)
        {
            private Polynom polynom = polynom;
            private int index = -1;

            public readonly PolynomItem Current => polynom[index];

            public bool MoveNext() => ++index < polynom.Count;
        }
    }
}
