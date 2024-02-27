using System.Linq;
using UnityEngine;

namespace Tactility.Ball
{
    [RequireComponent(typeof(UniformGrabbable))]
    [RequireComponent(typeof(Renderer))]
    public class DebugGribVisualizer : MonoBehaviour
    {
        private UniformGrabbable _ug;
        private Renderer _renderer;
        private static readonly int Color1 = Shader.PropertyToID("_Color");
        private int _lastCount;  // The number of touching bones in the previous frame update

        private void Start()
        {
            _ug = GetComponent<UniformGrabbable>();
            _renderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            ref var tactilityData = ref _ug.GetTactilityData();
            
            // Don't process highlight if no bones are touching, but remember to update once after the list empties
            if (tactilityData.Values.Count + _lastCount == 0) return;
            _lastCount = tactilityData.Values.Count;

            // Calculate total pressure being applied and update sphere material color
            var maxPressure = tactilityData.Values.Count > 0
                ? tactilityData.Values.Max()
                : 0f;
            _renderer.material.SetColor(Color1, new Color(maxPressure, 0f, 0f));
        }
    }
}
