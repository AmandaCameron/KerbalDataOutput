using System;
using SimpleJSON;

namespace KerbalDataOutput
{
	public class NodeInfo : Info
	{
		private Orbit mOrbit;
		private double mDeltaV;

		public NodeInfo (ManeuverNode n)
		{
			mOrbit = n.patch;
			mDeltaV = n.DeltaV.magnitude;
		}

		public JSONNode ToJSON()
		{
			var ret = new JSONClass ();

			ret ["delta-v"].AsDouble = mDeltaV;
			ret ["orbit"] = JsonifyOrbit (mOrbit);

			return ret;
		}
	}
}

