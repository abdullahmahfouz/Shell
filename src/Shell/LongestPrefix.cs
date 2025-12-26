public class LongestPrefix
{
    /// <summary>Finds the longest common prefix among a list of strings</summary>
    /// <param name="strings">List of strings to evaluate</param>
   // Helper to find the longest common prefix among a list of strings
    public static string GetLongestCommonPrefix(List<string> strings)
    {
        if (strings.Count == 0) return "";
        
        // Sort to easily compare the most different strings (first and last)
        strings.Sort();
        
        string first = strings[0];
        string last = strings[strings.Count - 1];
        
        int minLength = Math.Min(first.Length, last.Length);
        int i = 0;
        
        // Compare characters until they differ
        while (i < minLength && first[i] == last[i])
        {
            i++;
        }
        
        return first.Substring(0, i);
    }
}