using Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Plugins;

public class WebRetrieverPlugin(IOptions<PluginOptions> pluginOptions, ILoggerFactory loggerFactory)
{
    
    [KernelFunction, Description("Searches the web for answering user questions.")]
    public async Task<string> RetrieveAsync([Description("User's query"), Required] string question, Kernel kernel)
    {
        var searchEngine = new WebSearchEnginePlugin(new BingConnector(pluginOptions.Value.BingApiKey, loggerFactory: loggerFactory));
        var searchResults = await searchEngine.SearchAsync(question);
        
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
