using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Tactility.Box
{
    [RequireComponent(typeof(SerialController))]
    public class DebugSerialControllerAutomaticConnect : MonoBehaviour
    {
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
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
