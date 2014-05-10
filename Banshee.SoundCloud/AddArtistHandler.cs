using System;
using Mono.Unix;
using Gtk;

using Hyena.Json;

using Banshee.Base;
using Banshee.Sources;
using Banshee.Collection.Database;


namespace Banshee.SoundCloud
{
	public class AddArtistHandler : ActionEntryHandler
	{
		public AddArtistHandler(PrimarySource primarySource) : 
			base(primarySource, "Add Sound Owner", "Add exact sound owner name to find (ex TvQuran)", "", Stock.Add, SCResources.PEOPLE)
		{
			setOnResponseCallback(proccessPeopleResponse);
		}

		private void proccessPeopleResponse(JsonArray results){
			foreach(JsonObject artist in results) {
				string artist_name = (string)artist["username"];

				if (artist_name == editorEntryVal) {
					IO.MakeRequest(SCResources.GET_ALL_TRACKS, (int)artist["id"], processTracksResponse);
				}
			}
		}

		private void processTracksResponse(JsonArray tracks){
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

