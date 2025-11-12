namespace QRCoder;

public partial class QRCodeGenerator
{
    /// <summary>
    /// Represents a polynomial, which is a sum of polynomial terms.
    /// </summary>
    private sealed class Polynom(int capacity)
    {
        private readonly List<PolynomItem> polyItems = new(capacity);

        /// <summary>
        /// Adds a polynomial term to the polynomial.
        /// </summary>
        public void Add(PolynomItem item) => polyItems.Add(item);

        /// <summary>
        /// Removes the polynomial term at the specified index.
        /// </summary>
        public void RemoveAt(int index) => polyItems.RemoveAt(index);

        /// <summary>
        /// Gets or sets a polynomial term at the specified index.
        /// </summary>
        public PolynomItem this[int index]
        {
            get => polyItems[index];
            set => polyItems[index] = value;
        }

        /// <summary>
        /// Gets the number of polynomial terms in the polynomial.
        /// </summary>
        public int Count => polyItems.Count;

        /// <summary>
        /// Removes all polynomial terms from the polynomial.
        /// </summary>
        public void Clear() => polyItems.Clear();

        /// <summary>
        /// Clones the polynomial, creating a new instance with the same polynomial terms.
        /// </summary>
        public Polynom Clone()
        {
            var newPolynom = new Polynom(Count);
            newPolynom.polyItems.AddRange(polyItems);
            return newPolynom;
        }

        /// <summary>
        /// Sorts the collection of <see cref="PolynomItem"/> using a custom comparer function.
        /// </summary>
        public void Sort(Func<PolynomItem, PolynomItem, int> comparer) => polyItems.Sort((x, y) => comparer(x, y));

        /// <summary>
        /// Returns an enumerator that iterates through the polynomial terms.
        /// </summary>
        public List<PolynomItem>.Enumerator GetEnumerator() => polyItems.GetEnumerator();
    }
}