using Configuration;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Plugins;

public class WebRetrieverPlugin(IOptions<PluginOptions> pluginOptions)
{
    
    [KernelFunction, Description("Searches the web for answering user questions.")]
    public async Task<string> RetrieveAsync([Description("User's query"), Required] string question, Kernel kernel)
    {
        var rewriter = kernel.Plugins["QueryRewritePlugin"];
        
        var searchQuery = await kernel.InvokeAsync(rewriter["Rewrite"],
            new()
            {
                { "question", question }
            });

        var searchEngine = new WebSearchEnginePlugin(new BingConnector(pluginOptions.Value.BingApiKey));
        var searchResults = await searchEngine.SearchAsync(searchQuery.ToString());
        
        var rag = kernel.Plugins["Prompts"];

        var llmResult = await kernel.InvokeAsync(rag["BasicRAG"],
            new() {
                { "question", question },
                { "context", searchResults }
            }
        );

        return llmResult.ToString();
    }
}
