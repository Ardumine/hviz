using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BaseSLAM
{

    /// <summary>
    /// Scan segment.
    /// This is a set of lidar rays at certain pose.
    /// </summary>
    public class ScanSegment
    {
        /// <summary>
        /// Scan rays
        /// </summary>
        public List<Ray> Rays { get; set; } = new List<Ray>();

        /// <summary>
        /// Origin of rays
        /// </summary>
        public Vector3 Pose { get; set; }

        /// <summary>
        /// Is it last segment of the full 360 degrees scan ?
        /// </summary>
        public bool IsLast { get; set; }
    }
}
