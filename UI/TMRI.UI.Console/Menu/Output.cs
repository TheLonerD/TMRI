namespace TMRI.UI.Console.Menu
{
    public static class Output
    {
        public static void DisplayPrompt(string format, params object[] args)
        {
            format = format.Trim() + " ";
            System.Console.Write(format, args);
        }
    }
}
