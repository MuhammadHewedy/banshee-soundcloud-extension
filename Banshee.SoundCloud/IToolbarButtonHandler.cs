using System;
using Banshee.Sources;

namespace Banshee.SoundCloud
{
	public abstract class IToolbarButtonHandler
	{
		protected PrimarySource primarySource;

		public IToolbarButtonHandler(PrimarySource primarySource){
			this.primarySource = primarySource;
		}

		public abstract void toolBarButtonClicked(object o, EventArgs args);
	}
}
