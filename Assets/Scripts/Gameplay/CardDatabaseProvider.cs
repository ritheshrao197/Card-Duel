namespace CardDuel.Gameplay
{
    public static class CardDatabaseProvider
    {
        private static RuntimeCardDatabase _db;

        public static void Initialize(RuntimeCardDatabase database)
        {
            _db = database;
        }

        public static CardData Get(int cardId)
        {
            return _db.GetById(cardId);
        }

        public static RuntimeCardDatabase Database => _db;
    }
}
