using System.IO;
using System.Text;

public class Logger
{
    private string log_path;
    public Logger(string log_path)
    {
        this.log_path = log_path;
        File.WriteAllText(log_path, string.Empty);
    }

    public void WriteIntoLog(string message)
    {
        using (StreamWriter writer = new StreamWriter(log_path, true, Encoding.UTF8))
        {
            writer.WriteLine(message);
        }
    }
}
