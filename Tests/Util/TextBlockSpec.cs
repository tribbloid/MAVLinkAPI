using MAVLinkAPI.Util.Text;
using NUnit.Framework;
using System;

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
            Assert.AreEqual("ac\nbd".normaliseLineBreak(), result.ToString().normaliseLineBreak());
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
            Assert.AreEqual("ad\nbe\nc".normaliseLineBreak(), result.ToString().normaliseLineBreak());
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
            Assert.AreEqual("a\nb".normaliseLineBreak(), result.ToString().normaliseLineBreak());
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
            Assert.AreEqual("a  d\nbb e\ncccf".normaliseLineBreak(), result.ToString().normaliseLineBreak());
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
            Assert.AreEqual("a  d\nbb e\ncccf\n   g".normaliseLineBreak(), result.ToString().normaliseLineBreak());
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
            Assert.AreEqual("a  \nbb \nccc".normaliseLineBreak(), result.ToString().normaliseLineBreak());
        }

        [Test]
        public void PadLeft_WithDefaultPadding_ShouldAddPipeToEachLine()
        {
            // Arrange
            var block = new TextBlock("a\nb");

            // Act
            var result = block.PadLeft();

            // Assert
            Assert.AreEqual("|a\n|b".normaliseLineBreak(), result.ToString().normaliseLineBreak());
        }

        [Test]
        public void PadLeft_WithDifferentFirstRowPadding_ShouldApplyCorrectly()
        {
            // Arrange
            var block = new TextBlock("a\nb");

            // Act
            var result = block.PadLeft("-> ", "   ");

            // Assert
            Assert.AreEqual("-> a\n   b".normaliseLineBreak(), result.ToString().normaliseLineBreak());
        }

        [Test]
        public void NormaliseLineBreak_WithDosEndings_ShouldConvertToSystemDefault()
        {
            // Arrange
            var input = "line1\r\nline2";
            var expected = "line1\nline2";

            // Act
            var result = input.normaliseLineBreak();

            // Assert
            Assert.AreEqual(expected, result.Replace(Environment.NewLine, "\n"));
        }

        [Test]
        public void NormaliseLineBreak_WithUnixEndings_ShouldConvertToSystemDefault()
        {
            // Arrange
            var input = "line1\nline2";
            var expected = "line1\nline2";

            // Act
            var result = input.normaliseLineBreak();

            // Assert
            Assert.AreEqual(expected, result.Replace(Environment.NewLine, "\n"));
        }

        [Test]
        public void NormaliseLineBreak_WithMixedEndings_ShouldConvertToSystemDefault()
        {
            // Arrange
            var input = "line1\r\nline2\nline3";
            var expected = "line1\nline2\nline3";

            // Act
            var result = input.normaliseLineBreak();

            // Assert
            Assert.AreEqual(expected, result.Replace(Environment.NewLine, "\n"));
        }

        [Test]
        public void NormaliseLineBreak_WithNullInput_ShouldReturnNull()
        {
            // Arrange
            string input = null;

            // Act
            var result = input.normaliseLineBreak();

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void NormaliseLineBreak_WithEmptyInput_ShouldReturnEmpty()
        {
            // Arrange
            var input = "";

            // Act
            var result = input.normaliseLineBreak();

            // Assert
            Assert.AreEqual("", result);
        }
    }
}