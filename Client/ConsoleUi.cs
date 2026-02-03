namespace Client;

public static class ConsoleUi
{
    public static void CenterWrite(string text, int offsetY = 0)
    {
        int left = (Console.WindowWidth - text.Length) / 2;
        int top = Console.CursorTop + offsetY;
        Console.SetCursorPosition(left, top);
        Console.Write(text);
    }

    public static void CenterWriteLine(string text, int offsetY = 0)
    {
        CenterWrite(text, offsetY);
        Console.WriteLine();
    }

    public static void CenterWriteLineInfo(string text, int offsetY = 0)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        CenterWrite(text, offsetY);
        Console.WriteLine();
        Console.ResetColor();
    }
}