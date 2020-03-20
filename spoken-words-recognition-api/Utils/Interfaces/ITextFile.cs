namespace Utils.Interfaces
{
    public interface ITextFile
    {
        string ToText();
        ITextFile FromText(string text);
    }
}