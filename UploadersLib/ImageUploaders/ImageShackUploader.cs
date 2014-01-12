﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (C) 2008-2014 ShareX Developers

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using HelpersLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Web;
using UploadersLib.HelperClasses;

namespace UploadersLib.ImageUploaders
{
    public sealed class ImageShackUploader : ImageUploader
    {
        private const string URLAPI = "https://api.imageshack.us/v1/";
        private const string URLAccessToken = URLAPI + "user/login";
        private const string URLUpload = URLAPI + "images";

        public ImageShackOptions Config { get; set; }

        private string APIKey;

        public ImageShackUploader(string developerKey, ImageShackOptions config)
        {
            APIKey = developerKey;
            Config = config;
        }

        public bool GetAccessToken(ImageShackOptions config)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("user", config.Username);
            args.Add("password", config.Password);

            string response = SendPostRequest(URLAccessToken, args);

            if (!string.IsNullOrEmpty(response))
            {
                ImageShackLoginResponse resp = JsonConvert.DeserializeObject<ImageShackLoginResponse>(response);

                if (resp != null && resp.result != null && !string.IsNullOrEmpty(resp.result.auth_token))
                {
                    config.Auth_token = resp.result.auth_token;
                    return true;
                }
            }

            return false;
        }

        public override UploadResult Upload(Stream stream, string fileName)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            arguments.Add("api_key", APIKey);

            if (!string.IsNullOrEmpty(Config.Auth_token))
            {
                arguments.Add("auth_token", Config.Auth_token);
            }

            arguments.Add("public", Config.IsPublic ? "y" : "n");

            UploadResult result = UploadData(stream, URLUpload, fileName, "file", arguments);

            if (!string.IsNullOrEmpty(result.Response))
            {
                ImageShackUploadResponse resp = JsonConvert.DeserializeObject<ImageShackUploadResponse>(result.Response);

                if (resp != null && resp.result != null && resp.result.images.Count > 0)
                {
                    result.URL = "http://" + resp.result.images[0].direct_link;
                    result.ThumbnailURL = string.Format("http://imagizer.imageshack.us/v2/{0}x{1}q90/{2}/{3}",
                              256, 0, resp.result.images[0].server, resp.result.images[0].filename);
                }
            }

            return result;
        }

        public class ImageShackLoginResponse
        {
            public bool success { get; set; }
            public int process_time { get; set; }
            public ImageShackLogin result { get; set; }
        }

        public class ImageShackLogin
        {
            public string auth_token { get; set; }
            public int user_id { get; set; }
            public string email { get; set; }
            public string username { get; set; }
            public ImageShackeUserAvatar avatar { get; set; }
            public string membership { get; set; }
            public string membership_item_number { get; set; }
            public string membership_cookie { get; set; }
        }

        public class ImageShackUser
        {
            public bool is_owner { get; set; }
            public int cache_version { get; set; }
            public string username { get; set; }
            public string description { get; set; }
            public int creation_date { get; set; }
            public string location { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public ImageShackeUserAvatar Avatar { get; set; }
        }

        public class ImageShackeUserAvatar
        {
            public int image_id { get; set; }
            public int server { get; set; }
            public string filename { get; set; }
        }

        public class ImageShackUploadResponse
        {
            public bool success { get; set; }
            public int process_time { get; set; }
            public ImageShackUploadResult result { get; set; }
        }

        public class ImageShackUploadResult
        {
            public int max_filesize { get; set; }
            public int space_limit { get; set; }
            public int space_used { get; set; }
            public int space_left { get; set; }
            public int passed { get; set; }
            public int failed { get; set; }
            public int total { get; set; }
            public List<ImageShackImage> images { get; set; }
        }

        public class ImageShackImage
        {
            public int id { get; set; }
            public int server { get; set; }
            public int bucket { get; set; }
            public string lp_hash { get; set; }
            public string filename { get; set; }
            public string original_filename { get; set; }
            public string direct_link { get; set; }
            public object title { get; set; }
            public object description { get; set; }
            public List<string> tags { get; set; }
            public int likes { get; set; }
            public bool liked { get; set; }
            public int views { get; set; }
            public int comments_count { get; set; }
            public bool comments_disabled { get; set; }
            public int filter { get; set; }
            public int filesize { get; set; }
            public int creation_date { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public bool @public { get; set; }
            public bool is_owner { get; set; }
            public ImageShackUser owner { get; set; }
            public List<ImageShackImage> next_images { get; set; }
            public List<ImageShackImage> prev_images { get; set; }
            public object related_images { get; set; }
        }
    }

    public class ImageShackOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsPublic { get; set; }
        public string Auth_token { get; set; }
    }
}