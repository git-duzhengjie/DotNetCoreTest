namespace ConsoleApp1
{
    public interface IPerson
    {
        void SayHello(string word1, string word2);

        void SayHello3(string w1, string w2, string w3);

        Hello GetMessage(string w);
    }
}
