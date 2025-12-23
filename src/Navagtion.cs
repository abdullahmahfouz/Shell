// Navigation class handles directory-related operations for the shell
public class Navigation
{
    // Handle the cd (change directory) command
    public static void ChangeDir(string[] args){
        // Get the user's home directory from the environment variable
        var homeDir = Environment.GetEnvironmentVariable("HOME");
        
        // If no arguments provided, navigate to home directory
        if(args.Length == 0){
            // No argument provided, change to home directory
            if(homeDir != null){
                // Set current directory to home
                Directory.SetCurrentDirectory(homeDir);
            } 
            else {
                // HOME environment variable not set
                Console.WriteLine("cd: HOME not set");
            }
        }
        // Change to the specified directory
        else {
            // Get the directory path from the first argument
            var path = args[0];
            
            // Handle tilde (~) expansion - replace ~ with home directory
            if(path == "~")
            {
                if(homeDir != null){
                    // Replace ~ with actual home directory path
                    path = homeDir;
                } 
                else {
                    // Cannot expand ~ if HOME is not set
                    Console.WriteLine("cd: HOME not set");
                    return;
                }
            }
            
            try {
                // Attempt to change to the specified directory
                Directory.SetCurrentDirectory(path);
            } 
            catch (DirectoryNotFoundException){
                // Directory doesn't exist
                Console.WriteLine($"cd: {path}: No such file or directory");
            }
            catch (Exception ex){
                // Other errors (permission denied, etc.)
                Console.WriteLine($"cd: {path}: {ex.Message}");
            }
        }
    }

    // Get the current working directory path
    public static string GetCurrentDirectory(){
        // Return the full path of the current directory
        return Directory.GetCurrentDirectory();
    }
}