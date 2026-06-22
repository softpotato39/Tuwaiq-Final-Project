using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace InteractionSystem
{
    /// <summary>
    /// Lives on the player alongside PlayerInteractor. Tracks which tools have been unlocked
    /// (via ToolKitPickup) and equips/cycles through them with a single axis input action
    /// (bumpers, D-pad, or mouse scroll - whatever you bind CycleTool to in your Terminal/Player map).
    /// </summary>
    public class ToolController : MonoBehaviour
    {
        [Header("Input")]
        [Tooltip("A Value/Axis action: positive value = next tool, negative = previous tool.")]
        [SerializeField] private InputActionReference cycleToolAction;
        [Tooltip("Optional Button action: activates whatever's currently equipped, if it implements IUsableTool (e.g. toggling a flashlight).")]
        [SerializeField] private InputActionReference useToolAction;

        [Header("Equip Point")]
        [SerializeField] private Transform toolSocket;

        [Header("Events")]
        [Tooltip("Fires with the newly equipped tool (for HUD icon updates). Null is never passed - nothing fires until at least one tool is unlocked.")]
        public UnityEvent<ToolDefinition> OnToolEquipped;

        private readonly List<ToolDefinition> _unlockedTools = new List<ToolDefinition>();
        private GameObject _equippedInstance;
        private int _equippedIndex = -1;

        public ToolDefinition CurrentTool =>
            _equippedIndex >= 0 && _equippedIndex < _unlockedTools.Count ? _unlockedTools[_equippedIndex] : null;

        private void OnEnable()
        {
            if (cycleToolAction != null)
            {
                cycleToolAction.action.Enable();
                cycleToolAction.action.performed += OnCyclePerformed;
            }

            if (useToolAction != null)
            {
                useToolAction.action.Enable();
                useToolAction.action.performed += OnUsePerformed;
            }
        }

        private void OnDisable()
        {
            if (cycleToolAction != null)
            {
                cycleToolAction.action.performed -= OnCyclePerformed;
                cycleToolAction.action.Disable();
            }

            if (useToolAction != null)
            {
                useToolAction.action.performed -= OnUsePerformed;
                useToolAction.action.Disable();
            }
        }

        public void UnlockTools(IEnumerable<ToolDefinition> tools)
        {
            bool hadNoneBefore = _unlockedTools.Count == 0;

            foreach (var tool in tools)
            {
                if (tool != null && !_unlockedTools.Contains(tool))
                    _unlockedTools.Add(tool);
            }

            if (hadNoneBefore && _unlockedTools.Count > 0)
                Equip(0);
        }

        private void OnCyclePerformed(InputAction.CallbackContext ctx)
        {
            if (_unlockedTools.Count == 0) return;

            float dir = ctx.ReadValue<float>();
            if (Mathf.Approximately(dir, 0f)) return;

            int next = (_equippedIndex + (dir > 0f ? 1 : -1) + _unlockedTools.Count) % _unlockedTools.Count;
            Equip(next);
        }

        private void OnUsePerformed(InputAction.CallbackContext ctx)
        {
            if (_equippedInstance == null) return;
            _equippedInstance.GetComponent<IUsableTool>()?.Use();
        }

        private void Equip(int index)
        {
            if (_equippedInstance != null)
                Destroy(_equippedInstance);

            _equippedIndex = index;
            ToolDefinition tool = _unlockedTools[index];

            if (tool.toolPrefab != null && toolSocket != null)
            {
                _equippedInstance = Instantiate(tool.toolPrefab, toolSocket);
                _equippedInstance.transform.localPosition = Vector3.zero;
                _equippedInstance.transform.localRotation = Quaternion.identity;
            }

            OnToolEquipped?.Invoke(tool);
        }
    }
}
