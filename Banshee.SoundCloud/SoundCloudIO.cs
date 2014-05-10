/*
 * SoundCloudIO.cs
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

using Banshee.Collection.Database;

using Hyena;
using Hyena.Data;
using Hyena.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Banshee.SoundCloud
{
	public static class IO
	{
		public static void MakeRequest(string request, int data,  Action<JsonArray> func)
		{
			MakeRequest(request, data.ToString(), func);
		}

		public static void MakeRequest(string request, string data,  Action<JsonArray> func)
		{
			string url = "";
			
			switch(request)
			{
				// Search in Tracks, People and Groups.
				case SCResources.TRACKS:
					url = "http://api.soundcloud.com/tracks.json?q=" + data + "&client_id=" + SC.APPKEY;
					break;
				case SCResources.PEOPLE:
					url = "http://api.soundcloud.com/users.json?q=" + data + "&client_id=" + SC.APPKEY;
					break;
				case SCResources.GROUPS:
					url = "http://api.soundcloud.com/groups.json?q=" + data + "&client_id=" + SC.APPKEY;
					break;
				// Retrieve a user's info, sets and tracks
				case SCResources.GET_USER:
					url = "http://api.soundcloud.com/users/" + data + ".json?client_id=" + SC.APPKEY;
					break;
				case SCResources.GET_SETS:
					url = "http://api.soundcloud.com/playlists/" + data + ".json?client_id=" + SC.APPKEY;
					break;
				case SCResources.GET_ALL_TRACKS:
					url = "http://api.soundcloud.com/users/" + data + "/tracks.json?client_id=" + SC.APPKEY;
					break;
				default:
					throw new Exception("Invalid request");
			}
			ServerRequest(url, func);
		}

		static HttpWebRequest request; 
		private static void ServerRequest(String requestUrl, Action<JsonArray> func)
		{
			SC.log("reqeusting " + requestUrl);
			request = WebRequest.Create(requestUrl) as HttpWebRequest;

			request.BeginGetResponse((IAsyncResult result) => {
				HttpWebResponse response = request.EndGetResponse(result) as HttpWebResponse;

				if(response.StatusCode != HttpStatusCode.OK) {
					string s = String.Format("Server error(HTTP {0}: {1}).",
					                         response.StatusCode, response.StatusDescription);
					throw new Exception(s);
				}

				Stream			receiveStream = response.GetResponseStream();
				Deserializer	d = new Deserializer(receiveStream);
				object			jd = d.Deserialize();
				JsonArray		ja = jd as JsonArray;

				func(ja);
			
				response.Close();
				receiveStream.Close();

			}, null);
		}

		public static DatabaseTrackInfo makeTrackInfo(JsonObject o)
		{
			JsonObject user = (JsonObject)o["user"];
			DatabaseTrackInfo track = new DatabaseTrackInfo();
			
            track.Uri = new SafeUri(GetStreamURLFromTrack(o));
			track.TrackTitle = (string)o["title"];
			track.ArtistName = (string)user["username"];
			track.Comment =(string)o["description"];
			track.Genre =(string)o["genre"];
			track.Year = extractYear(o);
			return track;
		}
		
		private static int extractYear(JsonObject track)
		{
			string y = (string)track["created_at"];
			return Convert.ToInt32(y.Substring(0, 4));
		}
		
		private static string GetStreamURLFromTrack(JsonObject track)
		{
			string waveform_url =(string)track["waveform_url"];
			string url_id = waveform_url.Substring(21);		// chop off leading URL
			int end = url_id.IndexOf('_');
			url_id = url_id.Remove(end);
			return String.Format("http://media.soundcloud.com/stream/{0}?stream_token={1}", url_id, SC.STREAMKEY);
		}
	}
}
