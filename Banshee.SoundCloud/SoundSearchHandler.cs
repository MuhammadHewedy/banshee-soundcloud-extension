using System;

using Gtk;
using Banshee.Sources;

namespace Banshee.SoundCloud
{
	public class SoundSearchHandler : ActionEntryHandler
	{
		public SoundSearchHandler(PrimarySource primarySource) : base(primarySource){}

		public override void actionButtonClicked(object o, EventArgs args){
			BaseDialog editor = new BaseDialog("Search Sounds", "Type part of sound name to search", "", Stock.Find);
			editor.Response += onSearchDialogResponse;
			editor.Show();
		}

		private void onSearchDialogResponse(object o, ResponseArgs args){
			/*
			BaseDialog editor =(BaseDialog)o;
			bool destroy = true;

			try {
				if(args.ResponseId == ResponseType.Ok) {
					if(String.IsNullOrEmpty(editor.Entry)) {
						destroy = false;
						editor.ErrorMessage = Catalog.GetString("Please provide a artist name");
					} else {
						IO.MakeRequest("people", editor.Entry, proccessPeopleResponse);
						destroy = true;
					}
				}
			} finally {
				if(destroy) {
					// Remove response-handler reference.
					editor.Response -= onSearchDialogResponse;
					editor.Destroy();
				}
			}
			*/
		}
	}
}

