using System;
using MAVLinkAPI.Util.NullSafety;
using NUnit.Framework;

namespace MAVLinkAPI.Tests.Util.Maybe
{
    [TestFixture]
    public class MaybeSpec
    {
        [Test]
        public void Some_CreatesValueWithContent()
        {
            var maybe = Maybe<int>.Some(42);
            Assert.That(maybe.HasValue, Is.True);
            Assert.That(maybe.Value, Is.EqualTo(42));
        }

        [Test]
        public void None_CreatesEmptyValue()
        {
            var maybe = Maybe<int>.None();
            Assert.That(maybe.HasValue, Is.False);
        }

        [Test]
        public void Value_ThrowsWhenEmpty()
        {
            var maybe = Maybe<int>.None();
            Assert.Throws<InvalidOperationException>(() => _ = maybe.Value);
        }

        [Test]
        public void ValueOrDefault_ReturnsValueWhenSome()
        {
            var maybe = Maybe<int>.Some(42);
            Assert.That(maybe.ValueOrDefault(0), Is.EqualTo(42));
        }

        [Test]
        public void ValueOrDefault_ReturnsDefaultWhenNone()
        {
            var maybe = Maybe<int>.None();
            Assert.That(maybe.ValueOrDefault(0), Is.EqualTo(0));
        }

        [Test]
        public void Map_TransformsValueWhenSome()
        {
            var maybe = Maybe<int>.Some(42);
            var result = maybe.Select(x => x.ToString());
            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value, Is.EqualTo("42"));
        }

        [Test]
        public void Map_ReturnsNoneWhenNone()
        {
            var maybe = Maybe<int>.None();
            var result = maybe.Select(x => x.ToString());
            Assert.That(result.HasValue, Is.False);
        }

        [Test]
        public void Bind_TransformsValueWhenSome()
        {
            var maybe = Maybe<int>.Some(42);
            var result = maybe.SelectMany(x => Maybe<string>.Some(x.ToString()));
            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value, Is.EqualTo("42"));
        }

        [Test]
        public void Bind_ReturnsNoneWhenNone()
        {
            var maybe = Maybe<int>.None();
            var result = maybe.SelectMany(x => Maybe<string>.Some(x.ToString()));
            Assert.That(result.HasValue, Is.False);
        }

        [Test]
        public void ImplicitConversion_CreatesValueWithContent()
        {
            Maybe<int> maybe = 42;
            Assert.That(maybe.HasValue, Is.True);
            Assert.That(maybe.Value, Is.EqualTo(42));
        }

        [Test]
        public void ToString_ReturnsCorrectRepresentationWhenSome()
        {
            var maybe = Maybe<int>.Some(42);
            Assert.That(maybe.ToString(), Is.EqualTo("Some(42)"));
        }

        [Test]
        public void ToString_ReturnsCorrectRepresentationWhenNone()
        {
            var maybe = Maybe<int>.None();
            Assert.That(maybe.ToString(), Is.EqualTo("None"));
        }
    }
}