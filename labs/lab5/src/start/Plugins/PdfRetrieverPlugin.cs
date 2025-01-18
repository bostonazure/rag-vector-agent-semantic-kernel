using Memory;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Plugins;

public class PdfRetrieverPlugin(MemoryStore memoryStore)
{
    
    [KernelFunction, Description("Searches for company information.")]
    public async Task<string> RetrieveAsync([Description("User's message"), Required] string question, Kernel kernel)
    {
        var rewriter = kernel.Plugins["QueryRewritePlugin"];

        var searchQuery = await kernel.InvokeAsync(rewriter["Rewrite"],
            new()
            {
                { "question", question }
            });

        var searchResults = await memoryStore.SearchAsync(searchQuery.ToString());

        var rag = kernel.Plugins["Prompts"];

        var llmResult = await kernel.InvokeAsync(rag["BasicRAG"],
            new() {
                { "question", question },
                { "context", JsonSerializer.Serialize(searchResults) }
            }
        );

        return llmResult.ToString();
    }
}
