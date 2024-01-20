using UnityEngine;

namespace Internal.Box
{
    [RequireComponent(typeof(SerialController))]
    public class DebugSerialControllerAutomaticConnect : MonoBehaviour
    {
        private void Start()
        {
            var sc = GetComponent<SerialController>();
        
            sc.SendSerialMessage("iam TACTILITY\r");
            sc.SendSerialMessage("elec 1 *pads_qty 32\r");
            sc.SendSerialMessage("battery ?\r");
            sc.SendSerialMessage("freq 50\r");
        }
    }
}
