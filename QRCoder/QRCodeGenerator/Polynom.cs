namespace QRCoder;

public partial class QRCodeGenerator
{
    /// <summary>
    /// Represents a polynomial, which is a sum of polynomial terms.
    /// </summary>
    /// <remarks>
    /// Uses a pooled array internally. This type is a class to ensure a single owner of the pooled array
    /// and to avoid the risk of accidental copies that occur with structs.
    /// </remarks>
    private sealed class Polynom(int count) : IDisposable
    {
        private PolynomItem[]? polyItems = RentArray(count);
        private bool disposed;

        /// <summary>
        /// Adds a polynomial term to the polynomial.
        /// </summary>
        public void Add(PolynomItem item) => polyItems![Count++] = item;

        /// <summary>
        /// Removes the polynomial term at the specified index.
        /// </summary>
        public void RemoveAt(int index)
        {
            if (index < Count - 1)
            {
                Array.Copy(polyItems!, index + 1, polyItems!, index, Count - index - 1);
            }

            Count--;
        }

        /// <summary>
        /// Gets or sets a polynomial term at the specified index.
        /// </summary>
        public PolynomItem this[int index]
        {
            get => polyItems![index];
            set => polyItems![index] = value;
        }

        /// <summary>
        /// Gets the number of polynomial terms in the polynomial.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Removes all polynomial terms from the polynomial.
        /// </summary>
        public void Clear() => Count = 0;

        /// <summary>
        /// Clones the polynomial, creating a new instance with the same polynomial terms.
        /// </summary>
        public Polynom Clone()
        {
            var newPolynom = new Polynom(Count);
            Array.Copy(polyItems!, newPolynom.polyItems!, Count);
            newPolynom.Count = Count;
            return newPolynom;
        }

        /// <summary>
        /// Sorts the collection of <see cref="PolynomItem"/> using a custom comparer function.
        /// </summary>
        public void Sort(Func<PolynomItem, PolynomItem, int> comparer)
        {
            PolynomItem[] items = polyItems!;
            if (Count <= 1)
            {
                return;
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
                        (items[j], items[i]) = (items[i], items[j]);
                        i++;
                        j--;
                    }
                }

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
            if (disposed)
            {
                return;
            }

            disposed = true;

            if (polyItems is not null)
            {
                // If PolynomItem holds managed references and you worry about retention/leaks,
                // consider returning the array with clearArray: true:
                // System.Buffers.ArrayPool<PolynomItem>.Shared.Return(polyItems, clearArray: true);
                ReturnArray(polyItems);
                polyItems = null;
            }
        }

        private static PolynomItem[] RentArray(int count)
            => System.Buffers.ArrayPool<PolynomItem>.Shared.Rent(count);

        private static void ReturnArray(PolynomItem[] array)
            => System.Buffers.ArrayPool<PolynomItem>.Shared.Return(array);

        /// <summary>
        /// Returns an enumerator that iterates through the polynomial terms.
        /// </summary>
        public PolynumEnumerator GetEnumerator() => new(this);

        /// <summary>
        /// Value type enumerator for the <see cref="Polynom"/> class.
        /// </summary>
        public struct PolynumEnumerator
        {
            private readonly Polynom polynom;
            private int index;

            internal PolynumEnumerator(Polynom polynom)
            {
                this.polynom = polynom;
                index = -1;
            }

            public readonly PolynomItem Current => polynom[index];

            public bool MoveNext() => ++index < polynom.Count;
        }
    }
}