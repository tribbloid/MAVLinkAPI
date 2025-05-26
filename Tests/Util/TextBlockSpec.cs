using MAVLinkAPI.Util.Text;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace MAVLinkAPI.Tests.Util
{
    [TestFixture]
    public class TextBlockSpec
    {
        [Test]
        public void ZipRight_WithEqualLines_ShouldCombineBlocksSideBySide()
        {
            // Arrange
            var block1 = new TextBlock("a\nb");
            var block2 = new TextBlock("c\nd");

            // Act
            var result = block1.ZipRight(block2);

            // Assert
            Assert.AreEqual("ac\nbd", result.ToString());
        }

        [Test]
        public void ZipRight_WithDifferentLines_ShouldPadAndZip()
        {
            // Arrange
            var block1 = new TextBlock("a\nb\nc");
            var block2 = new TextBlock("d\ne");

            // Act
            var result = block1.ZipRight(block2);

            // Assert
            Assert.AreEqual("ad\nbe\nc", result.ToString());
        }

        [Test]
        public void ZipRight_WithEmptyBlock_ShouldPadAndZip()
        {
            // Arrange
            var block1 = new TextBlock("a\nb");
            var block2 = new TextBlock("");

            // Act
            var result = block1.ZipRight(block2);

            // Assert
            Assert.AreEqual("a\nb", result.ToString());
        }

        [Test]
        public void ZipRight_TwoEmptyBlocks_ShouldResultInEmptyBlock()
        {
            // Arrange
            var block1 = new TextBlock("");
            var block2 = new TextBlock("");

            // Act
            var result = block1.ZipRight(block2);

            // Assert
            Assert.AreEqual("", result.ToString());
        }

        [Test]
        public void ZipRight_WithVaryingLineLengths_ShouldPadAndZip()
        {
            // Arrange
            var block1 = new TextBlock("a\nbb\nccc");
            var block2 = new TextBlock("d\ne\nf");

            // Act
            var result = block1.ZipRight(block2);

            // Assert
            Assert.AreEqual("a  d\nbb e\ncccf", result.ToString());
        }


        [Test]
        public void ZipRight_WithDifferentVaryingLineLengths_ShouldPadAndZip()
        {
            // Arrange
            var block1 = new TextBlock("a\nbb\nccc");
            var block2 = new TextBlock("d\ne\nf\ng");

            // Act
            var result = block1.ZipRight(block2);

            // Assert
            Assert.AreEqual("a  d\nbb e\ncccf\n   g", result.ToString());
        }

        [Test]
        public void ZipRight_WithVaryingLineLengthsAndEmptyBlock_ShouldPadAndZip()
        {
            // Arrange
            var block1 = new TextBlock("a\nbb\nccc");
            var block2 = new TextBlock("");

            // Act
            var result = block1.ZipRight(block2);

            // Assert
            Assert.AreEqual("a  \nbb \nccc", result.ToString());
        }

        [Test]
        public void PadLeft_WithDefaultPadding_ShouldAddPipeToEachLine()
        {
            // Arrange
            var block = new TextBlock("a\nb");

            // Act
            var result = block.PadLeft();

            // Assert
            Assert.AreEqual("|a\n|b", result.ToString());
        }

        [Test]
        public void PadLeft_WithDifferentFirstRowPadding_ShouldApplyCorrectly()
        {
            // Arrange
            var block = new TextBlock("a\nb");

            // Act
            var result = block.PadLeft("-> ", "   ");

            // Assert
            Assert.AreEqual("-> a\n   b", result.ToString());
        }
    }
}