using UnityEngine;

namespace Tactility.Calibration
{
    [CreateAssetMenu(fileName = "TactilityDeviceConfig", menuName = "Tactility/Device Config", order = 1)]
    public class TactilityDeviceConfig : ScriptableObject
    {
        [Tooltip("The name of the device. This is used for identification purposes.")]
        public string deviceName;
        [Tooltip("The number of tactile pads on the device (including those which can be both anodes and cathodes).")]
        public int numPads;
        [Tooltip("The minimum amplitude value for the tactile stimulation. This value represents the lower limit of intensity that can be applied to the pads. Setting values lower than this may either have no additional effect or could potentially cause hardware damage or user discomfort.")]
        public float minAmp;
        [Tooltip("Maximum amplitude value for the tactile stimulation. This value represents the upper limit of intensity that can be applied to the pads. Setting values higher than this may either have no additional effect or could potentially cause hardware damage or user discomfort.")]
        public float maxAmp;
        [Tooltip("The minimum pulse width value for the tactile stimulation in microseconds. This value dictates the shortest duration a single stimulation pulse can last. Similar to minAmp, setting pulse widths below this limit may not increase the perceived intensity and could risk hardware integrity or user safety.")]
        public float minWidth;
        [Tooltip("Maximum pulse width value for the tactile stimulation in microseconds. This value dictates the longest duration a single stimulation pulse can last. Similar to maxAmp, setting pulse widths beyond this limit may not increase the perceived intensity and could risk hardware integrity or user safety.")]
        public float maxWidth;
        [Tooltip("The base frequency value for the tactile stimulation in Hz. This is used to set the default frequency for the device upon connection. This value is used as the default frequency for the device and can be modulated later.")]
        public int baseFreq;
        [Tooltip("The minimum frequency value for the tactile stimulation in Hz. This value represents the lowest frequency that can be applied to the pads. Setting frequencies lower than this may have no additional effect or could potentially cause hardware damage or user discomfort.")]
        public int minFreq;
        [Tooltip("The maximum frequency value for the tactile stimulation in Hz. This value represents the highest frequency that can be applied to the pads. Setting frequencies higher than this may have no additional effect or could potentially cause hardware damage or user discomfort. Bare in mind that a value much larger than the base frequency will likely lead to no stimulation happening if many pads are active at once.")]
        public int maxFreq;
        [Tooltip("Whether to use special anodes logic when determining which pads are anodes. If the device supports it, using special anodes can result in a shorter command string and thus faster processing times. Special anodes works by not declaring anodes explicitly in the command string, with the understanding that the omitted pads should be considered anodes by the stimulator. If disabled, explicit anode/cathode declarations will be sent in every command string.")]
        public bool useSpecialAnodes;
        [Tooltip("The non-remapped indexes of the pads which are anodes. This is used to determine which pads are anodes when the device has special anodes. If the device does not have implicit anodes, this field is ignored. If the device has implicit anodes and this field is not set, the default behaviour is to use the first half of the pads as anodes and the second half as cathodes. If the device has implicit anodes and this field is set, the values in this field are used to determine which pads are anodes. For example, if the device has 4 pads and the mapping is { 0, 1, 2, 3 } and the anodes are { 0, 3 }, then pads 0 and 3 are anodes and pads 1 and 2 are cathodes. If the device has implicit anodes and this field is set, the length of the array must be equal to the number of pads")]
        public int[] anodes;

        public bool IsAnode(int padIndex)
        {
            if (anodes is null || anodes.Length == 0)
            {
                return false;
            }
            return System.Array.Exists(anodes, element => element == padIndex);
        }
    }
}