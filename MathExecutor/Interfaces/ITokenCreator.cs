using MathExecutor.Models;

namespace MathExecutor.Interfaces
{
    public interface ITokenCreator
    {
        Token GetToken(SymbolType type, string value, int level, Token lastToken);
        Token GetToken(SymbolType type, string value, int level);
    }
}