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
		private String editorEntryVal;

		public AddArtistHandler(PrimarySource primarySource) : base(primarySource){}

		public override void actionButtonClicked(object o, EventArgs args){
			BaseDialog editor = new BaseDialog("Add Sound Owner", "Add exact sound owner name to find (ex TvQuran)", "", Stock.Add);
			editor.Response += OnArtistAdditionResponse;
			editor.Show();
		}

		// TODO move this method up to the parent class
		private void OnArtistAdditionResponse(object o, ResponseArgs args)
		{
			BaseDialog editor = (BaseDialog)o;
			bool destroy = true;

			try {
				if(args.ResponseId == ResponseType.Ok) {
					if(String.IsNullOrEmpty(editorEntryVal = editor.Entry)) {
						destroy = false;
						editor.ErrorMessage = Catalog.GetString("Please provide owner name");
					} else {
						IO.MakeRequest("people", editor.Entry, proccessPeopleResponse);
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

		private void proccessPeopleResponse(JsonArray results){
			foreach(JsonObject artist in results) {
				string artist_name = (string)artist["username"];

				if (artist_name == editorEntryVal) {
					IO.MakeRequest("getalltracks", (int)artist["id"], processTracksResponse);
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

