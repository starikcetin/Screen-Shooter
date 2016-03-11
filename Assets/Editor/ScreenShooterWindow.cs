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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Borodar.ScreenShooter.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using GameViewSizeType = Borodar.ScreenShooter.Utils.GameViewUtil.GameViewSizeType;
using Format = Borodar.ScreenShooter.ScreenshotData.Format;

namespace Borodar.ScreenShooter
{
    public class ScreenShooterWindow : EditorWindow
    {
        private static readonly string[] _fileTypes = { "PNG", "JPG" };
        private static readonly List<ScreenshotData> _listData;
        private static readonly ReorderableList _list;        

        private Camera _camera = Camera.main;
        private string _saveFolder = Application.dataPath +"/Screenshots";

        //---------------------------------------------------------------------
        // Constructors
        //---------------------------------------------------------------------

        static ScreenShooterWindow()
        {
            _listData = new List<ScreenshotData>
            {
                new ScreenshotData("scr_sample", 800, 200, Format.PNG),
                new ScreenshotData("scr_sample_2", 200, 800, Format.PNG),
                new ScreenshotData("scr_sample_3", 1024, 768, Format.PNG),
                new ScreenshotData("scr_sample_4", 1920, 1080, Format.PNG)
            };

            _list = new ReorderableList(_listData, typeof (ScreenshotData), true, false, true, true)
            {                
                elementHeight = EditorGUIUtility.singleLineHeight + 4,
                drawElementCallback = (position, index, isActive, isFocused) =>
                {
                    const float textWidth = 12f;                    
                    const float dimensionWidth = 45f;
                    const float typeWidth = 45f;
                    const float space = 10f;

                    var element = _listData[index];
                    var nameWidth = position.width - space - textWidth - 2 * dimensionWidth - space - typeWidth;

                    position.y += 2;
                    position.width = nameWidth;
                    position.height -= 4;
                    element.Name = EditorGUI.TextField(position, element.Name);

                    position.x += position.width + space;
                    position.width = dimensionWidth;
                    element.Width = EditorGUI.IntField(position, element.Width);

                    position.x += position.width;
                    position.width = textWidth;
                    EditorGUI.LabelField(position, "x");

                    position.x += position.width;
                    position.width = dimensionWidth;
                    element.Height = EditorGUI.IntField(position, element.Height);

                    position.x += position.width + space;
                    position.width = typeWidth;
                    element.Type = (Format) EditorGUI.Popup(position, (int) element.Type, _fileTypes);
                }
            };

        }

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

            GUILayout.Label("Screenshots", EditorStyles.boldLabel);
            _list.DoLayoutList();
            EditorGUILayout.Space();

            GUILayout.Label("Save To", EditorStyles.boldLabel);
            _saveFolder = EditorGUILayout.TextField(_saveFolder);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.enabled = Directory.Exists(_saveFolder);
            if (GUILayout.Button("Show", GUILayout.ExpandWidth(false)))
            {
                Application.OpenURL("file://" + _saveFolder);
            }
            GUI.enabled = true;

            if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
            {
                _saveFolder = EditorUtility.SaveFolderPanel("Save screenshots to:", _saveFolder, Application.dataPath);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUI.backgroundColor = new Color(0.5f, 0.8f, 0.77f);
            if (GUILayout.Button("Take Screenshots"))
            {
                    EditorCoroutine.Start(TakeScreenshots());
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private IEnumerator TakeScreenshots()
        {
            foreach (var data in _listData)
            {
                var sizeType = GameViewSizeType.FixedResolution;
                var sizeGroupType = GameViewUtil.GetCurrentGroupType();
                var name = "scr: " + data.Width + "x" + data.Height;

                if (!GameViewUtil.IsSizeExist(sizeGroupType, name))
                {
                    GameViewUtil.AddCustomSize(sizeType, sizeGroupType, data.Width, data.Height, name);
                }

                var index = GameViewUtil.FindSizeIndex(sizeGroupType, name);
                GameViewUtil.SetSizeByIndex(index);

                // add some delay while applying changes
                var lastFrameTime = EditorApplication.timeSinceStartup;
                while (EditorApplication.timeSinceStartup - lastFrameTime < 0.1f) yield return null;

                TakeScreenshot(_saveFolder, data);
            }
        }

        private void TakeScreenshot(string folderName, ScreenshotData screenshotData)
        {
            var scrTexture = new Texture2D(screenshotData.Width, screenshotData.Height, TextureFormat.RGB24, false);
            var scrRenderTexture = new RenderTexture(scrTexture.width, scrTexture.height, 24);
            var camRenderTexture = _camera.targetTexture;

            _camera.targetTexture = scrRenderTexture;
            _camera.Render();
            _camera.targetTexture = camRenderTexture;

            RenderTexture.active = scrRenderTexture;
            scrTexture.ReadPixels(new Rect(0, 0, scrTexture.width, scrTexture.height), 0, 0);
            scrTexture.Apply();

            SaveTextureAsFile(scrTexture, folderName, screenshotData);
        }

        private static void SaveTextureAsFile(Texture2D texture, string folder, ScreenshotData screenshotData)
        {
            byte[] bytes;
            string extension;

            switch (screenshotData.Type)
            {
                case Format.PNG:
                    bytes = texture.EncodeToPNG();
                    extension = ".png";
                    break;
                case Format.JPG:
                    bytes = texture.EncodeToJPG();
                    extension = ".jpg";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var fileName = screenshotData.Name + "." + screenshotData.Width + "x" + screenshotData.Height;
            var imageFilePath = folder + "/" + fileName + extension;

            // ReSharper disable once PossibleNullReferenceException
            (new FileInfo(imageFilePath)).Directory.Create();
            File.WriteAllBytes(imageFilePath, bytes);

            Debug.Log("Image saved to: " + imageFilePath);
        }
    }
}