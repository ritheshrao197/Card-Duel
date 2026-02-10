namespace CardDuel.Domain.Commands
{
    public interface ICommand
    {
        ulong PlayerId { get; }
    }
}
