using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Plugins;
public class QueryRewritePlugin
{
    
    [KernelFunction, Description("Rewrites the user's question for calling a web search.")]
    public async Task<string> RewriteAsync([Description("User's query"), Required] string question, Kernel kernel)
    {
        var prompts = kernel.Plugins["Prompts"];
        
        var result = await kernel.InvokeAsync(prompts["RewriteQuery"],
            new() {
                { "question", question },
            }
        );

        return result.ToString();
    }
}
