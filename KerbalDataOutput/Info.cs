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
			var ret = new JSONClass();

			ret ["orbit"] ["body"] = o.referenceBody.GetName ();
			ret ["orbit"] ["apoapsis"].AsDouble = o.ApA;
			ret ["orbit"] ["periapsis"].AsDouble = o.PeA;
			ret ["orbit"] ["time-to-ap"].AsInt = (int)o.timeToAp;
			ret ["orbit"] ["time-to-pe"].AsInt = (int)o.timeToPe;

			return ret;
		}
	}

}

