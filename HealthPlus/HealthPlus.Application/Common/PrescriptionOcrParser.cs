using System.Text.RegularExpressions;

namespace HealthPlus.Application.Common;

public class ParsedPrescriptionItem
{
    public string MedicineName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public int? FrequencyPerDay { get; set; }
    public int? DurationDays { get; set; }
}

/// <summary>
/// Best-effort: tách mỗi dòng OCR thành 1 dòng thuốc bằng regex, không phải NLP thật.
/// Hoạt động tốt với đơn thuốc in/đánh máy; đơn viết tay OCR ra sẽ kém chính xác hơn nhiều.
/// Kết quả luôn cần người dùng xem lại (IsConfirmed=false) qua UI thêm/sửa/xoá đã có sẵn.
/// </summary>
public static class PrescriptionOcrParser
{
    private static readonly string[] SkipKeywords =
    {
        "đơn thuốc", "họ tên", "họ và tên", "ngày sinh", "địa chỉ", "chẩn đoán",
        "bác sĩ", "bệnh viện", "phòng khám", "ký tên", "giới tính", "tuổi",
        "số điện thoại", "mã bệnh nhân", "khoa", "ngày khám", "cân nặng",
    };

    private static readonly Regex FrequencyRegex =
        new(@"(\d+)\s*(?:lần|viên)\s*/\s*ng[àa]y|x\s*(\d+)\s*(?:/\s*ng[àa]y|\s*lần)?", RegexOptions.IgnoreCase);

    private static readonly Regex DurationRegex =
        new(@"(\d+)\s*ng[àa]y", RegexOptions.IgnoreCase);

    private static readonly Regex DosageRegex =
        new(@"\d+([.,]\d+)?\s*(mg|ml|g|mcg|viên|gói|ống)\b", RegexOptions.IgnoreCase);

    private static readonly Regex TrailingPunctuationRegex = new(@"[-–,;:.\s]+$");

    public static List<ParsedPrescriptionItem> Parse(string? rawText)
    {
        var items = new List<ParsedPrescriptionItem>();
        if (string.IsNullOrWhiteSpace(rawText)) return items;

        var lines = rawText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            if (line.Length < 3 || !line.Any(char.IsLetter)) continue;

            var lower = line.ToLowerInvariant();
            if (SkipKeywords.Any(lower.Contains)) continue;

            var durationMatch = DurationRegex.Match(line);
            var frequencyMatch = FrequencyRegex.Match(line);
            var dosageMatch = DosageRegex.Match(line);

            var name = line;
            foreach (var m in new[] { durationMatch, frequencyMatch, dosageMatch })
                if (m.Success) name = name.Replace(m.Value, " ");

            name = Regex.Replace(name, @"\s{2,}", " ");
            name = TrailingPunctuationRegex.Replace(name, "").Trim(' ', '-', '–', '.');
            if (name.Length < 2) continue;

            items.Add(new ParsedPrescriptionItem
            {
                MedicineName = name,
                Dosage = dosageMatch.Success ? dosageMatch.Value.Trim() : null,
                FrequencyPerDay = frequencyMatch.Success
                    ? int.Parse(frequencyMatch.Groups[1].Success ? frequencyMatch.Groups[1].Value : frequencyMatch.Groups[2].Value)
                    : null,
                DurationDays = durationMatch.Success ? int.Parse(durationMatch.Groups[1].Value) : null,
            });
        }

        return items;
    }
}
