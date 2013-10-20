using System;
using SimpleJSON;

namespace KerbalDataOutput
{
	public class VesselInfo
	{
		private string mName;
		private string mId;

		private bool mActive;
		private string mType;
		private int mMissionTime;
		private Orbit mOrbit;

			
		public VesselInfo (Vessel v)
		{
			mName = v.GetName();
			mActive = v.isActiveVessel;
			mType = v.isEVA ? "EVA" : "Ship";
			mMissionTime = (int)v.missionTime;

			mId = v.GetInstanceID().ToString("X");

			mOrbit = v.GetOrbit();
		}

		public JSONNode ToJson ()
		{
			var ret = new JSONClass ();

			ret ["name"] = mName;
			ret ["id"] = mId;
			ret ["mission-time"].AsInt = mMissionTime;

			ret ["orbit"] = new JSONClass ();
			ret ["orbit"] ["body"] = mOrbit.referenceBody.GetName ();
			ret ["orbit"] ["apoapsis"].AsDouble = mOrbit.ApA;
			ret ["orbit"] ["periapsis"].AsDouble = mOrbit.PeA;
			ret ["orbit"] ["time-to-ap"].AsInt = (int)mOrbit.timeToAp;
			ret ["orbit"] ["time-to-pe"].AsInt = (int)mOrbit.timeToPe;

			return ret;
		}
	}
}

