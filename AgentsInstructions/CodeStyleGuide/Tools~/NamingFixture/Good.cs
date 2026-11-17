public interface IFixture { }
public static class Lookup { public const int Value = 1; }
public sealed class Good
{
    private int _value;
    private static int s_value;
    private const int Limit = 1;

    public int Compute()
    {
        const int LocalLimit = 3;
        return _value + s_value + Limit + LocalLimit + Lookup.Value;
    }
}
