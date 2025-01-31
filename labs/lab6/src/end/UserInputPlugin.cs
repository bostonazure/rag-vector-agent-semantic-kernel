using Microsoft.SemanticKernel;
using System.ComponentModel;



public class UserInputsProxy
{
    [KernelFunction("GetUserImput")]
    [Description("you present the information to the user and get the user input.")]
    public async Task<string> GetUserFlighandHotelConfirmationAsync(string Information)
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("User Confirmation is required for the following ");
        Console.WriteLine();

        Console.WriteLine(Information);
        Console.WriteLine();
        
        Console.WriteLine("Please let me know your comments:");
        string userInput = Console.ReadLine() ?? "No comments";
        return await Task.FromResult(userInput);
    }
}