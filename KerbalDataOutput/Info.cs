using System;
using System.Collections.Generic;
using SimpleJSON;

namespace KerbalDataOutput
{
	// Base class for the various JSON information dumps, Provides helper
	// functions for great justice.
	public class Info
	{
		protected JSONClass JsonifyOrbit (Orbit o)
		{
			var ret = new JSONClass ();

			ret ["body"] = o.referenceBody.GetName ();

			if (!Double.IsNaN (o.ApA)) {
				ret ["apoapsis"].AsDouble = o.ApA;
			}

			if (!Double.IsNaN (o.PeA)) {
				ret ["periapsis"].AsDouble = o.PeA;
			}

			ret["time-to"] = new JSONClass();

			ret["time-to"]["apoapsis"].AsInt = (int)o.timeToAp;
			ret["time-to"]["periapsis"].AsInt = (int)o.timeToPe;
			ret["time-to"]["transition-one"].AsInt = (int)o.timeToTransition1;
			ret["time-to"]["transition-two"].AsInt = (int)o.timeToTransition2;

			return ret;
		}
	}

}

