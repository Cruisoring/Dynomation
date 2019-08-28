namespace UILocator.Enums
{
    public enum ReadyState
    {
        uninitialized   // - Has not started loading yet
        , loading       // - Is loading
        , loaded        // - Has been loaded
        , interactive   // - Has loaded enough and the user can interact with it
        , complete      // - Fully loaded
        , unknown
    }
}
