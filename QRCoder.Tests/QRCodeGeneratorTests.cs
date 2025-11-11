using Xunit;

namespace QRCoder.Tests;

public class QRCodeGeneratorTests
{
    private static readonly string OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "out");

    public QRCodeGeneratorTests()
    {
        if (!Directory.Exists(OutputDirectory))
        {
            _ = Directory.CreateDirectory(OutputDirectory);
        }
    }

    [Fact]
    public void CreateQrCode_WithTrkidUrl_Version2_EccLevelM_ReturnsQRCodeData()
    {
        // Arrange
        var plainText = "HTTPS://TRKID.COM/Z/PPPPSSSSSSSSNNNNXX";
        //QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.M;
        //var requestedVersion = 2;
        var qrcode_hex = "FE0EBFC15ED06E938BB74735DBAA32EC12A507FAAAFE004C00AA728908B324D8A057E512933F62F2A340A84EBD5800BA43E8BBF2FC8076C57F906BB043B15BAC9FEDD3E842EB3CFB0428ABFEDE3C80";
        // Act
        QRCodeData qrCodeData = QRCodeGenerator.GenerateQrCode(plainText); //, eccLevel, requestedVersion: requestedVersion);

        // Assert
        Assert.NotNull(qrCodeData);
        Assert.Equal(2, qrCodeData.Version);

        var dataBytes = qrCodeData.GetRawData();
        var hexString = Convert.ToHexString(dataBytes);

        // some bad test behavior, writing files out for inspection
        var outputPath = Path.Combine(OutputDirectory, "qrcode_hex.txt");
        File.WriteAllText(outputPath, hexString);

        Assert.Equal(qrcode_hex, hexString);
    }

    [Fact]
    public void CreateQrCode_WithTrkidUrl_Version2_EccLevelM_ReturnsExpectedHexString()
    {
        var plainText = "HTTPS://TRKID.COM/Z/PPPPSSSSSSSSNNNNXX";
        //QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.M;
        //var requestedVersion = 2;

        // Act
        QRCodeData qrCodeData = QRCodeGenerator.GenerateQrCode(plainText); //, eccLevel, requestedVersion: requestedVersion);

        // Assert
        Assert.NotNull(qrCodeData);
        Assert.Equal(2, qrCodeData.Version);

        var dataBytes = qrCodeData.GetRawData();
        var qrhex = Convert.ToHexString(dataBytes);

        // Arrange
        //var qrhex = "FE0EBFC15ED06E938BB74735DBAA32EC12A507FAAAFE004C00AA728908B324D8A057E512933F62F2A340A84EBD5800BA43E8BBF2FC8076C57F906BB043B15BAC9FEDD3E842EB3CFB0428ABFEDE3C80";
        var cr = new CodeReader(qrhex);

        const int ModuleSize = 20;
        const int SafeZone = 2;
        const int MatrixSize = 25;

        var CanvasSize = (MatrixSize + SafeZone + SafeZone) * ModuleSize;

        var svg = true;
        var outputPath = Path.Combine(OutputDirectory, svg ? "qrcode.svg" : "qrcode.ps");

        using var writer = new StreamWriter(outputPath);
        if (svg)
        {
            writer.WriteLine("<svg version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" width=\"{0}\" height=\"{0}\">", CanvasSize);
        }
        else
        {
            writer.WriteLine("%!PS-Adobe EPSF-3.0");
            writer.WriteLine("%%BoundingBox:0 0 {0} {0}", CanvasSize);
        }


        for (var i = 0; i < MatrixSize; i++)
        {
            var RowTop = (SafeZone + i) * ModuleSize;
            for (var j = 0; j < MatrixSize; j++)
            {
                var ColLeft = (SafeZone + j) * ModuleSize;
                if (cr.NextBit())
                {
                    if (svg)
                    {
                        writer.WriteLine("<rect x=\"{0}\" y=\"{1}\" width=\"{2}\" height=\"{2}\"/>", ColLeft, RowTop, ModuleSize);
                    }
                    else
                    {
                        writer.Write("{0} {1} {2} {2} ", ColLeft, RowTop, ModuleSize);
                    }

                }

            }

            if (svg)
            {
                writer.WriteLine("");
            }
            else
            {
                writer.WriteLine("rectfill");
            }
        }

        if (svg)
        {
            writer.WriteLine("</svg>");
        }
        else
        {
            writer.WriteLine("%%EOF");
        }

        // debug dump bitmatrix with original code
        //var xcode = qrw.encode(l);
        //_outputHelper.WriteLine(xcode.ToString());
    }


    private sealed class CodeReader(string code)
    {
        private int bitCount;
        private readonly string hexString = code;

        public bool NextBit()
        {
            var stringIndex = bitCount / 4;
            var bitIndex = bitCount % 4;
            var hexByte = "0123456789ABCDEF".IndexOf(hexString[stringIndex]);
            bitCount++;
            return (hexByte & (1 << (3 - bitIndex))) > 0;
        }
    }
}
