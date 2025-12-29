using System.Text;

/// <summary>
/// Handles Tab auto-completion for shell commands.
/// Provides custom input reading with support for:
/// - Tab completion for built-in commands
/// - Backspace handling
/// - Character-by-character input processing
/// - Up/Down arrow history navigation
/// </summary>
public class AutoCompletion
{
    /// <summary>
    /// Reads user input character by character with Tab auto-completion support.
    /// Uses Console.ReadKey(intercept: true) to capture keys without displaying them,
    /// giving full control over what appears on screen.
    /// </summary>
    /// <returns>The complete input string when Enter is pressed</returns>
    public static string ReadInput()
    {
        // Buffer to accumulate typed characters
        StringBuilder stringBuilder = new StringBuilder();
        
        // Get list of built-in commands for auto-completion
        var builtins = Builtins.BuiltinCommands;

        bool lastKeyWasTab = false;
        
        // Reset history navigation state for new input
        HistoryNavigation.Reset();

        
        // Main input loop - runs until Enter is pressed
        while (true)
        { 
            // Read a key without displaying it (intercept: true)
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

            // 1. Handle ENTER key - submit the input
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();              // Move to new line
                return stringBuilder.ToString();  // Return the accumulated input
            }
            
            // 2. Handle UP ARROW - navigate to previous command in history
            else if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                HistoryNavigation.NavigateUp(stringBuilder);
                lastKeyWasTab = false;
            }
            
            // 3. Handle DOWN ARROW - navigate to next command in history
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                HistoryNavigation.NavigateDown(stringBuilder);
                lastKeyWasTab = false;
            }
            
            // 4. Handle TAB key - auto-complete commands
            else if (keyInfo.Key == ConsoleKey.Tab)
            {
                // Get what the user has typed so far
                string currentInput = stringBuilder.ToString();

                // Find all built-in commands that start with the current input
                var matches = builtins.Where(b => b.StartsWith(currentInput)).ToList();
                var externalMatches = SearchPath.Search(currentInput)
                    .Where(b => b.StartsWith(currentInput)).ToList();
                var allMatches = new HashSet<string>(matches);
                
                foreach (var match in externalMatches){
                    allMatches.Add(match);
                }
                
                var matchList = allMatches.OrderBy(m => m).ToList();
                
                // Only auto-complete if there's exactly one match (no ambiguity)
                if (matchList.Count == 1)
                {
                    string match = matchList[0];
                    // Calculate the remaining part to complete + trailing space
                    // Example: typed "ec", match "echo" → remainder = "ho "
                    string remainder = match.Substring(currentInput.Length) + " ";
                    
                    // Display the completion on screen
                    Console.Write(remainder);
                    
                    // Add completion to the internal buffer
                    stringBuilder.Append(remainder);
                    lastKeyWasTab = false;
                }
                else if  (matchList.Count > 1)
                {
                    // Multiple matches - complete to longest common prefix
                    string lcp = LongestPrefix.GetLongestCommonPrefix(matchList);
                    
                    if (lcp.Length > currentInput.Length)
                    {
                        // We can make progress - complete to LCP
                        string remainder = lcp.Substring(currentInput.Length);
                        Console.Write(remainder);
                        stringBuilder.Append(remainder);
                        lastKeyWasTab = false;
                    }
                    else if(lastKeyWasTab){
                        // Second TAB - show all matches
                        Console.WriteLine();
                        
                        // Print all matches on one line, separated by double spaces
                        Console.WriteLine(string.Join("  ", matchList));
                        
                        // Re-display the prompt with current input
                        Console.Write($"$ {currentInput}");
                        lastKeyWasTab = false;
                    }
                    else{
                        // No progress possible, beep
                        Console.Write('\a');
                        lastKeyWasTab = true;
                    }
                }
               
                
                else{
                    Console.Write('\a'); // Beep to indicate no unique match
                    lastKeyWasTab = true;
                }
            }
            
            // 3. Handle BACKSPACE key - delete last character
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Length--;  // Remove last char from buffer
                    Console.Write("\b \b");  // Move back, overwrite with space, move back again
                }
                 lastKeyWasTab = false;
            }
            
            // 4. Handle NORMAL CHARACTERS - add to input
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                stringBuilder.Append(keyInfo.KeyChar);  // Add to buffer
                Console.Write(keyInfo.KeyChar);         // Display on screen
                lastKeyWasTab = false; // Reset flag
            }
            else{
                lastKeyWasTab = false;}
        }
    }

    
}