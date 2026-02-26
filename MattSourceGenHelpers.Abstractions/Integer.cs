namespace MattSourceGenHelpers.Abstractions;

public static class Integer
{
    public static int[] Range(int start, int end) => Enumerable.Range(start, end - start + 1).ToArray();
}