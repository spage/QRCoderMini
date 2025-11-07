namespace QRCoder;

using System.Diagnostics;
using System.Text;

public partial class QRCodeGenerator
{
    /// <summary>
    /// Represents a polynomial, which is a sum of polynomial terms.
    /// </summary>
    private struct Polynom : IDisposable
    {
        private PolynomItem[] polyItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="Polynom"/> struct with a specified number of initial capacity for polynomial terms.
        /// </summary>
        /// <param name="count">The initial capacity of the polynomial items list.</param>
        public Polynom(int count)
        {
            this.Count = 0;
            this.polyItems = RentArray(count);
        }

        /// <summary>
        /// Adds a polynomial term to the polynomial.
        /// </summary>
        public void Add(PolynomItem item)
        {
            this.AssertCapacity(this.Count + 1);
            this.polyItems[this.Count++] = item;
        }

        /// <summary>
        /// Removes the polynomial term at the specified index.
        /// </summary>
        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)this.Count)
            {
                ThrowIndexArgumentOutOfRangeException();
            }

            if (index < this.Count - 1)
            {
                Array.Copy(this.polyItems, index + 1, this.polyItems, index, this.Count - index - 1);
            }

            this.Count--;
        }

        /// <summary>
        /// Gets or sets a polynomial term at the specified index.
        /// </summary>
        public PolynomItem this[int index]
        {
            get
            {
                if ((uint)index >= this.Count)
                {
                    ThrowIndexArgumentOutOfRangeException();
                }

                return this.polyItems[index];
            }

            set
            {
                if ((uint)index >= this.Count)
                {
                    ThrowIndexArgumentOutOfRangeException();
                }

                this.polyItems[index] = value;
            }
        }

        [StackTraceHidden]
        private static void ThrowIndexArgumentOutOfRangeException() => throw new ArgumentOutOfRangeException("index");

        /// <summary>
        /// Gets the number of polynomial terms in the polynomial.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Removes all polynomial terms from the polynomial.
        /// </summary>
        public void Clear() => this.Count = 0;

        /// <summary>
        /// Clones the polynomial, creating a new instance with the same polynomial terms.
        /// </summary>
        public Polynom Clone()
        {
            var newPolynom = new Polynom(this.Count);
            Array.Copy(this.polyItems, newPolynom.polyItems, this.Count);
            newPolynom.Count = this.Count;
            return newPolynom;
        }

        /// <summary>
        /// Sorts the collection of <see cref="PolynomItem"/> using a custom comparer function.
        /// </summary>
        /// <param name="comparer">
        /// A function that compares two <see cref="PolynomItem"/> objects and returns an integer indicating their relative order:
        /// less than zero if the first is less than the second, zero if they are equal, or greater than zero if the first is greater than the second.
        /// </param>
        public void Sort(Func<PolynomItem, PolynomItem, int> comparer)
        {
            ArgumentNullException.ThrowIfNull(comparer);

            var items = this.polyItems ?? throw new ObjectDisposedException(nameof(Polynom));

            if (this.Count <= 1)
            {
                return; // Nothing to sort if the list is empty or contains only one element
            }

            void QuickSort(int left, int right)
            {
                int i = left;
                int j = right;
                var pivot = items[(left + right) / 2];

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
                        var temp = items[i];
                        items[i] = items[j];
                        items[j] = temp;
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

            QuickSort(0, this.Count - 1);
        }

        /// <summary>
        /// Returns a string that represents the polynomial in standard algebraic notation.
        /// Example output: "a^2*x^3 + a^5*x^1 + a^3*x^0", which represents the polynomial 2x³ + 5x + 3.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < this.Count; i++)
            {
                var polyItem = this.polyItems[i];
                sb.Append("a^" + polyItem.Coefficient + "*x^" + polyItem.Exponent + " + ");
            }

            // Remove the trailing " + " if the string builder has added terms
            if (sb.Length > 0)
            {
                sb.Length -= 3;
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ReturnArray(this.polyItems);
            this.polyItems = null!;
        }

        /// <summary>
        /// Ensures that the polynomial has enough capacity to store the specified number of polynomial terms.
        /// </summary>
        private void AssertCapacity(int min)
        {
            if (this.polyItems.Length < min)
            {
                // All math by QRCoder should be done with fixed polynomials, so we don't need to grow the capacity.
                ThrowNotSupportedException();

                // Sample code for growing the capacity:
                // var newArray = RentArray(Math.Max(min - 1, 8) * 2); // Grow by 2x, but at least by 8
                // Array.Copy(_polyItems, newArray, _length);
                // ReturnArray(_polyItems);
                // _polyItems = newArray;
            }

            [StackTraceHidden]
            void ThrowNotSupportedException() => throw new NotSupportedException("The polynomial capacity is fixed and cannot be increased.");
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
        public PolynumEnumerator GetEnumerator() => new PolynumEnumerator(this);

        /// <summary>
        /// Value type enumerator for the <see cref="Polynom"/> struct.
        /// </summary>
        public struct PolynumEnumerator
        {
            private Polynom polynom;
            private int index;

            public PolynumEnumerator(Polynom polynom)
            {
                this.polynom = polynom;
                this.index = -1;
            }

            public PolynomItem Current => this.polynom[this.index];

            public bool MoveNext() => ++this.index < this.polynom.Count;
        }
    }
}
