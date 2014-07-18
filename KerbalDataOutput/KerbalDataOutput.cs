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
		private List<VesselInfo> mAsteroids;
		private List<SystemInfo> mSystem;

		private List<NodeInfo>   mNodes;
		private List<ContractInfo> mContracts;

		private bool mPaused;
		private float mWarpSpeed;

		private double mScience;
		private double mTime;
		private double mFunds;
		private double mReputation;

		private Server mServer;
	

		public void Start ()
		{

			mServer = new Server ();

			// Game Stuff.
			mServer.Hook ("/version", HandleVersion);
			mServer.Hook ("/game", HandleGame);

			// Vessels
			mServer.Hook ("/vessels/all", HandleAllVessels);
			mServer.Hook ("/vessels/list", HandleVesselList);

			mServer.Hook ("/vessels/active", HandleActiveVessel);
			mServer.Hook ("/vessels/by-id/", HandleSingleVessel);

			// Contracts
			mServer.Hook ("/contracts/all", HandleAllContracts);
			mServer.Hook ("/contracts/active", HandleActiveContracts);
			mServer.Hook ("/contracts/completed", HandleCompletedContracts);
	
			mServer.Hook ("/contracts/by-id/", HandleSingleContract);

			// Astroids
			mServer.Hook ("/asteroids/all", HandleAllAsteroids);
			mServer.Hook ("/asteroids/list", HandleAsteroidList);

			mServer.Hook ("/asteroids/by-id/", HandleSingleAsteroid);

			// Simulation
			mServer.Hook ("/sim", HandleSim);
			mServer.Hook ("/nodes", HandleNodes);

			// Bodies.
			mServer.Hook ("/bodies/all", HandleAllBodies);
			mServer.Hook ("/bodies/by-name/", HandleSingleBody);
		}

		public void OnDestroy ()
		{
			mServer.Stop ();
		}

		private void HandleGame (Server.Client cli) {
			var res = new JSONClass ();

			cli.Success (res);
		}

		private void HandleVersion (Server.Client cli)
		{
			var res = new JSONClass ();

			res ["ksp-version"] = Versioning.GetVersionString ();
			res ["kdo-version"] = "0.0.3";

			cli.Success (res);
		}

		#region Contracts!

		private void HandleAllContracts(Server.Client cli) {
			JSONArray ret = new JSONArray ();

			foreach (var c in mContracts) {
				ret.Add (c.ToJson ());
			}

			cli.Success (ret);
		}

		private void HandleSingleContract(Server.Client cli) {
			var cid = cli.Path.Substring (17);
			var c = mContracts.Find ((m) => m.GetID () == cid);

			if (c == null) {
				cli.Error ("No such contract '" + cid + "'");
			}

			cli.Success (c.ToJson());
		}

		private void HandleActiveContracts(Server.Client cli) {
			var ret = new JSONArray ();

			foreach (var c in mContracts.FindAll((m) => m.IsActive())) {
				ret.Add (c.ToJson ());
			}

			cli.Success (ret);
		}

		private void HandleCompletedContracts(Server.Client cli) {
			var ret = new JSONArray ();

			foreach (var c in mContracts.FindAll((m) => m.IsCompleted())) {
				ret.Add (c.ToJson ());
			}

			cli.Success (ret);
		}

		#endregion

		#region Asteroid Handlers.

		private void HandleAsteroidList(Server.Client cli) {
			JSONArray ret = new JSONArray ();

			foreach(var a in mAsteroids) {
				ret.Add (a.GetID ());
			}

			cli.Success (ret);
		}

		private void HandleAllAsteroids (Server.Client cli) {
			var ret = new JSONArray ();

			foreach(var a in mAsteroids) {
				ret.Add (a.ToJson ());
			}

			cli.Success (ret);
		}

		private void HandleSingleAsteroid (Server.Client cli) {
			var ret = mAsteroids.Find ((m) => m.GetID () == cli.Path.Substring (17)).ToJson();

			cli.Success (ret);
		}

		#endregion

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
					cli.Success(v.ToJson());
					return;
				}
			}

			cli.Error ("No active vessel");
		}
			
		private void HandleSingleVessel (Server.Client cli)
		{
			var id = cli.Path.Substring (15);
			var vessel = mVessels.Find ((m) => m.GetID () == id);

			if (vessel == null) {
				cli.Error ("No vessel with ID '" + id + "'");
			}

			cli.Success (vessel.ToJson ());
		}

		private void HandleVesselList(Server.Client cli) {
			var res = new JSONArray ();

			foreach(var v in mVessels) {
				res.Add (v.GetID ());
			}

			cli.Success(res);
		}

		#endregion

		#region Simulation Handlers.

		public void HandleSim (Server.Client cli)
		{
			var data = new JSONClass ();

			data ["paused"].AsBool = mPaused;
			data ["warp-speed"].AsFloat = mWarpSpeed;
			data ["time"].AsDouble = mTime;

			data ["science"].AsDouble = mScience;
			data ["funds"].AsDouble = mFunds;
			data ["reputation"].AsDouble = mReputation;

			cli.Success(data);
		}

		public void HandleNodes(Server.Client cli) {
			var res = new JSONArray ();

			foreach (var mn in mNodes) {
				res.Add (mn.ToJson ());
			}

			cli.Success (res);
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
					cli.Success(sys.ToJson ());
					return;
				}
			}

			cli.Error ("No such body " + cli.Path.Substring (16));
		}

		#endregion

		public void Update() {
			try {
				Do_Update ();
			} catch (Exception e) {
				MonoBehaviour.print ("Error in KDO Update: " + e.ToString ());
			}
		}


		private void Do_Update ()
		{
			// Vessel information.
			mVessels = new List<VesselInfo> ();
			mAsteroids = new List<VesselInfo> ();
			mContracts = new List<ContractInfo> ();

			mNodes = new List<NodeInfo> ();

			if (HighLogic.LoadedSceneHasPlanetarium) {
				if (FlightGlobals.Vessels != null) {
					foreach (Vessel v in FlightGlobals.Vessels) {
						if (v.vesselType == VesselType.SpaceObject) {
							mAsteroids.Add (new VesselInfo (v));
						} else {
							mVessels.Add (new VesselInfo (v));

							if (v.isActiveVessel) {
								foreach (ManeuverNode mn in v.patchedConicSolver.maneuverNodes) {
									mNodes.Add (new NodeInfo (mn));
								}
							}
						}
					}
				}

				if (FlightGlobals.Bodies != null && mSystem == null) {
					mSystem = new List<SystemInfo> ();

					foreach (var b in FlightGlobals.Bodies) {
						mSystem.Add (new SystemInfo (b));
					}
				}

				if (ResearchAndDevelopment.Instance != null) {
					mScience = ResearchAndDevelopment.Instance.Science;
				} else {
					mScience = -1;
				}

				if (Funding.Instance != null) {
					mFunds = Funding.Instance.Funds;
				} else {
					mFunds = -1;
				}


				if (Contracts.ContractSystem.Instance != null) {
					var inst = Contracts.ContractSystem.Instance;

					foreach(var contract in inst.Contracts) {
						mContracts.Add (new ContractInfo (contract));
					}
				}

				mReputation = Reputation.CurrentRep;

				mTime = Planetarium.GetUniversalTime ();
				mPaused = FlightDriver.Pause;
				mWarpSpeed = TimeWarp.CurrentRate;
			}
		}
	}
}