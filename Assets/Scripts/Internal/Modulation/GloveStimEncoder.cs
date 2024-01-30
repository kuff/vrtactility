using UnityEngine;

namespace Internal.Modulation
{
    [RequireComponent(typeof(IPadModulator))]
    [RequireComponent(typeof(IAmpModulator))]
    [RequireComponent(typeof(IWidthModulator))]
    public class GloveStimEncoder : MonoBehaviour, IStimEncoder
    {
        private void Start()
        {
            // TODO: Implement this
        }
        
        private void Update()
        {
            // TODO: Implement this
        }

        public string GetStimString()
        {
            throw new System.NotImplementedException();
        }

        public int GetIndexRemap(int index)
        {
            throw new System.NotImplementedException();
        }
    }
}
