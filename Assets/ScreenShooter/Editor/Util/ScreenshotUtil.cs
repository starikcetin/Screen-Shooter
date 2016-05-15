/*
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */

using System;
using System.IO;
using Borodar.ScreenShooter.Configs;
using UnityEngine;

namespace Borodar.ScreenShooter.Utils
{
    public class ScreenshotUtil
    {
        public static void TakeScreenshot(ScreenShooterSettings settings, ScreenshotConfig config)
        {
            TakeScreenshot(settings.Camera, settings.SaveFolder, settings.Tag, config);
        }

        public static void TakeScreenshot(Camera camera, string folderName, string tag, ScreenshotConfig screenshotConfig)
        {
            var scrTexture = new Texture2D(screenshotConfig.Width, screenshotConfig.Height, TextureFormat.RGB24, false);
            var scrRenderTexture = new RenderTexture(scrTexture.width, scrTexture.height, 24);
            var camRenderTexture = camera.targetTexture;

            camera.targetTexture = scrRenderTexture;
            camera.Render();
            camera.targetTexture = camRenderTexture;

            RenderTexture.active = scrRenderTexture;
            scrTexture.ReadPixels(new Rect(0, 0, scrTexture.width, scrTexture.height), 0, 0);
            scrTexture.Apply();

            SaveTextureAsFile(scrTexture, folderName, tag, screenshotConfig);
        }

        public static void SaveTextureAsFile(Texture2D texture, string folder, string tag, ScreenshotConfig screenshotConfig)
        {
            byte[] bytes;
            string extension;

            switch (screenshotConfig.Type)
            {
                case ScreenshotConfig.Format.PNG:
                    bytes = texture.EncodeToPNG();
                    extension = ".png";
                    break;
                case ScreenshotConfig.Format.JPG:
                    bytes = texture.EncodeToJPG();
                    extension = ".jpg";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var fileName = tag + screenshotConfig.Name + "." + screenshotConfig.Width + "x" + screenshotConfig.Height;
            var imageFilePath = screenshotConfig.AppendTimestamp 
                ? BuildFilePath(folder, fileName, DateTime.Now, extension)
                : BuildFilePath(folder, fileName, extension)
                ;

            // ReSharper disable once PossibleNullReferenceException
            (new FileInfo(imageFilePath)).Directory.Create();
            File.WriteAllBytes(imageFilePath, bytes);

            Debug.Log("Image saved to: " + imageFilePath);
        }

        private static string BuildFilePath(string folder, string fileName, string extension)
        {
            return folder + "/" + MakeValidFileName(fileName + extension);
        }

        private static string BuildFilePath(string folder, string fileName, DateTime time, string extension)
        {
            // Create timestamp with year, month, day, hours, minutes, seconds, and milliseconds
            // See https://msdn.microsoft.com/en-us/library/8kb3ddd4%28v=vs.110%29.aspx
            var formatString = "yyyyMMddHHmmssfff";
            var timeStamp = time.ToString(formatString);
                        
            return BuildFilePath(folder, fileName + "." + timeStamp, extension);
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }
    }
}