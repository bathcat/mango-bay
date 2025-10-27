namespace MBC.Core.Services;

public interface IHtmlSanitizer
{
    string Sanitize(string original);
}
