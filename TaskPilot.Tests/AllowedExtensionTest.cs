using Microsoft.AspNetCore.Http;
using Moq;
using TaskPilot.Application.Common.Utility.CustomValidator;

namespace TaskPilot.Tests
{
    [TestFixture]
    public class AllowedExtensionsAttributeTests
    {
        [TestCase("test.jpg")]
        [TestCase("test.png")]
        [TestCase("test.jpeg")]
        public void IsValid_ValidExtension_ReturnsSuccess(string fileName)
        {
            // Arrange
            var allowedExtensions = new string[] { ".jpg", ".png", ".jpeg" };
            var attribute = new AllowedExtensionsAttribute(allowedExtensions);

            var mockFormFile = new Mock<IFormFile>();
            mockFormFile.Setup(f => f.FileName).Returns(fileName);

            // Act
            var result = attribute.IsValid(mockFormFile.Object);

            // Assert
            Assert.IsTrue(result);
        }

        [TestCase("test.pdf")]
        [TestCase("test.csv")]
        [TestCase("test.doc")]
        public void IsValid_InValidExtension_ReturnsError(string fileName)
        {
            // Arrange
            var allowedExtensions = new string[] { ".jpg", ".png", ".jpeg" };
            var attribute = new AllowedExtensionsAttribute(allowedExtensions);

            var mockFormFile = new Mock<IFormFile>();
            mockFormFile.Setup(f => f.FileName).Returns(fileName);

            // Act
            var result = attribute.IsValid(mockFormFile.Object);

            // Assert
            Assert.IsFalse(result);
        }


    }
}
