using System;
using MAVLinkAPI.Ext;
using NUnit.Framework;

// Assuming your BuildList class is in this namespace

namespace MAVLinkAPI.Tests.Ext
{
    [TestFixture]
    public class ListOfSpec
    {
        [Test]
        public void ListFromTuple_TwoInts_ReturnsListOfInts()
        {
            var tuple = Tuple.Create(1, 2);
            var result = ListOf<int>.FromTuple(tuple);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(2, result[1]);
        }

        [Test]
        public void ListFromTuple_TwoStrings_ReturnsListOfStrings()
        {
            var tuple = Tuple.Create("hello", "world");
            var result = ListOf<string>.FromTuple(tuple);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("hello", result[0]);
            Assert.AreEqual("world", result[1]);
        }

        [Test]
        public void ListFromTuple_IntAndString_ReturnsListOfObjects()
        {
            var tuple = Tuple.Create(123, "test");
            var result = ListOf<object>.FromTuple(tuple);
            Assert.AreEqual(2, result.Count);
            Assert.IsInstanceOf<int>(result[0]);
            Assert.AreEqual(123, result[0]);
            Assert.IsInstanceOf<string>(result[1]);
            Assert.AreEqual("test", result[1]);
        }

        [Test]
        public void ListFromTuple_IntAndString_RequestIntList_ReturnsOnlyInts()
        {
            var tuple = Tuple.Create(456, "cannot_convert");
            var result = ListOf<int>.FromTuple(tuple);
            Assert.AreEqual(1, result.Count); // Only the int should be added
            Assert.AreEqual(456, result[0]);
        }

        [Test]
        public void ListFromTuple_ThreeDoubles_ReturnsListOfDoubles()
        {
            var tuple = Tuple.Create(1.1, 2.2, 3.3);
            var result = ListOf<double>.FromTuple(tuple);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1.1, result[0]);
            Assert.AreEqual(2.2, result[1]);
            Assert.AreEqual(3.3, result[2]);
        }

        [Test]
        public void ListFromTuple_MixedToObjects_ThreeElements()
        {
            var tuple = Tuple.Create(1, "text", 2.5);
            var result = ListOf<object>.FromTuple(tuple);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual("text", result[1]);
            Assert.AreEqual(2.5, result[2]);
        }

        [Test]
        public void ListFromTuple_MixedToInt_ThreeElements_ReturnsOnlyInts()
        {
            var tuple = Tuple.Create(10, "ignore", 20);
            var result = ListOf<int>.FromTuple(tuple);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(10, result[0]);
            Assert.AreEqual(20, result[1]);
        }

        [Test]
        public void ListFromTuple_FourBools_ReturnsListOfBools()
        {
            var tuple = Tuple.Create(true, false, true, false);
            var result = ListOf<bool>.FromTuple(tuple);
            Assert.AreEqual(4, result.Count);
            Assert.IsTrue(result[0]);
            Assert.IsFalse(result[1]);
            Assert.IsTrue(result[2]);
            Assert.IsFalse(result[3]);
        }

        [Test]
        public void ListFromTuple_MixedToObjects_FourElements()
        {
            var tuple = Tuple.Create('a', 99, "mixed bag", 3.14);
            var result = ListOf<object>.FromTuple(tuple);
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual('a', result[0]);
            Assert.AreEqual(99, result[1]);
            Assert.AreEqual("mixed bag", result[2]);
            Assert.AreEqual(3.14, result[3]);
        }

        [Test]
        public void ListFromTuple_AllDifferentTypes_ToCommonBase_AnimalExample()
        {
            // Test case for converting a tuple of different types of elements
            var dog = new Dog { Name = "Buddy" };
            var cat = new Cat { Name = "Whiskers" };
            var bird = new Bird { Name = "Tweety" };

            var tuple = Tuple.Create(dog, cat, bird);
            var result = ListOf<Animal>.FromTuple(tuple);

            Assert.AreEqual(3, result.Count);
            Assert.IsInstanceOf<Dog>(result[0]);
            Assert.AreEqual("Buddy", ((Dog)result[0]).Name);
            Assert.IsInstanceOf<Cat>(result[1]);
            Assert.AreEqual("Whiskers", ((Cat)result[1]).Name);
            Assert.IsInstanceOf<Bird>(result[2]);
            Assert.AreEqual("Tweety", ((Bird)result[2]).Name);
        }

        // Helper classes for the Animal test case
        public abstract class Animal
        {
            public string Name { get; set; }
        }

        public class Dog : Animal
        {
        }

        public class Cat : Animal
        {
        }

        public class Bird : Animal
        {
        }

        [Test]
        public void ListFromTuple_MixedToObjects_FiveElements()
        {
            var tuple = Tuple.Create(1, "two", 3.0, true, 'F');
            var result = ListOf<object>.FromTuple(tuple);
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual("two", result[1]);
            Assert.AreEqual(3.0, result[2]);
            Assert.AreEqual(true, result[3]);
            Assert.AreEqual('F', result[4]);
        }

        [Test]
        public void ListFromTuple_MixedToObjects_SixElements()
        {
            var tuple = Tuple.Create(1.0f, "two", 3, 'F', false, 6.0);
            var result = ListOf<object>.FromTuple(tuple);
            Assert.AreEqual(6, result.Count);
            Assert.AreEqual(1.0f, result[0]);
            Assert.AreEqual("two", result[1]);
            Assert.AreEqual(3, result[2]);
            Assert.AreEqual('F', result[3]);
            Assert.AreEqual(false, result[4]);
            Assert.AreEqual(6.0, result[5]);
        }

        [Test]
        public void ListFromTuple_MixedToObjects_SevenElements()
        {
            var tuple = Tuple.Create("one", 2, 3.0, 'F', true, 6.0f, "seven");
            var result = ListOf<object>.FromTuple(tuple);
            Assert.AreEqual(7, result.Count);
            Assert.AreEqual("one", result[0]);
            Assert.AreEqual(2, result[1]);
            Assert.AreEqual(3.0, result[2]);
            Assert.AreEqual('F', result[3]);
            Assert.AreEqual(true, result[4]);
            Assert.AreEqual(6.0f, result[5]);
            Assert.AreEqual("seven", result[6]);
        }

        [Test]
        public void ListFromTuple_MixedToInt_SevenElements_ReturnsOnlyInts()
        {
            var tuple = Tuple.Create("one", 2, 3.0, 4, true, 6.0f, 7);
            var result = ListOf<int>.FromTuple(tuple);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(4, result[1]);
            Assert.AreEqual(7, result[2]);
        }
    }
}