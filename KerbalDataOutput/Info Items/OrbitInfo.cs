using System;
using SimpleJSON;

namespace KerbalDataOutput
{
	public class OrbitInfo : Info
	{
		private Orbit mOrbit;

		public OrbitInfo (Orbit orbit)
		{
			mOrbit = orbit;
		}
			
		public JSONNode ToJson ()
		{
			var ret = new JSONClass ();

			ret ["body"] = mOrbit.referenceBody.GetName ();

			ret ["apoapsis"].AsDouble = mOrbit.ApA;
			ret ["periapsis"].AsDouble = mOrbit.PeA;

			ret ["progress"].AsDouble = mOrbit.orbitPercent;

			ret["time-to"] = new JSONClass();

			ret["time-to"]["apoapsis"].AsInt = (int)mOrbit.timeToAp;
			ret["time-to"]["periapsis"].AsInt = (int)mOrbit.timeToPe;
			ret["time-to"]["transition-one"].AsInt = (int)mOrbit.timeToTransition1;
			ret["time-to"]["transition-two"].AsInt = (int)mOrbit.timeToTransition2;

			return ret;
		}
	}
}

