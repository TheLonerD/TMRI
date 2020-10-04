namespace TMRI.UI.Console.Menu
{
    public static class Input
    {
        public static int ReadInt()
        {
            System.Console.Write("\nInput: ");
            string input = System.Console.ReadLine();
            int value;

            while (!int.TryParse(input, out value))
            {
                Output.DisplayPrompt("Please enter a number: ");
                input = System.Console.ReadLine();
            }

            return value;
        }

        public static int ReadInt(int min, int max)
        {
            int value = ReadInt();

            while (value < min || value > max)
            {
                Output.DisplayPrompt("Please enter a number between [{0};{1}]", min, max);
                value = ReadInt();
            }

            return value;
        }
    }
}
