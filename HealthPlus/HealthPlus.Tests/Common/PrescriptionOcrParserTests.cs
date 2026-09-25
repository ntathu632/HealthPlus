using HealthPlus.Application.Common;
using Xunit;

namespace HealthPlus.Tests.Common;

public class PrescriptionOcrParserTests
{
    [Fact]
    public void Parse_NullOrEmpty_ReturnsEmptyList()
    {
        Assert.Empty(PrescriptionOcrParser.Parse(null));
        Assert.Empty(PrescriptionOcrParser.Parse(""));
        Assert.Empty(PrescriptionOcrParser.Parse("   "));
    }

    [Fact]
    public void Parse_TypicalMultilinePrescription_ExtractsAllItemsCorrectly()
    {
        var raw = "Paracetamol 500mg x2/ngay 5 ngay\n" +
                   "Amoxicillin 250mg x3/ngay 7 ngay\n" +
                   "Vitamin C 1000mg x1/ngay 10 ngay";

        var items = PrescriptionOcrParser.Parse(raw);

        Assert.Equal(3, items.Count);

        Assert.Equal("Paracetamol", items[0].MedicineName);
        Assert.Equal("500mg", items[0].Dosage);
        Assert.Equal(2, items[0].FrequencyPerDay);
        Assert.Equal(5, items[0].DurationDays);

        Assert.Equal("Amoxicillin", items[1].MedicineName);
        Assert.Equal("250mg", items[1].Dosage);
        Assert.Equal(3, items[1].FrequencyPerDay);
        Assert.Equal(7, items[1].DurationDays);

        Assert.Equal("Vitamin C", items[2].MedicineName);
        Assert.Equal("1000mg", items[2].Dosage);
        Assert.Equal(1, items[2].FrequencyPerDay);
        Assert.Equal(10, items[2].DurationDays);
    }

    // Regression test for a real bug caught during manual end-to-end testing:
    // the "xN/ngày" shorthand used to leave a dangling "/ngay" stuck onto the medicine name.
    [Theory]
    [InlineData("Paracetamol 500mg x2/ngay 5 ngay", "Paracetamol")]
    [InlineData("Vitamin C 1000mg x1/ngay 10 ngay", "Vitamin C")]
    public void Parse_XNSlashNgayShorthand_DoesNotLeakIntoMedicineName(string line, string expectedName)
    {
        var items = PrescriptionOcrParser.Parse(line);

        Assert.Single(items);
        Assert.Equal(expectedName, items[0].MedicineName);
        Assert.DoesNotContain("ngay", items[0].MedicineName, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/", items[0].MedicineName);
    }

    [Theory]
    [InlineData("Họ tên: Nguyễn Văn A")]
    [InlineData("Ngày sinh: 01/01/1990")]
    [InlineData("Địa chỉ: Hà Nội")]
    [InlineData("Chẩn đoán: Viêm họng")]
    [InlineData("Bác sĩ: Trần Thị B")]
    [InlineData("ĐƠN THUỐC")]
    public void Parse_HeaderAndMetadataLines_AreSkipped(string line)
    {
        var items = PrescriptionOcrParser.Parse(line);
        Assert.Empty(items);
    }

    [Theory]
    [InlineData("12345")]      // no letters at all
    [InlineData("a")]          // too short
    [InlineData("")]
    public void Parse_LinesWithoutMeaningfulContent_AreSkipped(string line)
    {
        var items = PrescriptionOcrParser.Parse(line);
        Assert.Empty(items);
    }

    [Fact]
    public void Parse_LineWithNoDosageOrFrequency_StillExtractsMedicineName()
    {
        var items = PrescriptionOcrParser.Parse("Ibuprofen");

        Assert.Single(items);
        Assert.Equal("Ibuprofen", items[0].MedicineName);
        Assert.Null(items[0].Dosage);
        Assert.Null(items[0].FrequencyPerDay);
        Assert.Null(items[0].DurationDays);
    }

    [Theory]
    [InlineData("Panadol 500mg")]
    [InlineData("Panadol 2ml")]
    [InlineData("Panadol 1 viên")]
    [InlineData("Panadol 1 gói")]
    public void Parse_RecognizesCommonDosageUnits(string line)
    {
        var items = PrescriptionOcrParser.Parse(line);

        Assert.Single(items);
        Assert.NotNull(items[0].Dosage);
    }

    [Fact]
    public void Parse_AlternateFrequencyPhrasing_LanNgayNgayFormat()
    {
        var items = PrescriptionOcrParser.Parse("Panadol 500mg 2 lần/ngày");

        Assert.Single(items);
        Assert.Equal(2, items[0].FrequencyPerDay);
    }
}
