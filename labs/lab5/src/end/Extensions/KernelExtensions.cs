using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;

namespace Extensions;
internal static class KernelExtensions
{
    public static KernelPlugin ImportPluginFromDirectory(this Kernel kernel, string pluginDirectory, string? pluginName = null, IPromptTemplateFactory? promptTemplateFactory = null)
    {
        KernelPlugin plugin = CreatePluginFromPromptDirectory(kernel, pluginDirectory, pluginName, promptTemplateFactory);
        kernel.Plugins.Add(plugin);
        return plugin;
    }

    public static KernelPlugin CreatePluginFromPromptDirectory(this Kernel kernel, string pluginDirectory, string? pluginName = null, IPromptTemplateFactory? promptTemplateFactory = null)
    {
        var loggerFactory = kernel.Services.GetRequiredService<ILoggerFactory>() ?? NullLoggerFactory.Instance;

        var factory = promptTemplateFactory ?? new KernelPromptTemplateFactory(loggerFactory);

        pluginName ??= new DirectoryInfo(pluginDirectory).Name;

        var functions = new List<KernelFunction>();

        ILogger logger = loggerFactory.CreateLogger(typeof(Kernel)) ?? NullLogger.Instance;
        foreach (string fileInDirectory in Directory.EnumerateFiles(pluginDirectory))
        {
            if (Path.GetExtension(fileInDirectory) == ".yaml")
            {
                var kernelFunction = kernel.CreateFunctionFromPromptYaml(File.ReadAllText(fileInDirectory), factory);
                functions.Add(kernelFunction);
            }
        }
        return KernelPluginFactory.CreateFromFunctions(pluginName, null, functions);
    }
}
