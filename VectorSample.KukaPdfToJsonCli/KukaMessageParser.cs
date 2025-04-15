using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;
using System.Text.RegularExpressions;

namespace VectorSample.KukaPdfToJsonCli
{
    public class KukaMessageParser
    {
        public List<KukaMessage> Parse(string pdfPath)
        {
            var fullText = ExtractTextFromPdf(pdfPath, 6); // Start from page 6
            var blocks = Regex.Split(fullText, @"(?=^\d{1,5}\s*)", RegexOptions.Multiline);
            var result = new List<KukaMessage>();

            foreach (var block in blocks)
            {
                var lines = block
                    .Split('\n')
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .ToList();

                if (lines.Count == 0) continue;

                if (!int.TryParse(lines[0], out int code)) continue;

                var message = new KukaMessage { Code = code };

                // Parse title
                int i = 1;
                var sbTitle = new StringBuilder();
                while (i < lines.Count && !Regex.IsMatch(lines[i], @"^(Cause|Effect|Remedy)\b", RegexOptions.IgnoreCase))
                {
                    if (sbTitle.Length > 0) sbTitle.Append(" ");
                    sbTitle.Append(lines[i]);
                    i++;
                }
                message.Title = sbTitle.ToString().Trim();

                // Parse Cause/Effect/Remedy
                string currentField = null;
                var fieldBuffer = new StringBuilder();

                while (i < lines.Count)
                {
                    var line = lines[i];
                    var match = Regex.Match(line, @"^(Cause|Effect|Remedy)\b", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        if (!string.IsNullOrEmpty(currentField))
                        {
                            AssignField(message, currentField, fieldBuffer.ToString().Trim());
                            fieldBuffer.Clear();
                        }

                        currentField = match.Value;
                        var remainder = line.Substring(match.Value.Length).Trim();
                        if (!string.IsNullOrWhiteSpace(remainder))
                            fieldBuffer.AppendLine(remainder);
                    }
                    else if (!string.IsNullOrEmpty(currentField))
                    {
                        fieldBuffer.AppendLine(line);
                    }

                    i++;
                }

                if (!string.IsNullOrEmpty(currentField))
                {
                    AssignField(message, currentField, fieldBuffer.ToString().Trim());
                }

                result.Add(message);
            }

            return result;
        }

        private string ExtractTextFromPdf(string path, int startPage = 1)
        {
            var sb = new StringBuilder();
            var reader = new PdfReader(path);
            var pdf = new PdfDocument(reader);

            for (int i = startPage; i <= pdf.GetNumberOfPages(); i++)
            {
                var text = PdfTextExtractor.GetTextFromPage(pdf.GetPage(i), new SimpleTextExtractionStrategy());

                var lines = text.Split('\n')
                    .Select(l => l.Trim())
                    .Where(l =>
                        !string.IsNullOrWhiteSpace(l) &&
                        !Regex.IsMatch(l, @"^\d+\s+Systemmeldungen.*$") &&
                        !Regex.IsMatch(l, @"^System Messages$") &&
                        !Regex.IsMatch(l, @"^\d+\s+of\s+\d+$") &&
                        !Regex.IsMatch(l, @"^KUKA System Software.*$") &&
                        !Regex.IsMatch(l, @"^Issued:.*Version:.*$"));

                foreach (var line in lines)
                    sb.AppendLine(line);
            }

            pdf.Close();
            return sb.ToString();
        }

        private void AssignField(KukaMessage message, string field, string value)
        {
            switch (field)
            {
                case "Cause":
                    message.Cause = value;
                    break;
                case "Effect":
                    message.Effect = value;
                    break;
                case "Remedy":
                    message.Remedy = value;
                    break;
            }
        }
    }
}
