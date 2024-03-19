// Copyright (C) 2024 Peter Leth

#region
using System.Collections.Generic;
#endregion

namespace Tactility.Modulation
{
    public struct TactilityData
    {
        public List<float> Values { get; set; }
        public List<OVRSkeleton.BoneId> BoneIds { get; set; }
    }

    public interface ITactilityDataProvider
    {
        ref TactilityData GetTactilityData();
        bool IsActive();
    }
}
