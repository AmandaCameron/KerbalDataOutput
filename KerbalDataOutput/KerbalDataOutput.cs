using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace KerbalDataOutput
{
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class KerbalDataOutput : MonoBehaviour
	{
		private List<VesselInfo> mVessels;
		private List<SystemInfo> mSystem;

		private bool mPaused;
		private float mWarpSpeed;
		private double mScience;

		private Server mServer;

		public void Start ()
		{
			mServer = new Server ();

			// Game Stuff.
			mServer.Hook ("/version", HandleGame);

			// Vessels
			mServer.Hook ("/vessels/all", HandleAllVessels);
			mServer.Hook ("/vessels/active", HandleActiveVessel);
			mServer.Hook ("/vessels/by-id/", HandleSingleVessel);

			// Simulation
			mServer.Hook ("/sim", HandleSim);

			// Bodies.
			mServer.Hook ("/bodies/all", HandleAllBodies);
			mServer.Hook ("/bodies/by-name/", HandleSingleBody);
		}

		public void OnDestroy ()
		{
			mServer.Stop ();
		}

		private void HandleGame (Server.Client cli)
		{
			var res = new JSONClass ();

			res ["ksp-version"] = Versioning.GetVersionString ();
			res ["kdo-version"] = "0.0.1";

			cli.Success (res);
		}

		#region Vessel Handlers.

		private void HandleAllVessels (Server.Client cli)
		{
			var res = new JSONArray ();
			foreach (var v in mVessels) {
				res.Add (v.ToJson ());
			}

			cli.Success(res);

			return;
		}

		private void HandleActiveVessel (Server.Client cli)
		{
			foreach (var v in mVessels) {
				if (v.IsActive ()) {
					//return v.ToJson ();
					cli.Success(v.ToJson());
					return;
				}
			}

			cli.Error ("No active vessel");
		}

		private void HandleSingleVessel (Server.Client cli)
		{
			var id = cli.Path.Substring (15);

			foreach (var v in mVessels) {
				if (v.GetID () == id) {
					//return v.ToJson ();
					cli.Success(v.ToJson());
					return;
				}
			}

			cli.Error ("No vessel with ID " + id);
		}

		#endregion

		#region Simulation Handlers.

		public void HandleSim (Server.Client cli)
		{
			var data = new JSONClass ();

			data["paused"].AsBool = mPaused;
			data["warp-speed"].AsFloat = mWarpSpeed;
			data["science"].AsDouble = mScience;

			cli.Success(data);

			//return data;
		}

		#endregion

		#region Celestrial Bodies.

		public void HandleAllBodies (Server.Client cli)
		{
			var ret = new JSONArray ();

			foreach (var s in mSystem) {
				ret.Add (s.ToJson ());
			}

			cli.Success(ret);
		}

		public void HandleSingleBody (Server.Client cli)
		{
			var name = cli.Path.Substring (16).ToLower();

			foreach (var sys in mSystem) {
				if (sys.GetName ().ToLower() == name) {
					//return sys.ToJson ();
					cli.Success(sys.ToJson ());
					return;
				}
			}

			cli.Error ("No such body " + cli.Path.Substring (16));
		}

		#endregion

		public void Update ()
		{
			// Vessel information.
			mVessels = new List<VesselInfo>{};

			if (FlightGlobals.Vessels != null) {
				foreach (Vessel v in FlightGlobals.Vessels) {
					mVessels.Add (new VesselInfo (v));
				}
			}

			if (FlightGlobals.Bodies != null && mSystem == null) {
				mSystem = new List<SystemInfo> ();

				// Get the system information.
				foreach (var b in FlightGlobals.Bodies) {
					mSystem.Add (new SystemInfo (b));
				}
			}

			if (ResearchAndDevelopment.Instance != null) {
				mScience = ResearchAndDevelopment.Instance.Science;
			} else {
				mScience = -1;
			}
			mPaused = FlightDriver.Pause;
			mWarpSpeed = TimeWarp.CurrentRate;
		}
	}
}