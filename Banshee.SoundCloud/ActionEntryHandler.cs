using System;
using Banshee.Sources;

namespace Banshee.SoundCloud
{
	public abstract class ActionEntryHandler
	{
		protected PrimarySource primarySource;

		public ActionEntryHandler(PrimarySource primarySource){
			this.primarySource = primarySource;
		}

		public abstract void actionButtonClicked(object o, EventArgs args);
	}
}
