using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MAVLinkAPI.Ext;
using MAVLinkAPI.Util;
using NUnit.Framework;

namespace MAVLinkAPI.Tests.Ext
{
    [TestFixture]
    [TestOf(typeof(EnumerableExtensions))]
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        [Test]
        public void ZipWithNext_EmptySequence_ReturnsEmptySequence()
        {
            var result = Enumerable.Empty<int>().ZipWithNext().ToList();
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ZipWithNext_SingleElement_ReturnsOneElementWithNullNext()
        {
            var result = new[] { 1 }.ZipWithNext().ToList();
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo((1, (Box<int>)null)));
        }

        [Test]
        public void ZipWithNext_MultipleElements_ReturnsCorrectPairs()
        {
            var result = new[] { 1, 2, 3 }.ZipWithNext().ToList();
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0], Is.EqualTo((1, Box.Of(2))));
            Assert.That(result[1], Is.EqualTo((2, Box.Of(3))));
            Assert.That(result[2], Is.EqualTo((3, (Box<int>)null)));
        }

        [Test]
        public void ZipWithNext_ConsumesSequenceOnlyOnce()
        {
            var sequence = new SequenceCounter<int>(new[] { 1, 2, 3 });
            var result = sequence.ZipWithNext().ToList();

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(sequence.EnumerationCount, Is.EqualTo(1));
        }

        [Test]
        public void UnionNullSafe_FirstIsNull_ReturnsSecond()
        {
            IEnumerable<int>? first = null;
            var second = new[] { 1, 2, 3 };
            var result = first.UnionNullSafe(second);
            Assert.That(result, Is.EqualTo(second));
        }

        [Test]
        public void UnionNullSafe_SecondIsNull_ReturnsFirst()
        {
            var first = new[] { 1, 2, 3 };
            IEnumerable<int>? second = null;
            var result = first.UnionNullSafe(second);
            Assert.That(result, Is.EqualTo(first));
        }

        [Test]
        public void UnionNullSafe_BothAreNull_ReturnsNull()
        {
            IEnumerable<int>? first = null;
            IEnumerable<int>? second = null;
            var result = first.UnionNullSafe(second);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void UnionNullSafe_FirstIsEmpty_ReturnsSecond()
        {
            var first = Enumerable.Empty<int>();
            var second = new[] { 1, 2, 3 };
            var result = first.UnionNullSafe(second)!.ToList();
            Assert.That(result, Is.EquivalentTo(second));
        }

        [Test]
        public void UnionNullSafe_SecondIsEmpty_ReturnsFirst()
        {
            var first = new[] { 1, 2, 3 };
            var second = Enumerable.Empty<int>();
            var result = first.UnionNullSafe(second)!.ToList();
            Assert.That(result, Is.EquivalentTo(first));
        }

        [Test]
        public void UnionNullSafe_BothAreEmpty_ReturnsEmpty()
        {
            var first = Enumerable.Empty<int>();
            var second = Enumerable.Empty<int>();
            var result = first.UnionNullSafe(second)!.ToList();
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void UnionNullSafe_BothHaveElementsWithOverlap_ReturnsUnion()
        {
            var first = new[] { 1, 2, 3 };
            var second = new[] { 3, 4, 5 };
            var expected = new[] { 1, 2, 3, 4, 5 };
            var result = first.UnionNullSafe(second)!.ToList();
            Assert.That(result, Is.EquivalentTo(expected));
        }

        [Test]
        public void UnionNullSafe_BothHaveElementsNoOverlap_ReturnsUnion()
        {
            var first = new[] { 1, 2 };
            var second = new[] { 3, 4 };
            var expected = new[] { 1, 2, 3, 4 };
            var result = first.UnionNullSafe(second)!.ToList();
            Assert.That(result, Is.EquivalentTo(expected));
        }
    }

// Helper class to count sequence enumerations
    public class SequenceCounter<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _source;
        public int EnumerationCount { get; private set; }

        public SequenceCounter(IEnumerable<T> source)
        {
            _source = source;
        }

        public IEnumerator<T> GetEnumerator()
        {
            EnumerationCount++;
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}