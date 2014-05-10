using System;
using Mono.Unix;
using Gtk;

using Hyena.Json;

using Banshee.Base;
using Banshee.Sources;
using Banshee.Collection.Database;


namespace Banshee.SoundCloud
{
	public class AddArtistHandler : IToolbarButtonHandler
	{
		public AddArtistHandler(PrimarySource primarySource) : base(primarySource){}

		public override void toolBarButtonClicked(object o, EventArgs args){
			BaseDialog editor = new BaseDialog("Add Sound Owner", "Add exact sound owner name to find (ex TvQuran)", "", Stock.Add);
			// Add OnArtistAdditionResponse to the list of event handlers
			// for the artist adder.
			editor.Response += OnArtistAdditionResponse;
			editor.Show();
		}

		private void OnArtistAdditionResponse(object o, ResponseArgs args)
		{
			BaseDialog editor =(BaseDialog)o;
			bool destroy = true;

			try {
				if(args.ResponseId == ResponseType.Ok) {
					if(String.IsNullOrEmpty(editor.ArtistName)) {
						destroy = false;
						editor.ErrorMessage = Catalog.GetString("Please provide a artist name");
					} else {
						IO.MakeRequest("people", editor.ArtistName, proccessPeopleResponse, editor.ArtistName);
						destroy = true;
					}
				}
			} finally {
				if(destroy) {
					// Remove response-handler reference.
					editor.Response -= OnArtistAdditionResponse;
					editor.Destroy();
				}
			}
		}

		private void proccessPeopleResponse(JsonArray results, String artistName){
			foreach(JsonObject artist in results) {
				string artist_name = (string)artist["username"];

				if (artist_name == artistName) {
					IO.MakeRequest("getalltracks", (int)artist["id"], 
					               processTracksResponse, artistName);
				}
			}
		}

		private void processTracksResponse(JsonArray tracks, String dummy){
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

