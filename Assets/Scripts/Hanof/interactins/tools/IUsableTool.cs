namespace InteractionSystem
{
    /// <summary>
    /// Optional - implement on a tool's prefab root if pressing the "use" input while it's
    /// equipped should do something (toggle a flashlight, swing a mop, fire a tool, etc).
    /// ToolController looks for this automatically on whatever's currently equipped; tools
    /// that don't need an active "use" simply don't implement it.
    /// </summary>
    public interface IUsableTool
    {
        void Use();
    }
}
