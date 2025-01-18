using Microsoft.SemanticKernel.Memory;

namespace Memory;
public static class MemoryExtensions
{
    public static async Task<bool> TableExistsAsync(this ISemanticTextMemory semanticTextMemory, string tableName)
    {
        var collection = await semanticTextMemory.GetCollectionsAsync();
        if (collection != null && collection.Any(n => n == tableName))
        {
            return true;
        }
        return false;
    }

    public static async Task<bool> RecordExistsAsync(this ISemanticTextMemory semanticTextMemory, string tableName, string key)
    {
        var record = await semanticTextMemory.GetAsync(tableName, key);
        return record != null;
    }
}
