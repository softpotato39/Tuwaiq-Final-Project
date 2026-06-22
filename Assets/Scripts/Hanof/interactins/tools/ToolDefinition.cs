using UnityEngine;

namespace InteractionSystem
{
    /// <summary>
    /// Data for one equippable tool. Create instances via Assets > Create > Interaction System > Tool Definition.
    /// A tool kit pickup references a list of these to unlock at once.
    /// </summary>
    [CreateAssetMenu(menuName = "Interaction System/Tool Definition", fileName = "NewTool")]
    public class ToolDefinition : ScriptableObject
    {
        public string toolName;
        public Sprite icon;
        [Tooltip("Instantiated into the player's tool socket when this tool is equipped.")]
        public GameObject toolPrefab;
    }
}
