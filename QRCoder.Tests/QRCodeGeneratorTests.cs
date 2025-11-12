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
        // test call and expected result
        var plainText = "HTTPS://TRKID.COM/Z/PPPP6915189C0001XX";
        var qrcode_hex = "FE58BFC149D06E9E2BB74CB5DBA932EC101507FAAAFE001800AA588944C624C4F2D7EE8213386292A14AB84E1E4000B383E8AB50FC8077C57F98EBB048315BAEBFEDD1A842EB2CFB0428ABFE8A3C80";


        var hexString = QRCodeGenerator.GenerateTrkidQrCode(plainText);

        // some bad test behavior, writing files out for inspection
        var outputPath = Path.Combine(OutputDirectory, "qrcode_hex.txt");
        File.WriteAllText(outputPath, hexString);

        Assert.Equal(qrcode_hex, hexString);
    }

    [Fact]
    public void CreateQrCode_WithTrkidUrl_Version2_EccLevelM_ReturnsExpectedHexString()
    {
        //builds an SVG of the qrcode to output directory for visual inspection
        var plainText = "HTTPS://TRKID.COM/Z/PPPP6915189C0001XX";
        var qrcode_hex = "FE58BFC149D06E9E2BB74CB5DBA932EC101507FAAAFE001800AA588944C624C4F2D7EE8213386292A14AB84E1E4000B383E8AB50FC8077C57F98EBB048315BAEBFEDD1A842EB2CFB0428ABFE8A3C80";


        var hexString = QRCodeGenerator.GenerateTrkidQrCode(plainText);
        Assert.Equal(qrcode_hex, hexString);

        var cr = new CodeReader(hexString);

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
