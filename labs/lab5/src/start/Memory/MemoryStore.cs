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
    private const string TABLE_NAME = "jh-memory";

    public async Task<IEnumerable<string>> SearchAsync(string query)
    {
        var results = new List<string>();

        var searchItems = semanticTextMemory.SearchAsync(TABLE_NAME, query, 3);
        await foreach (var item in searchItems)
        {
            results.Add(item.Metadata.Text);
        }
        return results;
    }

    public async Task PopulateAsync(string assetsDir)
    {
        var chunkCount = 0;
        var tokenizer = TiktokenTokenizer.CreateForModel("gpt-4o");

        foreach (var file in Directory.GetFiles(assetsDir, "*.pdf"))
        {
            Console.WriteLine($"Generating chunks for {file}...");
                        
            var docId = Path.GetFileNameWithoutExtension(file);

            // See if file already ingested
            if (await semanticTextMemory.TableExistsAsync(TABLE_NAME) 
                && await semanticTextMemory.RecordExistsAsync(TABLE_NAME, GenerateKey(docId, 1, 0)))
            {
                Console.WriteLine($"Document {docId} already exists.");
                continue;
            }
            
            using var pdf = PdfDocument.Open(file);
            foreach (var page in pdf.GetPages())
            {
                var pageText = GetPageText(page);
                var paragraphs = TextChunker.SplitPlainTextParagraphs([pageText], 500, 100, null, text => tokenizer.CountTokens(text));
                
                for (int i = 0; i < paragraphs.Count; i++)
                {
                    var paragraph = paragraphs[i];

                    var textToEmbed = paragraph;
                    var title = $"Document {docId}, Page {page.Number}, Chunk {i}";

                    await semanticTextMemory.SaveInformationAsync(TABLE_NAME, textToEmbed, GenerateKey(docId, page.Number, i), title);
                    chunkCount++;

                    Console.WriteLine($"Saved {title}...");
                }
            }
        }

        if (chunkCount > 0)
        {
            Console.WriteLine($"Generated {chunkCount}.");
        }
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
