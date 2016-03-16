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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Borodar.ScreenShooter.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using GameViewSizeType = Borodar.ScreenShooter.Utils.GameViewUtil.GameViewSizeType;
using Format = Borodar.ScreenShooter.ScreenshotConfig.Format;

namespace Borodar.ScreenShooter
{    
    public class ScreenShooterWindow : EditorWindow
    {
        public const string RESOURCE_NAME = "ScreenShooterSettings";

        private static readonly string[] _fileTypes = { "PNG", "JPG" };

        private ScreenShooterSettings _settings;
        private ReorderableList _list;

        private bool _isMakingScreenshotsNow;
        private bool _hasErrors;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        [MenuItem("Window/Screen Shooter")]
        protected static void ShowWindow()
        {
            var window = (ScreenShooterWindow) GetWindow(typeof(ScreenShooterWindow));
            window.autoRepaintOnSceneChange = true;
            window.titleContent = new GUIContent("Screen Shooter");
            window.Show();
        }

        protected void OnEnable()
        {
            _settings = ScreenShooterSettings.Load();

            // Init reorderable list if required
            if (_list == null)
            {
                _list = new ReorderableList(_settings.ScreenshotConfigs, typeof(ScreenshotConfig), true, false, true, true)
                {
                    elementHeight = EditorGUIUtility.singleLineHeight + 4,
                    drawElementCallback = (position, index, isActive, isFocused) =>
                    {
                        const float textWidth = 12f;
                        const float dimensionWidth = 45f;
                        const float typeWidth = 45f;
                        const float space = 10f;

                        var element = _settings.ScreenshotConfigs[index];
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
                        element.Type = (Format)EditorGUI.Popup(position, (int)element.Type, _fileTypes);
                    }
                };
            }
        }

        protected void OnGUI()
        {
            _hasErrors = false;
            GUI.changed = false;
            GUI.enabled = !_isMakingScreenshotsNow;            

            // -- Camera ------------------------------------------------

            GUILayout.Label("Camera", EditorStyles.boldLabel);
            if (_settings.Camera == null) _settings.Camera = Camera.main;
            _settings.Camera = (Camera) EditorGUILayout.ObjectField(_settings.Camera, typeof (Camera), true);
            if (_settings.Camera == null)
            {
                EditorGUILayout.HelpBox("Camera is not selected.", MessageType.Error);
                _hasErrors = true;
            }
            EditorGUILayout.Space();

            // -- Screenshots -------------------------------------------

            GUILayout.Label("Screenshots", EditorStyles.boldLabel);
            _list.DoLayoutList();
            EditorGUILayout.Space();

            // -- Save Folder --------------------------------------------

            GUILayout.Label("Save To", EditorStyles.boldLabel);
            _settings.SaveFolder = EditorGUILayout.TextField(_settings.SaveFolder);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.enabled &= Directory.Exists(_settings.SaveFolder);
            if (GUILayout.Button("Show", GUILayout.ExpandWidth(false)))
            {
                Application.OpenURL("file://" + _settings.SaveFolder);
            }
            GUI.enabled = !_isMakingScreenshotsNow;

            if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
            {
                _settings.SaveFolder = EditorUtility.SaveFolderPanel("Save screenshots to:", _settings.SaveFolder, Application.dataPath);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // -- Take Button ---------------------------------------------

            GUI.enabled = !_hasErrors;
            GUI.backgroundColor = new Color(0.5f, 0.8f, 0.77f);
            if (GUILayout.Button("Take Screenshots"))
            {
                EditorCoroutine.Start(TakeScreenshots());
            }

            if (GUI.changed) EditorUtility.SetDirty(_settings);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
        private IEnumerator TakeScreenshots()
        {
            _isMakingScreenshotsNow = true;
            var currentIndex = GameViewUtil.GetCurrentSizeIndex();

            // Slow down and unpause editor if required
            var paused = EditorApplication.isPaused;
            var timeScale = Time.timeScale;            

            try
            {
                EditorApplication.isPaused = false;
                Time.timeScale = 0.001f;

                var configsCount = _settings.ScreenshotConfigs.Count;
                for (var i = 0; i < configsCount; i++)
                {
                    var data = _settings.ScreenshotConfigs[i];

                    // Show progress
                    var info = (i + 1) + " / " + configsCount + " - " + data.Name;
                    EditorUtility.DisplayProgressBar("Taking Screenshots", info, (float) (i + 1)/configsCount);

                    // apply custom resolution for game view
                    var sizeType = GameViewSizeType.FixedResolution;
                    var sizeGroupType = GameViewUtil.GetCurrentGroupType();
                    var sizeName = "scr_" + data.Width + "x" + data.Height;

                    if (!GameViewUtil.IsSizeExist(sizeGroupType, sizeName))
                    {
                        GameViewUtil.AddCustomSize(sizeType, sizeGroupType, data.Width, data.Height, sizeName);
                    }

                    var index = GameViewUtil.FindSizeIndex(sizeGroupType, sizeName);
                    GameViewUtil.SetSizeByIndex(index);

                    // add some delay while applying changes
                    var lastFrameTime = EditorApplication.timeSinceStartup;
                    while (EditorApplication.timeSinceStartup - lastFrameTime < 0.1f) yield return null;

                    ScreenshotUtil.TakeScreenshot(_settings.Camera, _settings.SaveFolder, data);

                    // just clean it up
                    GameViewUtil.RemoveCustomSize(sizeGroupType, index);
                }                
            }
            finally
            {
                // Restore pause state and time scale
                EditorApplication.isPaused = paused;
                Time.timeScale = timeScale;

                GameViewUtil.SetSizeByIndex(currentIndex);
                EditorUtility.ClearProgressBar();

                _isMakingScreenshotsNow = false;
            }                        
        }
    }
}