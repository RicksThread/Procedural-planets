using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MEC;


namespace UtilityPack
{
    namespace InputSystem
    {
        ///<summary>
        ///It adapts the old inputSystem with the new (adaptation for old big system that are too big to replace)
        ///</summary>
        public class InputControllerButton
        {
            public static List<InputControllerButton> InputButtons = new List<InputControllerButton>();

            private bool pressedInput;
            private bool hasStarted = false;
            private bool hasCancelled = false;

            private InputAction inputType;

            public InputControllerButton(InputAction inputType)
            {
                this.inputType = inputType;
                inputType.Enable();
                inputType.performed += StartInputTimeDelay;
                inputType.canceled += CancelInputTimeDelay;
                InputButtons.Add(this);
            }

            void StartInputTimeDelay(InputAction.CallbackContext context)
            {
                hasStarted = true;

                pressedInput = true;
            }

            void CancelInputTimeDelay(InputAction.CallbackContext context)
            {
                hasCancelled = true;
                pressedInput = false;
            }

            ///<summary>
            ///Used to update the input calls (DO NOT CALL THIS)
            ///</summary>
            public void Tick()
            {
                if (hasCancelled) 
                {
                    hasCancelled = false;
                }
                if (hasStarted)
                {
                    hasStarted =false;
                }
            }

            public bool GetInput()
            {
                return pressedInput;
            }

            public bool GetInputStart()
            {
                return hasStarted;
            }

            public bool GetInputEnd()
            {
                return hasCancelled;
            }

            public void Dispose()
            {
                inputType.started -= StartInputTimeDelay;
                inputType.canceled -= CancelInputTimeDelay;
                InputButtons.Remove(this);
            }
            
            ~InputControllerButton()
            {
                Dispose();
            }
        }
    }
}

