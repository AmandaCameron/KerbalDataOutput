using System;
using System.Collections.Generic;
using SimpleJSON;

namespace KerbalDataOutput
{
	public class SystemInfo : Info
	{
		private string mName;

		private double mMass;
		private double mRadius;
		private double mSOI;

		private Orbit mOrbit;

		private List<string> mChildren;

		public SystemInfo (CelestialBody b)
		{
			mName = b.GetName ();

			mMass = b.Mass;
			mRadius = b.Radius;
			mSOI = b.sphereOfInfluence;

			if (b.GetName() != "Sun") {
				mOrbit = b.GetOrbit ();
			}

			mChildren = new List<string> ();

			if (b.orbitingBodies != null) {
				foreach (var c in b.orbitingBodies) {
					mChildren.Add (c.GetName ());
				}
			}
		}

		public string GetName ()
		{
			return mName;
		}

		public JSONNode ToJson ()
		{
			var ret = new JSONClass ();

			ret ["name"] = mName;
			ret ["mass"].AsDouble = mMass;
			ret ["radius"].AsDouble = mRadius;

			ret ["sphere-of-influence"].AsDouble = mSOI;

			if (mOrbit != null) {
				ret["orbit"] = JsonifyOrbit (mOrbit);
			}

			ret ["children"] = new JSONArray ();

			foreach (var c in mChildren) {
				ret ["children"].Add (c);
			}

			return ret;
		}
	}
}

