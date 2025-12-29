
using System.Text;

public class HistoryNavgation
{
    // Tracks current position in history when navigating with arrows
    // -1 means not navigating (at the fresh input line)
    private static int historyIndex = -1;
    
    // Stores what user typed before navigating history
    private static string savedInput = "";

    /// <summary>
    /// Resets navigation state - call this when starting a new input
    /// </summary>
    public static void Reset()
    {
        historyIndex = -1;
        savedInput = "";
    }

    /// <summary>
    /// Handles Up Arrow - navigate to previous (older) command in history
    /// </summary>
    /// <param name="sb">The current input buffer</param>
    public static void NavigateUp(StringBuilder sb)
    {
        var history = History.GetHistory();
        if (history.Count == 0) return;
        
        // Save current input if we're just starting to navigate
        if (historyIndex == -1)
        {
            savedInput = sb.ToString();
            historyIndex = history.Count;
        }
        
        // Move up in history (toward older commands)
        if (historyIndex > 0)
        {
            historyIndex--;
            ReplaceCurrentLine(sb, history[historyIndex]);
        }
    }

    /// <summary>
    /// Handles Down Arrow - navigate to next (newer) command in history
    /// </summary>
    /// <param name="sb">The current input buffer</param>
    public static void NavigateDown(StringBuilder sb)
    {
        var history = History.GetHistory();
        if (historyIndex == -1) return;  // Not navigating history
        
        // Move down in history (toward newer commands)
        if (historyIndex < history.Count - 1)
        {
            historyIndex++;
            ReplaceCurrentLine(sb, history[historyIndex]);
        }
        else
        {
            // Past the end - restore original input
            historyIndex = -1;
            ReplaceCurrentLine(sb, savedInput);
        }
    }

    /// <summary>
    /// Clears current line and replaces with new text
    /// </summary>
    private static void ReplaceCurrentLine(StringBuilder sb, string newText)
    {
        // Clear current display: move back, overwrite with spaces, move back again
        int currentLength = sb.Length;
        Console.Write(new string('\b', currentLength));  // Move cursor back
        Console.Write(new string(' ', currentLength));   // Overwrite with spaces
        Console.Write(new string('\b', currentLength));  // Move cursor back again
        
        // Display and store new text
        Console.Write(newText);
        sb.Clear();
        sb.Append(newText);
    }
}
