using Xunit;

namespace QRCoder.Tests;

public class QRCodeGeneratorTests
{
    [Fact]
    public void CreateQrCode_WithTrkidUrl_Version2_EccLevelM_ReturnsQRCodeData()
    {
        // Arrange
        var plainText = "HTTPS://TRKID.COM/Z/PPPPSSSSSSSSNNNNXX";
        //QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.M;
        //var requestedVersion = 2;

        // Act
        QRCodeData qrCodeData = QRCodeGenerator.GenerateQrCode(plainText); //, eccLevel, requestedVersion: requestedVersion);

        // Assert
        Assert.NotNull(qrCodeData);
        Assert.Equal(2, qrCodeData.Version);

        var dataBytes = qrCodeData.GetRawData(QRCodeData.Compression.Uncompressed);
        var hexString = Convert.ToHexString(dataBytes);
        var expectedLength = 284; // Expected length of the byte array for this specific QR code
        Assert.Equal(expectedLength, hexString.Length);
        Console.WriteLine(hexString);
    }
}
