using System;
using System.Collections.Generic;
using SimpleJSON;

namespace KerbalDataOutput
{
	public interface Info
	{
		JSONNode ToJson ();
	}
}

