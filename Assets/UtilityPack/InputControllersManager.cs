using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 

namespace UtilityPack
{
    namespace InputSystem
    {
        public class InputControllersManager : MonoBehaviour
        {
            private void LateUpdate()
            {
                for (int i = 0; i < InputControllerButton.InputButtons.Count; i++)
                {
                    InputControllerButton.InputButtons[i].Tick();
                }    
            }
        }
    }
}
