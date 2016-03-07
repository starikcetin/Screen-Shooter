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

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Borodar.ScreenShooter
{
    public class ScreenShooterWindow : EditorWindow
    {
        private Camera _camera = Camera.main;
        private int _width = Screen.width;
        private int _height = Screen.height;

        private string _saveFolder = "Assets/Screenshots";

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        [MenuItem("Window/Screen Shooter")]
        protected static void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            ScreenShooterWindow window = (ScreenShooterWindow) EditorWindow.GetWindow(typeof(ScreenShooterWindow));
            window.autoRepaintOnSceneChange = true;
            window.titleContent = new GUIContent("Screen Shooter");
            window.Show();
        }

        protected void OnGUI()
        {
            GUILayout.Label("Camera", EditorStyles.boldLabel);
            _camera = (Camera) EditorGUILayout.ObjectField(_camera, typeof (Camera), true);
            EditorGUILayout.Space();

            GUILayout.Label("Resolution", EditorStyles.boldLabel);
            _width = EditorGUILayout.IntField("Width", _width);
            _height = EditorGUILayout.IntField("Height", _height);
            EditorGUILayout.Space();

            GUILayout.Label("Save To", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(_saveFolder, GUILayout.ExpandWidth(false));

            if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
            {
                _saveFolder = EditorUtility.SaveFolderPanel("Save screenshots to:", _saveFolder, Application.dataPath);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button("Take Screenshot"))
            {
                TakeScreenshot(_width, _height, _saveFolder, "screenshot");
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void TakeScreenshot(int width, int height, string folderName, string fileName)
        {
            var scrTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
            var scrRenderTexture = new RenderTexture(scrTexture.width, scrTexture.height, 24);
            var camRenderTexture = _camera.targetTexture;

            _camera.targetTexture = scrRenderTexture;
            _camera.Render();
            _camera.targetTexture = camRenderTexture;

            RenderTexture.active = scrRenderTexture;
            scrTexture.ReadPixels(new Rect(0, 0, scrTexture.width, scrTexture.height), 0, 0);
            scrTexture.Apply();

            SaveTextureAsJPG(scrTexture, folderName, fileName);
        }

        private static void SaveTextureAsJPG(Texture2D texture, string folderName, string fileName)
        {
            var bytes = texture.EncodeToJPG();
            var imageFilePath = folderName + "/" + fileName + ".jpg";

            // ReSharper disable once PossibleNullReferenceException
            (new FileInfo(imageFilePath)).Directory.Create();
            File.WriteAllBytes(imageFilePath, bytes);

            Debug.Log("Image saved to: " + imageFilePath);
        }
    }
}