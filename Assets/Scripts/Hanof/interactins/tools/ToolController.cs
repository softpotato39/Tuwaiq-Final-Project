using System.Collections;
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
        [SerializeField] private InputActionReference unequipToolAction;

        [Header("Equip Point")]
        [SerializeField] private Transform toolSocket;

        [Header("Events")]
        [Tooltip("Fires with the newly equipped tool (for HUD icon updates). Null is never passed - nothing fires until at least one tool is unlocked.")]
        public UnityEvent<ToolDefinition> OnToolEquipped;

        [Header("Unequip/Equip Animation")]
        [SerializeField] private float unequipSlideDistance = 0.3f;
        [SerializeField] private float unequipDuration = 0.15f;

        [SerializeField] private float equipSlideDistance = 0.3f;
        [SerializeField] private float equipDuration = 0.15f;

        private readonly List<ToolDefinition> _unlockedTools = new List<ToolDefinition>();
        private GameObject _equippedInstance;
        private int _equippedIndex = -1;
        private int _lastEquippedIndex = 0;

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

            if (unequipToolAction != null)
            {
                unequipToolAction.action.Enable();
                unequipToolAction.action.performed += OnUnequipPerformed;
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

            if (unequipToolAction != null)
            {
                unequipToolAction.action.performed -= OnUnequipPerformed;
                unequipToolAction.action.Disable();
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
            if (_equippedIndex == -1) return;  // add this line

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
                StartCoroutine(EquipRoutine(_equippedInstance));
            }

            OnToolEquipped?.Invoke(tool);
        }

        private IEnumerator EquipRoutine(GameObject instance)
        {
            Vector3 endPos = Vector3.zero;
            Vector3 startPos = endPos + Vector3.back * equipSlideDistance;
            float elapsed = 0f;

            instance.transform.localPosition = startPos;

            while (elapsed < equipDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / equipDuration);
                instance.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            instance.transform.localPosition = endPos;
        }

        private void OnUnequipPerformed(InputAction.CallbackContext ctx)
        {
            if (_equippedIndex == -1)
                Equip(_lastEquippedIndex);
            else
                Unequip();
        }

        public void Unequip()
        {
            if (_equippedInstance == null) return;

            _lastEquippedIndex = _equippedIndex;
            _equippedIndex = -1;
            StartCoroutine(UnequipRoutine(_equippedInstance));
            _equippedInstance = null;
        }

        private IEnumerator UnequipRoutine(GameObject instance)
        {
            Vector3 startPos = instance.transform.localPosition;
            Vector3 endPos = startPos + Vector3.back * unequipSlideDistance;
            float elapsed = 0f;

            while (elapsed < unequipDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 2f, elapsed / unequipDuration);
                instance.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            Destroy(instance);
        }

    }
}
