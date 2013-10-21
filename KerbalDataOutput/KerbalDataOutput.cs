using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace KerbalDataOutput
{
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class KerbalDataOutput : MonoBehaviour
	{
		private List<VesselInfo> mInfo;
		private List<SystemInfo> mSystem;

		private bool mPaused;
		private float mWarpSpeed;

		private Server mServer;

		public void Start ()
		{
			mServer = new Server ();

			// Vessels
			mServer.Hook (HandleAllVessels);
			mServer.Hook (HandleActiveVessel);
			mServer.Hook (HandleSingleVessel);

			// Simulation
			mServer.Hook (HandleSim);

			// Bodies.
			mServer.Hook (HandleAllBodies);
		}

		public void OnDestroy ()
		{
			mServer.Stop ();
		}

		#region Vessel Handlers.

		private JSONNode HandleAllVessels (string path)
		{
			if (path != "/vessels/all") {
				return null;
			}

			var res = new JSONArray ();
			foreach (var v in mInfo) {
				res.Add (v.ToJson ());
			}

			return res;
		}

		private JSONNode HandleActiveVessel (string path)
		{
			if (path != "/vessels/active") {
				return null;
			}

			foreach (var v in mInfo) {
				if (v.IsActive ()) {
					return v.ToJson ();
				}
			}

			return null;
		}

		private JSONNode HandleSingleVessel (string path)
		{
			if (!path.StartsWith ("/vessels/")) {
				return null;
			}

			var id = path.Substring (9);

			foreach (var v in mInfo) {
				if (v.GetID () == id) {
					return v.ToJson ();
				}
			}

			return null;
		}

		#endregion

		#region Simulation Handlers.

		public JSONNode HandleSim (string path)
		{
			if (path != "/sim") {
				return null;
			}

			var data = new JSONClass ();

			data["paused"].AsBool = mPaused;
			data["warp-speed"].AsFloat = mWarpSpeed;

			return data;
		}

		#endregion

		#region Celestrial Bodies.

		public JSONNode HandleAllBodies (string path)
		{
			if (path != "/bodies/all") {
				return null;
			}

			var ret = new JSONArray ();

			foreach (var s in mSystem) {
				ret.Add (s.ToJson ());
			}


			return ret;
		}

		#endregion

		public void Update ()
		{
			// Vessel information.
			mInfo = new List<VesselInfo>{};

			if (FlightGlobals.Vessels != null) {
				foreach (Vessel v in FlightGlobals.Vessels) {
					mInfo.Add (new VesselInfo (v));
				}
			}

			if (FlightGlobals.Bodies != null && mSystem == null) {
				mSystem = new List<SystemInfo> ();

				// Get the system information.
				foreach (var b in FlightGlobals.Bodies) {
					mSystem.Add (new SystemInfo (b));
				}
			}
			 
			mPaused = FlightDriver.Pause;
			mWarpSpeed = TimeWarp.CurrentRate;
		}
	}
}