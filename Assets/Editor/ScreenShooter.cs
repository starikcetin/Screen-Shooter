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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Borodar.ScreenShooter.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using GameViewSizeType = Borodar.ScreenShooter.Utils.GameViewUtil.GameViewSizeType;

namespace Borodar.ScreenShooter
{
    public class ScreenShooterWindow : EditorWindow
    {
        private static readonly List<ScreenshotData> listData;
        private static readonly ReorderableList list;

        private Camera _camera = Camera.main;

        private string _saveFolder = Application.dataPath +"/Screenshots";
        private string _fileName = "screenshot";
        private readonly string[] _fileTypes = {"PNG", "JPG"};
        private int _selectedType;

        //---------------------------------------------------------------------
        // Constructors
        //---------------------------------------------------------------------

        static ScreenShooterWindow()
        {
            listData = new List<ScreenshotData>
            {
                new ScreenshotData(800, 200),
                new ScreenshotData(200, 800),
                new ScreenshotData(1024, 768),
                new ScreenshotData(1920, 1080)
            };

            list = new ReorderableList(listData, typeof (ScreenshotData), true, true, true, true)
            {
                elementHeight = EditorGUIUtility.singleLineHeight + 4,
                drawElementCallback = (position, index, isActive, isFocused) =>
                {
                    var element = listData[index];

                    const float textWidth = 15;
                    var inputWidth = (position.width - textWidth) / 2;
                    
                    position.y += 2;
                    position.width = inputWidth;
                    position.height -= 4;
                    element.Width = EditorGUI.IntField(position, element.Width);

                    position.x += position.width;
                    position.width = textWidth;
                    EditorGUI.LabelField(position, "x");

                    position.x += position.width;
                    position.width = inputWidth;
                    element.Height = EditorGUI.IntField(position, element.Height);
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

            GUILayout.Label("Resolution", EditorStyles.boldLabel);
            list.DoLayoutList();
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

            GUILayout.Label("File Name", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _fileName = EditorGUILayout.TextField(_fileName, GUILayout.ExpandWidth(false));
            _selectedType = EditorGUILayout.Popup(_selectedType, _fileTypes, GUILayout.MaxWidth(55f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUI.backgroundColor = new Color(0.5f, 0.8f, 0.77f);
            if (GUILayout.Button("Take Screenshot"))
            {
                    EditorCoroutine.Start(TakeScreenshots());
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private IEnumerator TakeScreenshots()
        {
            foreach (var data in listData)
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

                TakeScreenshot(data.Width, data.Height, _saveFolder, _fileName);
            }
        }

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

            SaveTextureAsFile(scrTexture, folderName, fileName + "." + width + "x" + height, _selectedType);
        }

        private static void SaveTextureAsFile(Texture2D texture, string folder, string name, int type)
        {
            byte[] bytes;
            string extension;

            if (type > 0)
            {
                bytes = texture.EncodeToJPG();
                extension = ".jpg";
            }
            else
            {
                bytes = texture.EncodeToPNG();
                extension = ".png";
            }

            var imageFilePath = folder + "/" + name + extension;

            // ReSharper disable once PossibleNullReferenceException
            (new FileInfo(imageFilePath)).Directory.Create();
            File.WriteAllBytes(imageFilePath, bytes);

            Debug.Log("Image saved to: " + imageFilePath);
        }
    }
}