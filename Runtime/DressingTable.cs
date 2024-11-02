#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using com.github.pandrabox.pandravase.runtime;
using static com.github.pandrabox.pandravase.runtime.Util;
using static com.github.pandrabox.pandravase.runtime.Global;
using static com.github.pandrabox.dressingtable.runtime.Global;
using System.Linq;
using System;
using UnityEditor.PackageManager.UI;
using VRC.SDK3.Dynamics.Contact.Components;

namespace com.github.pandrabox.dressingtable.runtime
{
    public class DressingTable : MonoBehaviour, IEditorOnly
    {
        public Texture2D Tex;
        public LilTexBlendMode BlendMode;
        public string Path;
        public bool ViewSample;
        public bool CreateMenu;
        public VRCContactReceiver LinkContact;
    }

    [Serializable]
    public enum LilTexBlendMode
    {
        Normal,
        Add,
        Screen,
        Multiply
    }

    [CustomEditor(typeof(DressingTable))]
    public class DressingTableEditor : PandraEditor
    {
        bool someInput = false;
        DressingTarget DressingTarget;
        public DressingTableEditor() : base(true, PROJECTNAME, ProjectTypes.Asset)
        {
        }
        SerializedProperty _spTex, _spBlendMode, _spPath, _spViewSample, _spCreateMenu, _spLinkContact;

        protected override void DefineSerial()
        {
            _spTex = serializedObject.FindProperty(nameof(DressingTable.Tex));
            _spBlendMode = serializedObject.FindProperty(nameof(DressingTable.BlendMode));
            _spPath = serializedObject.FindProperty(nameof(DressingTable.Path));
            _spViewSample = serializedObject.FindProperty(nameof(DressingTable.ViewSample));
            _spCreateMenu = serializedObject.FindProperty(nameof(DressingTable.CreateMenu));
            _spLinkContact = serializedObject.FindProperty(nameof(DressingTable.LinkContact));
        }

        public override void OnInnerInspectorGUI()
        {
            Title("Config");

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(_spTex);
                    EditorGUILayout.PropertyField(_spBlendMode);
                    if (_spViewSample.boolValue) GUI.enabled = false;
                    EditorGUILayout.PropertyField(_spPath);
                    GUI.enabled = true;
                    EditorGUILayout.PropertyField(_spCreateMenu);
                    EditorGUILayout.PropertyField(_spLinkContact);
                }

                if (_spTex != null)
                {
                    var tex = _spTex.objectReferenceValue as Texture2D;
                    if (tex != null && !_spTex.hasMultipleDifferentValues)
                    {
                        var size = EditorGUIUtility.singleLineHeight * 5;
                        var margin = 4;
                        var withMargin = new Vector2(margin + size, margin + size);

                        var rect = GUILayoutUtility.GetRect(withMargin.x, withMargin.y, GUILayout.ExpandWidth(false),
                            GUILayout.ExpandHeight(true));
                        rect.x += margin;
                        rect.y = rect.y + rect.height / 2 - size / 2;
                        rect.width = size;
                        rect.height = size;

                        GUI.Box(rect, new GUIContent(), "flow node 0");
                        GUI.DrawTexture(rect, tex);
                    }
                }
            }


            Title("サンプル");
            EditorGUILayout.PropertyField(_spViewSample);
            serializedObject.ApplyModifiedProperties();
            someInput = EditorGUI.EndChangeCheck();
            if (_spViewSample.boolValue)
            {
                CreateSample();
            }
            else
            {
                RemoveSample();
            }


            Title("チェック");
            DressingTarget = new DressingTarget((DressingTable)target);
            var ok = true;
            if (DressingTarget.ErrorMsg != "")
            {
                EditorGUILayout.HelpBox($"エラーがあります。解決しないと本ギミックは動作しません。", MessageType.Error);
                EditorGUILayout.HelpBox($"{DressingTarget.ErrorMsg}", MessageType.Error);
                ok=false;
            }
            if (DressingTarget.WarningMsg != "")
            {
                EditorGUILayout.HelpBox($"ワーニングがあります。このままでも動く可能性はありますが、解決することでよりよくなります。", MessageType.Warning);
                EditorGUILayout.HelpBox($"{DressingTarget.WarningMsg}", MessageType.Warning);
                ok = false;
            }
            if(ok)
            {
                EditorGUILayout.HelpBox("DressingTableは適切に準備できています！", MessageType.Info);
            }
        }


        /// <summary>
        /// サンプル用のObjectを作成・表示・オプション変更・元の非表示等
        /// </summary>
        private void CreateSample()
        {
            if (someInput)
            {
                DressingTarget.CreateSample();
            }
        }

        /// <summary>
        /// サンプルオブジェクト削除
        /// </summary>
        private void RemoveSample()
        {
            if(DressingTarget!=null) DressingTarget.RemoveSample();
            _spViewSample.boolValue = false;
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Inspectorからフォーカスが外れた時の処理
        /// </summary>
        private void OnDisable()
        {
            RemoveSample();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
        protected override void OnInnerEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingEditMode)
            {
                // Playモードに入ったときやビルド時にRemoveSampleを呼び出す
                RemoveSample();
            }
        }
    }
}
#endif