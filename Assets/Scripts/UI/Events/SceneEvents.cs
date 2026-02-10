namespace CardDuel.UI.Events
{
    public struct SceneLoadStartedEvent : IUIEvent
    {
        public string SceneName;
    }

    public struct SceneLoadCompletedEvent : IUIEvent
    {
        public string SceneName;
    }
}
