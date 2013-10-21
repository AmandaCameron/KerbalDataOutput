using System;
using SimpleJSON;

namespace KerbalDataOutput
{
	public class VesselInfo : Info
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
			mType = v.vesselType.ToString();
			mMissionTime = (int)v.missionTime;

			mId = v.id.ToString();

			mOrbit = v.GetOrbit();
		}

		public bool IsActive ()
		{
			return mActive;
		}

		public string GetID ()
		{
			return mId;
		}

		public JSONNode ToJson ()
		{
			var ret = new JSONClass ();

			ret ["name"] = mName;
			ret ["id"] = mId;
			ret ["type"] = mType;
			ret ["mission-time"].AsInt = mMissionTime;

			ret ["orbit"] = JsonifyOrbit(mOrbit);

			return ret;
		}
	}
}

