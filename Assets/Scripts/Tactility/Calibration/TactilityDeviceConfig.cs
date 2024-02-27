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
        [Tooltip("Maximum amplitude value for the tactile stimulation. This value represents the upper limit of " +
                 "intensity that can be applied to the pads. Setting values higher than this may either have no " +
                 "additional effect or could potentially cause hardware damage or user discomfort.")]
        public float maxAmp;
        [Tooltip("Maximum pulse width value for the tactile stimulation in microseconds. This value dictates the " +
                 "longest duration a single stimulation pulse can last. Similar to maxAmp, setting pulse widths " +
                 "beyond this limit may not increase the perceived intensity and could risk hardware integrity or user safety.")]
        public float maxWidth;
        [Tooltip("The base frequency for the tactility device. You should most likely not need to change this.")]
        public int baseFreq;
        [Tooltip("The non-remapped indexes of the pads which are anodes. This is used to determine which pads are " +
                 "anodes when the device has special anodes. If the device does not have implicit anodes, this " +
                 "field is ignored. If the device has implicit anodes and this field is not set, the default " +
                 "behaviour is to use the first half of the pads as anodes and the second half as cathodes. If the " +
                 "device has implicit anodes and this field is set, the values in this field are used to determine " +
                 "which pads are anodes. For example, if the device has 4 pads and the mapping is { 0, 1, 2, 3 } " +
                 "and the anodes are { 0, 3 }, then pads 0 and 3 are anodes and pads 1 and 2 are cathodes. If the " +
                 "device has implicit anodes and this field is set, the length of the array must be equal to the " +
                 "number of pads")]
        public int[] anodes;
        // [Tooltip("The remap for the pads, with each value corresponding to the pad number. This is used to map the " +
        //          "modulation data to the correct pad. For example, if the device has 4 pads and the mapping is " +
        //          "{ 0, 1, 2, 3 }, then the first value in the modulation data will be sent to pad 0, the second to " +
        //          "pad 1, and so on. If the mapping is { 1, 0, 3, 2 }, then the first value in the modulation data " +
        //          "will be sent to pad 1, the second to pad 0, and so on. This is useful for devices where the pads " +
        //          "are not in a linear order. If the mapping is not set, the default mapping is used, which is a " +
        //          "linear mapping from 0 to numPads - 1.")]
        // public int[] mapping;
        
        // public int GetRemappedPadIndex(int padIndex)
        // {
        //     if (mapping is null || mapping.Length == 0)
        //         return padIndex;
        //     return mapping[padIndex];
        // }

        public bool IsAnode(int padIndex)
        {
            if (anodes is null || anodes.Length == 0)
                return false;
            return System.Array.Exists(anodes, element => element == padIndex);
        }
    }
}