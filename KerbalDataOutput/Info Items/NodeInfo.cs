using System;
using SimpleJSON;

namespace KerbalDataOutput
{
	public class NodeInfo : Info
	{
		private OrbitInfo mOrbit;
		private double mDeltaV;
		private double mTime;

		public NodeInfo (ManeuverNode n)
		{
			mOrbit = new OrbitInfo (n.patch);
			mDeltaV = n.DeltaV.magnitude;
			mTime = n.UT;
		}

		public JSONNode ToJson()
		{
			var ret = new JSONClass ();

			ret ["delta-v"].AsDouble = mDeltaV;
			ret ["time"].AsDouble = mTime;
			ret ["orbit"] = mOrbit.ToJson ();

			return ret;
		}
	}
}

