using System;
using SimpleJSON;

namespace KerbalDataOutput
{
	public class ContractParamaterInfo : Info
	{
		private string mTitle;
		private string mState;
		private string mNotes;

		private double mFundsAward;
		private double mScienceAward;
		private double mReputationAward;

		private double mFundsPenalty;
		private double mReputationPenalty;

		private bool mEnabled;
		private bool mOptional;

		public ContractParamaterInfo (Contracts.ContractParameter p)
		{
			mTitle = p.Title;
			mState = p.State.ToString();
			mNotes = p.Notes;

			mEnabled = p.Enabled;
			mOptional = p.Optional;

			mFundsAward = p.FundsCompletion;
			mScienceAward = p.ScienceCompletion;
			mReputationAward = p.ReputationCompletion;

			mReputationPenalty = p.ReputationFailure;
			mFundsPenalty = p.FundsFailure;
		}

		public JSONNode ToJson() {
			var ret = new JSONClass();

			ret ["title"] = mTitle;
			ret ["state"] = mState;
			ret ["notes"] = mNotes;

			ret ["enabled"].AsBool = mEnabled;
			ret ["optional"].AsBool = mOptional;

			ret ["award"] = new JSONClass ();
			ret ["award"] ["funds"].AsDouble = mFundsAward;
			ret ["award"] ["reputation"].AsDouble = mReputationAward;
			ret ["award"] ["science"].AsDouble = mScienceAward;

			ret ["failure-penalty"] = new JSONClass ();
			ret ["failure-penalty"] ["funds"].AsDouble = mFundsPenalty;
			ret ["failure-penalty"] ["reputation"].AsDouble = mReputationPenalty;

			return ret;
		}
	}
}

