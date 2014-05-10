using System;

using Hyena.Json;

using Gtk;
using Banshee.Sources;
using Banshee.Collection.Database;

namespace Banshee.SoundCloud
{
	public class SoundSearchHandler : ActionEntryHandler
	{
		public SoundSearchHandler(PrimarySource primarySource) : 
			base(primarySource, "Search Sounds", "Type part of sound name to search", "", Stock.Find, SCResources.TRACKS)
		{
			setOnResponseCallback(processTrackResponse);
		}

		private void processTrackResponse(JsonArray tracks){
			foreach(JsonObject t in tracks) {
				DatabaseTrackInfo track = IO.makeTrackInfo(t);
				track.PrimarySource = primarySource;
				track.IsLive = true;
				track.Save();
				SC.log("  added track: " + track.TrackTitle);
			}
		}
	}
}

