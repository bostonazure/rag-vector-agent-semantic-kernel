using Microsoft.ML.Tokenizers;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace Memory;

public class MemoryStore(ISemanticTextMemory semanticTextMemory)
{
    private const string TABLE_NAME = ""; // TODO: Set the table name

    public async Task<IEnumerable<string>> SearchAsync(string query)
    {
        var results = new List<string>();

        // TODO: Implement search functionality 

        return results;
    }

    public async Task PopulateAsync(string assetsDir)
    {
        // TODO: Implement the logic to load pdfs, parse, split into chunks and save into the memory store method
    }

    private static string GenerateKey(string docId, int pageNumber, int chunkNumber)
    {
        return $"{docId}-{pageNumber}-{chunkNumber}";
    }

    private static string GetPageText(Page pdfPage)
    {
        var letters = pdfPage.Letters;
        var words = NearestNeighbourWordExtractor.Instance.GetWords(letters);
        var textBlocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);
        return string.Join(Environment.NewLine + Environment.NewLine,
            textBlocks.Select(t => t.Text.ReplaceLineEndings(" ")));
    }

}
