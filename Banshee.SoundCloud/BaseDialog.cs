using System;
using Gtk;
using Mono.Unix;

using Banshee.ServiceStack;
using Banshee.Collection.Database;

using Hyena.Widgets;

namespace Banshee.SoundCloud
{
	public class BaseDialog : Gtk.Dialog
	{
		private Button				save_button;
		private Entry				entry;
		private Alignment			error_container;
		private Label				error;

		Table table;

		public BaseDialog(string mainTitle, string subTitle, string textTitle, string okBtnStock) : base()
		{
			string title = Catalog.GetString(mainTitle);

			AccelGroup accel_group = new AccelGroup();
			AddAccelGroup(accel_group);

			Title = title;
			SkipTaskbarHint = true;
			Modal = true;
			BorderWidth = 6;
			HasSeparator = false;
			DefaultResponse = ResponseType.Ok;
			Modal = true;

			VBox.Spacing = 6;

			HBox split_box = new HBox();
			split_box.Spacing = 12;
			split_box.BorderWidth = 6;

			/* TODO:
			 * 		Get this bloody image to display.
            Image image = new Image("soundcloud");
            image.IconSize =(int)IconSize.Dialog;
            image.IconName = "soundcloudd";
            image.Yalign = 0.0f;
            image.Show();
            */

			VBox main_box = new VBox();
			main_box.BorderWidth = 5;
			main_box.Spacing = 10;

			Label header = new Label();
			header.Markup = String.Format("<big><b>{0}</b></big>", GLib.Markup.EscapeText(title));
			header.Xalign = 0.0f;
			header.Show();

			Label message = new Label();
			message.Text = Catalog.GetString(subTitle);
			message.Xalign = 0.0f;
			message.Wrap = true;
			message.Show();

			table = new Table(5, 2, false);
			table.RowSpacing = 6;
			table.ColumnSpacing = 6;

			entry = AddEntryRow(Catalog.GetString(textTitle));

			table.ShowAll();

			main_box.PackStart(header, false, false, 0);
			main_box.PackStart(message, false, false, 0);
			main_box.PackStart(table, false, false, 0);
			main_box.Show();

			//split_box.PackStart(image, false, false, 0);
			split_box.PackStart(main_box, true, true, 0);
			split_box.Show();

			VBox.PackStart(split_box, true, true, 0);

			Button cancel_button = new Button(Stock.Cancel);
			cancel_button.CanDefault = false;
			cancel_button.UseStock = true;
			cancel_button.Show();
			AddActionWidget(cancel_button, ResponseType.Close);

			cancel_button.AddAccelerator("activate", accel_group,(uint)Gdk.Key.Escape,
			                             0, Gtk.AccelFlags.Visible);

			save_button = new Button(okBtnStock);
			save_button.CanDefault = true;
			save_button.UseStock = true;
			save_button.Sensitive = false;
			save_button.Show();
			AddActionWidget(save_button, ResponseType.Ok);

			save_button.AddAccelerator("activate", accel_group,(uint)Gdk.Key.Return,
			                           0, Gtk.AccelFlags.Visible);

			entry.HasFocus = true;

			error_container = new Alignment(0.0f, 0.0f, 1.0f, 1.0f);
			error_container.TopPadding = 6;
			HBox error_box = new HBox();
			error_box.Spacing = 4;

			Image error_image = new Image();
			error_image.Stock = Stock.DialogError;
			error_image.IconSize =(int)IconSize.Menu;
			error_image.Show();

			error = new Label();
			error.Xalign = 0.0f;
			error.Show();

			error_box.PackStart(error_image, false, false, 0);
			error_box.PackStart(error, true, true, 0);
			error_box.Show();

			error_container.Add(error_box);

			table.Attach(error_container, 0, 2, 6, 7, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Shrink, 0, 0);

			entry.Changed += OnFieldsChanged;

			OnFieldsChanged(this, EventArgs.Empty);
		}

		private Entry AddEntryRow(string title)
		{
			Entry entry = new Entry();
			AddRow(title, entry);
			return entry;
		}

		private uint row = 0;
		private void AddRow(string title, Widget entry)
		{
			Label label = new Label(title);
			label.Xalign = 0.0f;

			table.Attach(label, 0, 1, row, row + 1, AttachOptions.Fill, AttachOptions.Fill | AttachOptions.Expand, 0, 0);
			table.Attach(entry, 1, 2, row, row + 1, AttachOptions.Fill | AttachOptions.Expand, AttachOptions.Shrink, 0, 0);
			row++;
		}

		private void OnFieldsChanged(object o, EventArgs args)
		{
			// When the button becomes sensitive it can be executed.
			save_button.Sensitive = entry.Text.Trim().Length > 0;
		}

		public void FocusUri()
		{
			entry.HasFocus = true;
			entry.SelectRegion(0, entry.Text.Length);
		}

		public string Entry {
			get { return entry.Text.Trim(); }
		}

		public string ErrorMessage {
			set {
				if(value == null) {
					error_container.Hide();
				} else {
					error.Text = value;
					error_container.Show();
				}
			}
		}
	}
}

