using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

//////////////////////////////////////////////  
//                                          //
//       This script shall live on:         //
//            The Player ! :D               //
//                                          //
//////////////////////////////////////////////

// script's purpose: it basically tracks what tools have been unlocked, lets the player
//                   cycle them and equip/unequip them ! >:3

// script's requirements: Assigned Use, Cycle, and Unequip action buttons.

namespace InteractionSystem // <-- this is for unity so it groups classes together without losing track
{
    public class ToolController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference cycleToolAction;      // the button for cycling between the items
        [SerializeField] private InputActionReference useToolAction;        // the button for using the tool, whatever the use is
        [SerializeField] private InputActionReference unequipToolAction;    // the same button for equip and unequip :3 i like it this way

        [Header("Equip Point")]
        [SerializeField] private Transform toolSocket;

        [Header("UI Stuff")]
        // this activates when a tool is equipped, we can do stuff with the UI later like flashlight battery or something
        public UnityEvent<ToolDefinition> OnToolEquipped;

        [Header("Unequip/Equip Animation")]
        // i wanted a little animation of the tools sliding back to play once equipped/ uneqipped :3 
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
            // starts looking for player input !

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
            // stops looking for player input - no lag >:)
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
            // this stops the player from equipping and unequipping
            // without actually unlocking the tools first >:(
            // and automatically equips the first tool unlocked ;3

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
            // im scared of maths :)
            // basically counts the tools unlocked and cycles forward and backward

            if (_unlockedTools.Count == 0) return;
            if (_equippedIndex == -1) return;

            float dir = ctx.ReadValue<float>();
            if (Mathf.Approximately(dir, 0f)) return;

            int next = (_equippedIndex + (dir > 0f ? 1 : -1) + _unlockedTools.Count) % _unlockedTools.Count;
            Equip(next);
        }
        private void OnUsePerformed(InputAction.CallbackContext ctx)
        {
            // lets the player actually use the tool once they cycle to it

            if (_equippedInstance == null) return;
            _equippedInstance.GetComponent<IUsableTool>()?.Use();
        }
        private void OnUnequipPerformed(InputAction.CallbackContext ctx)
        {
            if (_equippedIndex == -1)
                Equip(_lastEquippedIndex);
            else
                Unequip();
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
        public void Unequip()
        {
            if (_equippedInstance == null) return;

            _lastEquippedIndex = _equippedIndex;
            _equippedIndex = -1;
            StartCoroutine(UnequipRoutine(_equippedInstance));
            _equippedInstance = null;
        }
        private IEnumerator EquipRoutine(GameObject instance)
        {
            // this is the equip animation !
            // having it like this is a lot easier because i wont have to animate
            // each tool. Also the speed and direction will be the same for all tools ! :D
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
        private IEnumerator UnequipRoutine(GameObject instance)
        {
            // this is the unequip animation !
            // having it like this is a lot easier because i wont have to animate
            // each tool. Also the speed and direction will be the same for all tools ! :D

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
