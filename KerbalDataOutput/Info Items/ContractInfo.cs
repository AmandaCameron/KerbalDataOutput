using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using KronalUtils;

namespace KerbalDataOutput
{
	class ContractInfo : Info
	{
		private bool mComplete;
		private double mWhenAccepted;
		private string mId;
		private string mState;

		private string mTitle;
		private string mDescription;

		private double mFundsAdvance;

		private double mScienceAward;
		private double mFundsAward;
		private double mReputationAward;

		private List<ContractParamaterInfo> mParameters;

		public ContractInfo(Contracts.Contract c) {
			mParameters = new List<ContractParamaterInfo> ();

			foreach (var p in c.AllParameters) {
				mParameters.Add (new ContractParamaterInfo (p));
			}

			mWhenAccepted = c.DateAccepted;

			mId = c.ContractGuid.ToString ();

			mState = c.ContractState.ToString ();

			mTitle = c.Title;
			mDescription = c.Description;

			mScienceAward = c.ScienceCompletion;
			mFundsAward = c.FundsCompletion;
			mReputationAward = c.ReputationCompletion;

			mFundsAdvance = c.FundsAdvance;
		}


		public bool IsActive() {
			return mState == "Active";
		}

		public bool IsCompleted() {
			return mState == "Complete";
		}

		public string GetID() {
			return mId;
		}

		public JSONNode ToJson() {
			var ret = new JSONClass ();

			ret ["title"] = mTitle;
			ret ["description"] = mDescription;

			ret ["state"] = mState;
			ret ["id"] = mId;

			ret ["awards"] = new JSONClass ();
			ret ["awards"] ["funds"].AsDouble = mFundsAward;
			ret ["awards"] ["science"].AsDouble = mScienceAward;
			ret ["awards"] ["reputation"].AsDouble = mReputationAward;

			ret ["funds-advance"].AsDouble = mFundsAdvance;

			//ret ["complete"].AsBool = mComplete;

			ret ["accepted-at"].AsDouble = mWhenAccepted;

			ret ["paramaters"] = new JSONArray ();

			foreach (var p in mParameters) {
				ret ["parameters"].Add (p.ToJson ());
			}

			return ret;
		}
	}

}