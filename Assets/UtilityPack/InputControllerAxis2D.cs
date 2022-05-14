using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace UtilityPack
{
    namespace InputSystem
    {
        public class InputControllerAxis2D
        {
            InputAction inputType;
            Vector2 axis2D;

            public InputControllerAxis2D(InputAction inputType){
                inputType.Enable();
                this.inputType = inputType;
                inputType.performed += UpdateAxis2D;
                inputType.canceled += UpdateAxis2D;
            }

            void UpdateAxis2D(InputAction.CallbackContext context){
                axis2D = context.ReadValue<Vector2>();
            }

            public Vector2 GetAxis(){
                return axis2D;
            }

            public void Dispose(){
                inputType.performed -= UpdateAxis2D;
                inputType.canceled -= UpdateAxis2D;
            }
        }
    }
}
