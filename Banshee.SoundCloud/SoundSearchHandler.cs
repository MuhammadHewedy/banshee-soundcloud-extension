using System;

using Gtk;
using Banshee.Sources;

namespace Banshee.SoundCloud
{
	public class SoundSearchHandler : IToolbarButtonHandler
	{
		public SoundSearchHandler(PrimarySource primarySource) : base(primarySource){}

		public void toolBarButtonClicked(object o, EventArgs args){
			BaseDialog editor = new BaseDialog("Search Sounds", "Type part of sound name to search", "Sound contains", Stock.Find);
			//editor.Response += OnArtistAdditionResponse;
			editor.Show();
		}
	}
}

