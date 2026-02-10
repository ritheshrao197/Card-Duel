namespace CardDuel.Gameplay.Commands
{
    [System.Serializable]
    public struct PlayCardCommand : CardDuel.Domain.Commands.ICommand
    {
        public ulong PlayerId;
        public int CardId;

        ulong CardDuel.Domain.Commands.ICommand.PlayerId => PlayerId;
    }
}
