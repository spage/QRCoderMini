using Xunit;

namespace QRCoder.Tests;

public class QRCodeGeneratorTests
{
    [Fact]
    public void CreateQrCode_WithTrkidUrl_Version2_EccLevelM_ReturnsQRCodeData()
    {
        // Arrange
        var plainText = "HTTPS://TRKID.COM/Z/PPPPSSSSSSSSNNNNXX";
        var eccLevel = QRCodeGenerator.ECCLevel.M;
        var requestedVersion = 2;

        // Act
        var qrCodeData = QRCodeGenerator.GenerateQrCode(plainText, eccLevel, requestedVersion: requestedVersion);

        // Assert
        Assert.NotNull(qrCodeData);
        Assert.Equal(2, qrCodeData.Version);
    }
}
