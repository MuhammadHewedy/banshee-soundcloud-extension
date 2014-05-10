using System;
using Mono.Unix;
using Banshee.Sources;

using Hyena.Json;

using Banshee.ServiceStack;
using Gtk;

namespace Banshee.SoundCloud
{
	/**
	 * ActionEntry Handler that shows a dialog where its OK button goes to `scResource` and handled by the callback `responseCallbackFunc`
	 */
	public abstract class ActionEntryHandler
	{
		protected PrimarySource primarySource;

		protected string dialogTitle;
		protected string dialogSubtitle1;
		protected string dialogSubtitle2;
		protected string dialogStock;

		protected String editorEntryVal;

		private string scResource;
		private Action<JsonArray> responseCallbackFunc;

		public ActionEntryHandler(PrimarySource primarySource, string dialogTitle, string dialogSubtitle1, string dialogSubtitle2,
		                          string dialogStock, String scResource)
		{
			this.primarySource = primarySource;

			this.dialogTitle = dialogTitle;
			this.dialogSubtitle1 = dialogSubtitle1;
			this.dialogSubtitle2 = dialogSubtitle2;
			this.dialogStock = dialogStock;
			this.scResource = scResource;
		}

		public void actionButtonClicked(object o, EventArgs args){
			BaseDialog editor = new BaseDialog(dialogTitle, dialogSubtitle1, dialogSubtitle2, dialogStock);
			editor.Response += onResponse;
			editor.Show();
		}

		private void onResponse(object o, ResponseArgs args)
		{
			BaseDialog editor = (BaseDialog)o;
			bool destroy = true;

			try {
				if(args.ResponseId == ResponseType.Ok) {
					if(String.IsNullOrEmpty(editorEntryVal = editor.Entry)) {
						destroy = false;
						editor.ErrorMessage = Catalog.GetString("Please provide owner name");
					} else {
						IO.MakeRequest(scResource, editor.Entry, responseCallbackFunc);
						destroy = true;
					}
				}
			} finally {
				if(destroy) {
					editor.Response -= onResponse;
					editor.Destroy();
				}
			}
		}

		/**
		 * should called by child classes to define the callback of the resource `scResource`
		 */
		protected void setOnResponseCallback(Action<JsonArray> responseCallbackFunc){
			this.responseCallbackFunc = responseCallbackFunc;
		}

	}
}
