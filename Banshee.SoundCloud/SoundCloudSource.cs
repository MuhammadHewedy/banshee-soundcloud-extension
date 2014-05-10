/*
 * SoundCloudSource.cs
 * 
 *
 * Copyright 2012 Paul Mackin
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using Mono.Unix;
using Gtk;

using Hyena;
using Hyena.Json;

using Banshee.Base;
using Banshee.Collection;
using Banshee.Collection.Database;
using Banshee.Configuration;
using Banshee.Gui;
using Banshee.PlaybackController;
using Banshee.ServiceStack;
using Banshee.Sources;
using Banshee.Sources.Gui;
using Banshee.Streaming;

using Banshee.SoundCloud;

namespace Banshee.SoundCloud
{
	public class SoundCloudSource : PrimarySource, IDisposable, IBasicPlaybackController
    {
        private uint ui_id;
		const int sort_order = 190;

		// list af ActionEntry used 
		private IList<ActionEntry> actionEntryList = null;

        public SoundCloudSource() : base(Catalog.GetString("SoundCloud"),
		                                 Catalog.GetString("SoundCloud"), "soundcloud", 52)
        {
			Properties.SetString("Icon.Name", "soundcloud");
            TypeUniqueId = "soundcloud";
            IsLocal = false;

            AfterInitialized();
			actionEntryList = getActionEntryList();

            InterfaceActionService uia_service = ServiceManager.Get<InterfaceActionService>();

			foreach (ActionEntry actionEntry in actionEntryList) {
				uia_service.GlobalActions.Add(actionEntry);
			}

            uia_service.GlobalActions["AddSoundCloudArtistAction"].IsImportant = false;

            ui_id = uia_service.UIManager.AddUiFromResource("GlobalUI.xml");

			initProperties();
           
            ServiceManager.PlayerEngine.TrackIntercept += OnPlayerEngineTrackIntercept;
            
            TrackEqualHandler = delegate(DatabaseTrackInfo a, TrackInfo b) {
				RadioTrackInfo r = b as RadioTrackInfo;
                return r != null && DatabaseTrackInfo.TrackEqual(
                    r.ParentTrack as DatabaseTrackInfo, a);
                   
            };
			
			/**
			 * TODO:
			 * 		Figure out how to tell Banshee where to find the track artwork. See below.
			 *
			TrackArtworkIdHandler = delegate(DatabaseTrackInfo a) {
				return;
			};
			*/
			SC.log("Initialized");
        }

		/**
		 * To add an action entry, you have to configure in both ActiveSourceUI.xml and GlobalUI.xml
		*/
		private IList<ActionEntry> getActionEntryList(){
			IList<ActionEntry> list = new List<ActionEntry>();
			list.Add(new ActionEntry("AddSoundCloudArtistAction", Stock.Add,
			                          Catalog.GetString("Add a SoundCloud Owner"), null,
			                          Catalog.GetString("Add a SoundCloud Owner"),
			                         new AddArtistHandler(this).actionButtonClicked));

			list.Add(new ActionEntry("SearchSoundCloudAction", Stock.Find,
			                         Catalog.GetString("Search"), null,
			                         Catalog.GetString("Search SoundCloud"),
			                         new SoundSearchHandler(this).actionButtonClicked));
			return list;
		}

		/*
		 * This may be the place to tell Banshee about artwork re: MetadataService
		 * 
		 *private void OnTrackInfoUpdated (Banshee.MediaEngine.PlayerEventArgs args)
        {
            RadioTrackInfo radio_track = ServiceManager.PlaybackController.CurrentTrack as RadioTrackInfo;
            if (radio_track != null) {
                Banshee.Metadata.MetadataService.Instance.Lookup (radio_track);
            }
        }*/


		private void initProperties(){
			Properties.SetString("ActiveSourceUIResource", "ActiveSourceUI.xml");
			Properties.Set<bool>("ActiveSourceUIResourcePropagate", true);
			Properties.Set<System.Reflection.Assembly>("ActiveSourceUIResource.Assembly",
			                                           typeof(SoundCloudSource).Assembly);

			Properties.SetString("GtkActionPath", "/SoundCloudContextMenu");
			Properties.Set<bool>("Nereid.SourceContentsPropagate", false);

			Properties.Set<ISourceContents>("Nereid.SourceContents",
			                                new LazyLoadSourceContents<SoundCloudSourceContents>());

			Properties.Set<string>("SearchEntryDescription", Catalog.GetString("Search your SoundCloud artists"));

			Properties.SetString("TrackView.ColumnControllerXml", String.Format(@"
                <column-controller>
                  <!--<column modify-default=""IndicatorColumn"">
                    <renderer type=""Banshee.Podcasting.Gui.ColumnCellPodcastStatusIndicator"" />
                  </column>-->
                  <add-default column=""IndicatorColumn"" />
                  <add-default column=""GenreColumn"" />
                  <column modify-default=""GenreColumn"">
                    <visible>false</visible>
                  </column>
                  <add-default column=""TitleColumn"" />
                  <column modify-default=""TitleColumn"">
                    <title>{0}</title>
                    <long-title>{0}</long-title>
                  </column>
                  <add-default column=""ArtistColumn"" />
                  <column modify-default=""ArtistColumn"">
                    <title>{1}</title>
                    <long-title>{1}</long-title>
                  </column>
				  <add-default column=""CommentColumn"" />
                  <column modify-default=""CommentColumn"">
                    <title>{2}</title>
                    <long-title>{2}</long-title>
                  </column>
				  <add-default column=""YearColumn"" />
                  <column modify-default=""YearColumn"">
                    <title>{3}</title>
                    <long-title>{3}</long-title>
                  </column>
                  <add-default column=""RatingColumn"" />
                  <add-default column=""PlayCountColumn"" />
                  <add-default column=""LastPlayedColumn"" />
                  <add-default column=""LastSkippedColumn"" />
                  <add-default column=""DateAddedColumn"" />
                  <add-default column=""UriColumn"" />
                  <sort-column direction=""asc"">artist</sort-column>
                </column-controller>",
			                                                                    Catalog.GetString("Track"),
			                                                                    Catalog.GetString("Artist"),
			                                                                    Catalog.GetString("Description"),
			                                                                    Catalog.GetString("Year")
			                                                                    ));
		}

        public override int Count { get { return 0; } }
		
		public override string GetPluralItemCountString(int count)
        {
            return Catalog.GetPluralString("{0} artist", "{0} artists", count);
        }
		
		/*
		 * TODO:
		 * 		Filter by Artist instead of by Genre. How do I do this?
		 */
		protected override IEnumerable<IFilterListModel> CreateFiltersFor(DatabaseSource src)
		{
			DatabaseArtistListModel artist_model = new DatabaseArtistListModel(src, src.DatabaseTrackModel, ServiceManager.DbConnection, src.UniqueId);
			//DatabaseQueryFilterModel<string> genre_model = new DatabaseQueryFilterModel<string>(src, src.DatabaseTrackModel, ServiceManager.DbConnection,
           //             Catalog.GetString("All Genre({0})"), src.UniqueId, Banshee.Query.BansheeQuery.GenreField , "Genre");
			
			if(this == src) {
				//this.genre_model = genre_model;
                this.artist_model = artist_model;
            }
			yield return artist_model;
			//yield return genre_model;
		}

        public override void Dispose()
        {
            base.Dispose();

            //ServiceManager.PlayerEngine.DisconnectEvent(OnTrackInfoUpdated);

            InterfaceActionService uia_service = ServiceManager.Get<InterfaceActionService>();
            if(uia_service == null) {
                return;
            }

            if(ui_id > 0) {
                uia_service.UIManager.RemoveUi(ui_id);

				foreach (ActionEntry actionEntry in actionEntryList) {
					uia_service.GlobalActions.Remove(actionEntry.name);
				}
                ui_id = 0;
            }

            ServiceManager.PlayerEngine.TrackIntercept -= OnPlayerEngineTrackIntercept;
        }
	
        private bool OnPlayerEngineTrackIntercept(TrackInfo track)
        {
            DatabaseTrackInfo t = track as DatabaseTrackInfo;
            if(t == null || t.PrimarySource != this) {
                return false;
            }

            new RadioTrackInfo(t).Play();

            return true;
        }
		
        #region IBasicPlaybackController implementation

        public bool First()
        {
            return false;
        }

        public bool Next(bool restart, bool changeImmediately)
        {
            if(!changeImmediately) {
                ServiceManager.PlayerEngine.SetNextTrack((SafeUri)null);
                if(ServiceManager.PlayerEngine.IsPlaying()) {
                    return true;
                }
            }
            RadioTrackInfo radio_track = ServiceManager.PlaybackController.CurrentTrack as RadioTrackInfo;
            if(radio_track != null && radio_track.PlayNextStream()) {
                return true;
            } else {
                return false;
            }
        }

        public bool Previous(bool restart)
        {
            RadioTrackInfo radio_track = ServiceManager.PlaybackController.CurrentTrack as RadioTrackInfo;
            if(radio_track != null && radio_track.PlayPreviousStream()) {
                return true;
            } else {
                return false;
            }
        }

        #endregion

        public override bool AcceptsInputFromSource(Source source)
        {
            return false;
        }

        public override bool CanDeleteTracks {
            get { return false; }
        }

        public override bool ShowBrowser {
            get { return true; }
        }

        public override bool CanRename {
            get { return false; }
        }

        protected override bool HasArtistAlbum {
            get { return true; }
        }

        public override bool HasViewableTrackProperties {
            get { return true; }
        }

        public override bool HasEditableTrackProperties {
            get { return false; }
        }
    }
}
